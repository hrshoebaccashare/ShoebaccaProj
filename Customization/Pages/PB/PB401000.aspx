<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="PB401000.aspx.cs" Inherits="Page_PB401000" Title="Untitled Page" %>
<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>

<asp:Content ID="cont1" ContentPlaceHolderID="phDS" Runat="Server">
	<px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" PrimaryView="Items" TypeName="ShoebaccaProj.CustomItemValidationInq">
		<CallbackCommands>
			<px:PXDSCallbackCommand Name="Insert" PostData="Self" />
			<px:PXDSCallbackCommand CommitChanges="True" Name="Save" />
			<px:PXDSCallbackCommand Name="First" PostData="Self" StartNewGroup="true" />
			<px:PXDSCallbackCommand Name="Last" PostData="Self" />
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" Runat="Server">
	<px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100" 
		Width="100%">
	</px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" Runat="Server">
	<px:PXGrid ID="grid" runat="server" DataSourceID="ds" Style="z-index: 100" 
		Width="100%" Height="150px" SkinID="Details" TabIndex="2300">
		<Levels>
			<px:PXGridLevel DataMember="Items" DataKeyNames="UPCcode">
				<Columns>
					<px:PXGridColumn DataField="UPCcode" Width="300px" />
					<px:PXGridColumn DataField="Description" Width="300px" />
					<px:PXGridColumn DataField="AlreadyExists" TextAlign="Center" Type="CheckBox" Width="80px" />
					<px:PXGridColumn DataField="ItemID" Width="300px" />
					<px:PXGridColumn DataField="ItemDescription" Width="300px" />
				</Columns>
			</px:PXGridLevel>
		</Levels>
		<AutoSize Container="Window" Enabled="True" MinHeight="150" />
		<Mode InitNewRow="True" AllowUpload="True" />
	</px:PXGrid>
</asp:Content>
