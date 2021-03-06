﻿<%@ Control Language="C#" AutoEventWireup="true" CodeFile="Edit.ascx.cs" Inherits="Transportation_Carrier_Edit" %>

<%@ Register Src="~/Controls/TextBox.ascx" TagName="textbox" TagPrefix="uc3" %>
<div id="divFV" runat="server">
    <asp:FormView ID="FV_Carrier" runat="server" DataSourceID="ODS_Carrier" DefaultMode="Edit"
        Width="100%" DataKeyNames="Code" OnDataBound="FV_Carrier_DataBound">
        <EditItemTemplate>
            <fieldset>
                <legend>${Transportation.Carrier.UpdateCarrier}</legend>
                <table class="mtable">
                    <tr>
                        <td class="td01">
                            <asp:Literal ID="lblCode" runat="server" Text="${Transportation.Carrier.Code}:" />
                        </td>
                        <td class="td02">
                            <asp:Literal ID="tbCode" runat="server" Text='<%# Bind("Code") %>' />
                        </td>
                        <td class="td01">
                            <asp:Literal ID="lblName" runat="server" Text="${Transportation.Carrier.Name}:" />
                        </td>
                        <td class="td02">
                            <asp:TextBox ID="tbName" runat="server" Text='<%# Bind("Name") %>' Width="250"></asp:TextBox>
                        </td>
                    </tr>
                    <tr>
                        <td class="td01">
                            <asp:Literal ID="lblIsActive" runat="server" Text="${Transportation.Carrier.IsActive}:" />
                        </td>
                        <td class="td02">
                            <asp:CheckBox ID="tbIsActive" runat="server" Checked='<%#Bind("IsActive") %>' Width="250" />
                        </td>
                         <td class="td01">
                            <asp:Literal ID="lblCountry" runat="server" Text="${Transportation.Carrier.Country}:" />
                        </td>
                        <td class="td02">
                            <asp:TextBox ID="tbCountry" runat="server" Text='<%# Bind("Country") %>' Width="250"></asp:TextBox>
                        </td>
                    </tr>
                    <tr>
                        <td class="td01">
                            <asp:Literal ID="lblPaymentTerm" runat="server" Text="${Transportation.Carrier.PaymentTerm}:" />
                        </td>
                        <td class="td02">
                             <asp:TextBox ID="tbPaymentTerm" runat="server" Text='<%# Bind("PaymentTerm") %>' Width="250"></asp:TextBox>
                        </td>
                         <td class="td01">
                            <asp:Literal ID="lblTradeTerm" runat="server" Text="${Transportation.Carrier.TradeTerm}:" />
                        </td>
                        <td class="td02">
                            <asp:TextBox ID="tbTradeTerm" runat="server" Text='<%# Bind("TradeTerm") %>' Width="250"></asp:TextBox>
                        </td>
                    </tr>
                    <tr>
                        <td class="td01">
                            <asp:Literal ID="lblReferenceSupplier" runat="server" Text="${Transportation.Carrier.ReferenceSupplier}:" />
                        </td>
                        <td class="td02">
                             <asp:TextBox ID="tbReferenceSupplier" runat="server" Text='<%# Bind("ReferenceSupplier") %>' Width="250"></asp:TextBox>
                        </td>
                        <td class="td01">
                        </td>
                        <td class="td02">
                        </td>
                    </tr>
                    <tr>
                        <td class="td01">
                        </td>
                        <td class="td02">
                        </td>
                        <td class="td01">
                        </td>
                        <td class="td02">
                            <div class="buttons">
                                <asp:Button ID="Button1" runat="server" CommandName="Update" Text="${Common.Button.Save}" CssClass="apply"
                                    ValidationGroup="vgSave" />
                                <asp:Button ID="Button2" runat="server" CommandName="Delete" Text="${Common.Button.Delete}" CssClass="delete"
                                    OnClientClick="return confirm('${Common.Button.Delete.Confirm}')" />
                                <asp:Button ID="Button3" runat="server" Text="${Common.Button.Back}" OnClick="btnBack_Click" CssClass="back" />
                            </div>
                        </td>
                    </tr>
                </table>
            </fieldset>
        </EditItemTemplate>
    </asp:FormView>
</div>
<asp:ObjectDataSource ID="ODS_Carrier" runat="server" TypeName="com.Sconit.Web.CarrierMgrProxy"
    DataObjectTypeName="com.Sconit.Entity.Transportation.Carrier" UpdateMethod="UpdateCarrier"
    OnUpdated="ODS_Carrier_Updated" OnUpdating="ODS_Carrier_Updating" DeleteMethod="DeleteCarrier"
    OnDeleted="ODS_Carrier_Deleted" SelectMethod="LoadCarrier">
    <SelectParameters>
        <asp:Parameter Name="code" Type="String" />
    </SelectParameters>
    <DeleteParameters>
        <asp:Parameter Name="code" Type="String" />
    </DeleteParameters>
</asp:ObjectDataSource>
