﻿<%@ Control Language="C#" AutoEventWireup="true" CodeFile="NewSearch.ascx.cs" Inherits="Transportation_Bill_NewSearch" %>
<%@ Register Src="~/Controls/TextBox.ascx" TagName="textbox" TagPrefix="uc3" %>
<script language="javascript" type="text/javascript" src="Js/DatePicker/WdatePicker.js"></script>
<%@ Register Src="NewList.ascx" TagName="NewList" TagPrefix="uc2" %>
<fieldset>
    <table class="mtable">
        <tr>
            <td class="td01">
                <asp:Literal ID="ltlPartyCode" runat="server" Text="${Transportation.TransportationActBill.Party}:" />
            </td>
            <td class="td02">
                <uc3:textbox ID="tbPartyCode" runat="server" DescField="Name" ValueField="Code" ServicePath="PartyMgr.service"
                    ServiceMethod="GetTransportationParty" Width="250" />
                <asp:Literal ID="ltlParty" runat="server" Visible="false" />
            </td>
            <td class="td01">
                <asp:Literal ID="ltlExpenseNo" runat="server" Text="${Transportation.TransportationActBill.ExpenseNo}:" />
            </td>
            <td class="td02">
                <asp:TextBox ID="tbExpenseNo" runat="server" Visible="true" />
            </td>
        </tr>
        <tr>
            <td class="td01">
                <asp:Literal ID="ltlStartDate" runat="server" Text="${Transportation.TransportationActBill.EffectiveDateFrom}:" />
            </td>
            <td class="td02">
                <asp:TextBox ID="tbStartDate" runat="server" onClick="WdatePicker({dateFmt:'yyyy-MM-dd'})" />
            </td>
            <td class="td01">
                <asp:Literal ID="ltlEndDate" runat="server" Text="${Transportation.TransportationActBill.EffectiveDateTo}:" />
            </td>
            <td class="td02">
                <asp:TextBox ID="tbEndDate" runat="server" onClick="WdatePicker({dateFmt:'yyyy-MM-dd'})" />
            </td>
        </tr>
        <tr>
            <td class="td01">
                <asp:Literal ID="ltlItem" runat="server" Text="${Common.Business.ItemCode}:" />
            </td>
            <td class="td02">
                <uc3:textbox ID="tbItemCode" runat="server" Visible="true" Width="250" DescField="Description"
                    ValueField="Code" ServicePath="ItemMgr.service" ServiceMethod="GetCacheAllItem" />
            </td>
            <td class="td01">
                <asp:Literal ID="ltlCurrency" runat="server" Text="${Transportation.TransportationActBill.Currency}:" />
            </td>
            <td class="td02">
                <uc3:textbox ID="tbCurrency" runat="server" Visible="true" Width="250" DescField="Name"
                    ValueField="Code" ServicePath="CurrencyMgr.service" ServiceMethod="GetAllCurrency" />
            </td>
        </tr>
        <tr>
            <td class="td01">
            </td>
            <td class="td02">
                <asp:CheckBox ID="IsRelease" runat="server" Font-Size="9pt" Text="${Transportation.TransportationActBill.IsRelease}" />
            </td>
            <td class="td01">
            </td>
            <td class="td02">
                <asp:Button ID="btnSearch" runat="server" Text="${Common.Button.Search}" OnClick="btnSearch_Click"
                    Width="59px" CssClass="button2" />
                <asp:Button ID="btnConfirm" runat="server" Text="${Common.Button.Create}" OnClick="btnConfirm_Click"
                    Width="59px" CssClass="button2" />
                <asp:Button ID="btnImport" runat="server" Text="${Common.Button.Import}" OnClick="btnImport_Click"
                    Width="59px" CssClass="button2" />
                <asp:Button ID="btnBack" runat="server" Text="${Common.Button.Back}" OnClick="btnBack_Click"
                    Width="59px" CssClass="button2" />
            </td>
        </tr>
    </table>
</fieldset>
<uc2:NewList ID="ucNewList" runat="server" />
<asp:Button ID="btnAddDetail" runat="server" Text="${Common.Button.AddDetail}" OnClick="btnAddDetail_Click"
    CssClass="button2" Visible="false" />
<asp:Button ID="btnClose" runat="server" OnClick="btnBack_Click" Width="59px" CssClass="button2"
    Visible="false" />
<div id="floatdiv">
    <fieldset runat="server" id="fs01" visible="false">
        <legend>文件上传</legend>
        <table style="width: 100%" class="mtable">
            <tr>
                <td class="td01">
                    文件位置：
                </td>
                <td class="td02">
                    <asp:FileUpload ID="fileUpload" runat="server" />
                    <asp:Button runat="server" ID="btnUpload" OnClick="btnUpload_Click" Text="上传" CssClass="button2" />
                    <asp:Button ID="Button9" runat="server" Text="返回" Width="59px" OnClick="Button9_Click" />
                    <a href="..\..\Reports\Templates\YFKExcelTemplates\BatchImport.xls">模板下载</a>
                </td>
            </tr>
        </table>
    </fieldset>
</div>
