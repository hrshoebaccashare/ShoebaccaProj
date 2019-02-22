using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using PX.Common;
using PX.Data;
using PX.Objects.CS;
using PX.Objects.CR;


namespace ShoebaccaProj
{
    public class CRCustomAttributeList<TEntity> : PXSelectBase<CSAnswers>
    {
        private readonly EntityHelper _helper;

        public CRCustomAttributeList(PXGraph graph)
        {
            _Graph = graph;
            _helper = new EntityHelper(graph);
            View = new PXView(graph, false,
                new Select3<CSAnswers, OrderBy<Asc<CSAnswers.order>>>(),
                new PXSelectDelegate(SelectDelegate));

            _Graph.EnsureCachePersistence(typeof(CSAnswers));
            PXDBAttributeAttribute.Activate(_Graph.Caches[typeof(TEntity)]);
            _Graph.FieldUpdating.AddHandler<CSAnswers.value>(FieldUpdatingHandler);
            _Graph.FieldSelecting.AddHandler<CSAnswers.value>(FieldSelectingHandler);
            _Graph.FieldSelecting.AddHandler<CSAnswers.isRequired>(IsRequiredSelectingHandler);
            _Graph.FieldSelecting.AddHandler<CSAnswers.attributeID>(AttrFieldSelectingHandler);
            _Graph.RowPersisting.AddHandler<CSAnswers>(RowPersistingHandler);
            _Graph.RowPersisting.AddHandler<TEntity>(ReferenceRowPersistingHandler);
            _Graph.RowUpdating.AddHandler<TEntity>(ReferenceRowUpdatingHandler);
            _Graph.RowDeleted.AddHandler<TEntity>(ReferenceRowDeletedHandler);
            _Graph.RowInserted.AddHandler<TEntity>(RowInsertedHandler);
        }

        virtual protected IEnumerable SelectDelegate()
        {
            var currentObject = _Graph.Caches[typeof(TEntity)].Current;

            // var row = GetCurrentRow();

            foreach (CSAnswers item in SelectInternal(currentObject))
            {
                var attr = CRAttribute.Attributes[item.AttributeID];
                CRAttribute.Attribute duplicateAttr = attr;
                duplicateAttr.Values.Clear();

                if (PXSiteMap.IsPortal && duplicateAttr != null && duplicateAttr.IsInternal)
                    continue;
                yield return item;
            }
        }

        //protected virtual IEnumerable SelectDelegate()
        //{
        //    var currentObject = _Graph.Caches[typeof(TEntity)].Current;
        //    return SelectInternal(currentObject);
        //}

        protected Guid? GetNoteId(object row)
        {
            return _helper.GetEntityNoteID(row);
        }

        private Type GetClassIdField(object row)
        {
            if (row == null)
                return null;


            var fieldAttribute =
                _Graph.Caches[row.GetType()].GetAttributes(row, null)
                    .OfType<CRAttributesFieldAttribute>()
                    .FirstOrDefault();

            if (fieldAttribute == null)
                return null;

            return fieldAttribute.ClassIdField;
        }

        private Type GetEntityTypeFromAttribute(object row)
        {
            var classIdField = GetClassIdField(row);
            if (classIdField == null)
                return null;

            return classIdField.DeclaringType;
        }

        private string GetClassId(object row)
        {
            var classIdField = GetClassIdField(row);
            if (classIdField == null)
                return null;

            var entityCache = _Graph.Caches[row.GetType()];

            var classIdValue = entityCache.GetValue(row, classIdField.Name);

            return classIdValue == null ? null : classIdValue.ToString();
        }

        protected IEnumerable<CSAnswers> SelectInternal(object row)
        {
            if (row == null)
                yield break;

            var noteId = GetNoteId(row);

            if (!noteId.HasValue)
                yield break;

            var answerCache = _Graph.Caches[typeof(CSAnswers)];
            var entityCache = _Graph.Caches[row.GetType()];

            List<CSAnswers> answerList;

            var status = entityCache.GetStatus(row);

            if (status == PXEntryStatus.Inserted || status == PXEntryStatus.InsertedDeleted)
            {
                answerList = answerCache.Inserted.Cast<CSAnswers>().Where(x => x.RefNoteID == noteId).ToList();
            }
            else
            {
                answerList = PXSelect<CSAnswers, Where<CSAnswers.refNoteID, Equal<Required<CSAnswers.refNoteID>>>>
                    .Select(_Graph, noteId).FirstTableItems.ToList();
            }

            var classId = GetClassId(row);

            CRAttribute.ClassAttributeList classAttributeList = new CRAttribute.ClassAttributeList();

            if (classId != null)
            {
                classAttributeList = CRAttribute.EntityAttributes(GetEntityTypeFromAttribute(row), classId);
            }
            //when coming from Import scenarios there might be attributes which don't belong to entity's current attribute class or the entity might not have any attribute class at all
            if (_Graph.IsImport && PXView.SortColumns.Any() && PXView.Searches.Any())
            {
                var columnIndex = Array.FindIndex(PXView.SortColumns,
                    x => x.Equals(typeof(CSAnswers.attributeID).Name, StringComparison.OrdinalIgnoreCase));

                if (columnIndex >= 0 && columnIndex < PXView.Searches.Length)
                {
                    var searchValue = PXView.Searches[columnIndex];

                    if (searchValue != null)
                    {
                        //searchValue can be either AttributeId or Description
                        var attributeDefinition = CRAttribute.Attributes[searchValue.ToString()] ??
                                             CRAttribute.AttributesByDescr[searchValue.ToString()];

                        if (attributeDefinition == null)
                        {
                            throw new PXSetPropertyException(PX.Objects.CR.Messages.AttributeNotValid);
                        }
                        //avoid duplicates
                        else if (classAttributeList[attributeDefinition.ToString()] == null)
                        {
                            classAttributeList.Add(new CRAttribute.AttributeExt(attributeDefinition, null, false));
                        }
                    }
                }
            }

            if (answerList.Count == 0 && classAttributeList.Count == 0)
                yield break;

            //attribute identifiers that are contained in CSAnswers cache/table but not in class attribute list
            List<string> attributeIdListAnswers =
                answerList.Select(x => x.AttributeID)
                    .Except(classAttributeList.Select(x => x.ID))
                    .Distinct()
                    .ToList();

            //attribute identifiers that are contained in class attribute list but not in CSAnswers cache/table
            List<string> attributeIdListClass =
                classAttributeList.Select(x => x.ID)
                    .Except(answerList.Select(x => x.AttributeID))
                    .ToList();

            //attribute identifiers which belong to both lists
            List<string> attributeIdListIntersection =
                classAttributeList.Select(x => x.ID)
                    .Intersect(answerList.Select(x => x.AttributeID))
                    .Distinct()
                    .ToList();


            var cacheIsDirty = answerCache.IsDirty;

            List<CSAnswers> output = new List<CSAnswers>();

            //attributes contained only in CSAnswers cache/table should be added "as is"
            output.AddRange(answerList.Where(x => attributeIdListAnswers.Contains(x.AttributeID)));

            //attributes contained only in class attribute list should be created and initialized with default value
            foreach (var attributeId in attributeIdListClass)
            {
                var classAttributeDefinition = classAttributeList[attributeId];

                if (PXSiteMap.IsPortal && classAttributeDefinition.IsInternal)
                    continue;

                CSAnswers answer = (CSAnswers)answerCache.CreateInstance();
                answer.AttributeID = classAttributeDefinition.ID;
                answer.RefNoteID = noteId;
                answer.Value = GetDefaultAnswerValue(classAttributeDefinition);
                if (classAttributeDefinition.ControlType == CSAttribute.CheckBox)
                {
                    bool value;
                    if (bool.TryParse(answer.Value, out value))
                        answer.Value = Convert.ToInt32(value).ToString(CultureInfo.InvariantCulture);
                    else if (answer.Value == null)
                        answer.Value = 0.ToString();
                }

                answer.IsRequired = classAttributeDefinition.Required;
                answer = (CSAnswers)(answerCache.Insert(answer) ?? answerCache.Locate(answer));
                output.Add(answer);
            }

            //attributes belonging to both lists should be selected from CSAnswers cache/table with and additional IsRequired check against class definition
            foreach (CSAnswers answer in answerList.Where(x => attributeIdListIntersection.Contains(x.AttributeID)).ToList())
            {
                var classAttributeDefinition = classAttributeList[answer.AttributeID];

                if (PXSiteMap.IsPortal && classAttributeDefinition.IsInternal)
                    continue;

                if (answer.Value == null && classAttributeDefinition.ControlType == CSAttribute.CheckBox)
                    answer.Value = bool.FalseString;

                if (answer.IsRequired == null || classAttributeDefinition.Required != answer.IsRequired)
                {
                    answer.IsRequired = classAttributeDefinition.Required;

                    var fieldState = View.Cache.GetValueExt<CSAnswers.isRequired>(answer) as PXFieldState;
                    var fieldValue = fieldState != null && ((bool?)fieldState.Value).GetValueOrDefault();

                    answer.IsRequired = classAttributeDefinition.Required || fieldValue;
                }



                output.Add(answer);
            }

            answerCache.IsDirty = cacheIsDirty;

            output =
                output.OrderBy(
                    x =>
                        classAttributeList.Contains(x.AttributeID)
                            ? classAttributeList.IndexOf(x.AttributeID)
                            : (x.Order ?? 0))
                    .ThenBy(x => x.AttributeID)
                    .ToList();

            short attributeOrder = 0;

            foreach (CSAnswers answer in output)
            {
                answer.Order = attributeOrder++;
                yield return answer;
            }
        }

        private void FieldUpdatingHandler(PXCache sender, PXFieldUpdatingEventArgs e)
        {
            var row = e.Row as CSAnswers;


            if (row == null || !(e.NewValue is string) || row.AttributeID == null)
                return;

            var attr = CRAttribute.Attributes[row.AttributeID];
            if (attr == null)
                return;

            var newValue = (string)e.NewValue;
            switch (attr.ControlType)
            {
                case CSAttribute.CheckBox:
                    bool value;
                    if (bool.TryParse(newValue, out value))
                    {
                        e.NewValue = Convert.ToInt32(value).ToString(CultureInfo.InvariantCulture);
                    }
                    break;
                case CSAttribute.Datetime:
                    DateTime dt;
                    if (sender.Graph.IsMobile)
                    {
                        newValue = newValue.Replace("Z", "");
                        if (DateTime.TryParse(newValue, CultureInfo.InvariantCulture, DateTimeStyles.None, out dt))
                        {
                            e.NewValue = dt.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture);
                        }
                    }
                    else
                    {
                        if (DateTime.TryParse(newValue, CultureInfo.InvariantCulture, DateTimeStyles.None, out dt))
                        {
                            e.NewValue = dt.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture);
                        }
                    }
                    break;
            }
        }

        private void FieldSelectingHandler(PXCache sender, PXFieldSelectingEventArgs e)
        {
            var row = e.Row as CSAnswers;
            if (row == null) return;

            var question = CRAttribute.Attributes[row.AttributeID];

            var options = question != null ? question.Values : null;

            var required = row.IsRequired == true ? 1 : -1;

            if (options != null && options.Count > 0)
            {
                //ComboBox:
                var allowedValues = new List<string>();
                var allowedLabels = new List<string>();

                foreach (var option in options)
                {
                    if (option.Disabled && row.Value != option.ValueID) continue;

                    allowedValues.Add(option.ValueID);
                    allowedLabels.Add(option.Description);
                }

                e.ReturnState = PXStringState.CreateInstance(e.ReturnState, CSAttributeDetail.ParameterIdLength,
                    true, typeof(CSAnswers.value).Name, false, required, question.EntryMask, allowedValues.ToArray(),
                    allowedLabels.ToArray(), true, null);
                if (question.ControlType == CSAttribute.MultiSelectCombo)
                {
                    ((PXStringState)e.ReturnState).MultiSelect = true;
                }
            }
            else if (question != null)
            {
                if (question.ControlType == CSAttribute.CheckBox)
                {
                    e.ReturnState = PXFieldState.CreateInstance(e.ReturnState, typeof(bool), false, false, required,
                        null, null, false, typeof(CSAnswers.value).Name, null, null, null, PXErrorLevel.Undefined, true,
                        true, null,
                        PXUIVisibility.Visible, null, null, null);
                    if (e.ReturnValue is string)
                    {
                        int value;
                        if (int.TryParse((string)e.ReturnValue, NumberStyles.Integer, CultureInfo.InvariantCulture,
                            out value))
                        {
                            e.ReturnValue = Convert.ToBoolean(value);
                        }
                    }
                }
                else if (question.ControlType == CSAttribute.Datetime)
                {
                    e.ReturnState = PXDateState.CreateInstance(e.ReturnState, typeof(CSAnswers.value).Name, false,
                        required, question.EntryMask, question.EntryMask,
                        null, null);
                }
                else
                {
                    //TextBox:					
                    var vstate = sender.GetStateExt<CSAnswers.value>(null) as PXStringState;
                    e.ReturnState = PXStringState.CreateInstance(e.ReturnState, vstate.With(_ => _.Length), null,
                        typeof(CSAnswers.value).Name,
                        false, required, question.EntryMask, null, null, true, null);
                }
            }
            if (e.ReturnState != null)
            {
                var error = PXUIFieldAttribute.GetError<CSAnswers.value>(sender, row);
                if (error != null)
                {
                    var state = (PXFieldState)e.ReturnState;
                    state.Error = error;
                    state.ErrorLevel = PXErrorLevel.RowError;
                }
            }
        }

        private void AttrFieldSelectingHandler(PXCache sender, PXFieldSelectingEventArgs e)
        {
            PXUIFieldAttribute.SetEnabled<CSAnswers.attributeID>(sender, e.Row, false);
        }

        private void IsRequiredSelectingHandler(PXCache sender, PXFieldSelectingEventArgs e)
        {
            var row = e.Row as CSAnswers;
            var current = sender.Graph.Caches[typeof(TEntity)].Current;

            if (row == null || current == null)
                return;
            var currentNoteId = GetNoteId(current);

            if (e.ReturnValue != null || row.RefNoteID != currentNoteId)
                return;

            //when importing data - make all attributes nonrequired (otherwise import might fail)
            if (sender.Graph.IsImport)
            {
                e.ReturnValue = false;
                return;
            }

            var currentClassId = GetClassId(current);

            var attribute = CRAttribute.EntityAttributes(GetEntityTypeFromAttribute(current), currentClassId)[row.AttributeID];

            if (attribute == null)
            {
                e.ReturnValue = false;
            }
            else
            {
                if (PXSiteMap.IsPortal && attribute.IsInternal)
                    e.ReturnValue = false;
                else
                    e.ReturnValue = attribute.Required;
            }
        }

        private void RowPersistingHandler(PXCache sender, PXRowPersistingEventArgs e)
        {
            if (e.Operation != PXDBOperation.Insert && e.Operation != PXDBOperation.Update) return;

            var row = e.Row as CSAnswers;
            if (row == null) return;

            if (!row.RefNoteID.HasValue)
            {
                e.Cancel = true;
                RowPersistDeleted(sender, row);
            }
            else if (string.IsNullOrEmpty(row.Value))
            {
                var mayNotBeEmpty = PXMessages.LocalizeFormatNoPrefix(ErrorMessages.FieldIsEmpty,
                    sender.GetStateExt<CSAnswers.value>(null).With(_ => _ as PXFieldState).With(_ => _.DisplayName));
                if (row.IsRequired == true &&
                    sender.RaiseExceptionHandling<CSAnswers.value>(e.Row, row.Value,
                        new PXSetPropertyException(mayNotBeEmpty, PXErrorLevel.RowError, typeof(CSAnswers.value).Name)))
                {
                    throw new PXRowPersistingException(typeof(CSAnswers.value).Name, row.Value, mayNotBeEmpty,
                        typeof(CSAnswers.value).Name);
                }
                e.Cancel = true;
                if (sender.GetStatus(row) != PXEntryStatus.Inserted)
                    RowPersistDeleted(sender, row);
            }
        }

        private void RowPersistDeleted(PXCache cache, object row)
        {
            try
            {
                cache.PersistDeleted(row);
                cache.SetStatus(row, PXEntryStatus.InsertedDeleted);
            }
            catch (PXLockViolationException)
            {
            }
            cache.ResetPersisted(row);
        }
        private void ReferenceRowDeletedHandler(PXCache sender, PXRowDeletedEventArgs e)
        {
            object row = e.Row;
            if (row == null) return;

            var noteId = GetNoteId(row);

            if (!noteId.HasValue) return;

            var answerCache = _Graph.Caches[typeof(CSAnswers)];
            var entityCache = _Graph.Caches[row.GetType()];

            List<CSAnswers> answerList;

            var status = entityCache.GetStatus(row);

            if (status == PXEntryStatus.Inserted || status == PXEntryStatus.InsertedDeleted)
            {
                answerList = answerCache.Inserted.Cast<CSAnswers>().Where(x => x.RefNoteID == noteId).ToList();
            }
            else
            {
                answerList = PXSelect<CSAnswers, Where<CSAnswers.refNoteID, Equal<Required<CSAnswers.refNoteID>>>>
                    .Select(_Graph, noteId).FirstTableItems.ToList();
            }
            foreach (var answer in answerList)
                this.Cache.Delete(answer);
        }

        private void ReferenceRowPersistingHandler(PXCache sender, PXRowPersistingEventArgs e)
        {
            var row = e.Row;

            if (row == null) return;

            var answersCache = _Graph.Caches[typeof(CSAnswers)];

            var emptyRequired = new List<string>();
            foreach (CSAnswers answer in answersCache.Cached)
            {
                if (answer.IsRequired == null)
                {
                    var state = View.Cache.GetValueExt<CSAnswers.isRequired>(answer) as PXFieldState;
                    if (state != null)
                        answer.IsRequired = state.Value as bool?;
                }

                if (e.Operation == PXDBOperation.Delete)
                {
                    answersCache.Delete(answer);
                }
                else if (string.IsNullOrEmpty(answer.Value) && answer.IsRequired == true && !_Graph.UnattendedMode)
                {
                    var displayName = "";

                    var attributeDefinition = CRAttribute.Attributes[answer.AttributeID];
                    if (attributeDefinition != null)
                        displayName = attributeDefinition.Description;

                    emptyRequired.Add(displayName);
                    var mayNotBeEmpty = PXMessages.LocalizeFormatNoPrefix(ErrorMessages.FieldIsEmpty, displayName);
                    answersCache.RaiseExceptionHandling<CSAnswers.value>(answer, answer.Value,
                        new PXSetPropertyException(mayNotBeEmpty, PXErrorLevel.RowError, typeof(CSAnswers.value).Name));
                    PXUIFieldAttribute.SetError<CSAnswers.value>(answersCache, answer, mayNotBeEmpty);
                }
            }
            if (emptyRequired.Count > 0)
                throw new PXException(PX.Objects.CR.Messages.RequiredAttributesAreEmpty,
                    string.Join(", ", emptyRequired.Select(s => string.Format("'{0}'", s))));
        }

        private void ReferenceRowUpdatingHandler(PXCache sender, PXRowUpdatingEventArgs e)
        {
            var row = e.Row;
            var newRow = e.NewRow;

            if (row == null || newRow == null)
                return;

            var rowNoteId = GetNoteId(row);

            var rowClassId = GetClassId(row);
            var newRowClassId = GetClassId(newRow);

            if (string.Equals(rowClassId, newRowClassId, StringComparison.InvariantCultureIgnoreCase))
                return;

            var newAttrList = new HashSet<string>();

            if (newRowClassId != null)
            {
                foreach (var attr in CRAttribute.EntityAttributes(GetEntityTypeFromAttribute(newRow), newRowClassId))
                {
                    newAttrList.Add(attr.ID);
                }
            }
            var relatedEntityTypes =
                sender.GetAttributesOfType<CRAttributesFieldAttribute>(newRow, null).FirstOrDefault()?.RelatedEntityTypes;

            PXGraph entityGraph = new PXGraph();
            var entityHelper = new EntityHelper(entityGraph);

            if (relatedEntityTypes != null)
                foreach (var classField in relatedEntityTypes)
                {
                    object entity = entityHelper.GetEntityRow(classField.DeclaringType, rowNoteId);
                    if (entity == null) continue;
                    string entityClass = (string)entityGraph.Caches[classField.DeclaringType].GetValue(entity, classField.Name);
                    if (entityClass == null) continue;
                    CRAttribute.EntityAttributes(classField.DeclaringType, entityClass)
                        .Where(x => !newAttrList.Contains(x.ID)).Select(x => x.ID)
                        .ForEach(x => newAttrList.Add(x));
                }

            foreach (CSAnswers answersRow in
                PXSelect<CSAnswers,
                    Where<CSAnswers.refNoteID, Equal<Required<CSAnswers.refNoteID>>>>
                    .SelectMultiBound(sender.Graph, null, rowNoteId))
            {
                var copy = PXCache<CSAnswers>.CreateCopy(answersRow);
                View.Cache.Delete(answersRow);
                if (newAttrList.Contains(copy.AttributeID))
                {
                    View.Cache.Insert(copy);
                }
            }

            if (newRowClassId != null)
                SelectInternal(newRow).ToList();

            sender.IsDirty = true;
        }

        private void RowInsertedHandler(PXCache sender, PXRowInsertedEventArgs e)
        {
            if (sender != null && sender.Graph != null && !sender.Graph.IsImport)
                SelectInternal(e.Row).ToList();
        }

        private void CopyAttributes(object destination, object source, bool copyall)
        {
            if (destination == null || source == null) return;

            var sourceAttributes = SelectInternal(source).RowCast<CSAnswers>().ToList();
            var targetAttributes = SelectInternal(destination).RowCast<CSAnswers>().ToList();

            var answerCache = _Graph.Caches<CSAnswers>();


            foreach (var targetAttribute in targetAttributes)
            {
                var sourceAttr = sourceAttributes.SingleOrDefault(x => x.AttributeID == targetAttribute.AttributeID);

                if (sourceAttr == null || string.IsNullOrEmpty(sourceAttr.Value) ||
                    sourceAttr.Value == targetAttribute.Value)
                    continue;

                if (string.IsNullOrEmpty(targetAttribute.Value) || copyall)
                {
                    var answer = PXCache<CSAnswers>.CreateCopy(targetAttribute);
                    answer.Value = sourceAttr.Value;
                    answerCache.Update(answer);
                }
            }
        }

        public void CopyAllAttributes(object row, object src)
        {
            CopyAttributes(row, src, true);
        }

        public void CopyAttributes(object row, object src)
        {
            CopyAttributes(row, src, false);
        }

        protected virtual string GetDefaultAnswerValue(CRAttribute.AttributeExt attr)
        {
            return attr.DefaultValue;
        }
    }

}
