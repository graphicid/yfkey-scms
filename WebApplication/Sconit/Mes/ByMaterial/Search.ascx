﻿<%@ Control Language="C#" AutoEventWireup="true" CodeFile="Search.ascx.cs" Inherits="Mes_ByMaterial_Search" %>
<%@ Register Src="~/Controls/TextBox.ascx" TagName="textbox" TagPrefix="uc3" %>
<fieldset>
    <table class="mtable">
        <tr>
            <td class="td01">
                <asp:Literal ID="lblCode" runat="server" Text="${Mes.ByMaterial.OrderNo}:" />
            </td>
            <td class="td02">
                <asp:TextBox ID="tbOrderNo" runat="server" />
            </td>
             <td class="td01">
                <asp:Literal ID="lblTagNo" runat="server" Text="${Mes.ByMaterial.TagNo}:" />
            </td>
            <td class="td02">
                <asp:TextBox ID="tbTagNo" runat="server" />
            </td>
        </tr>
         <tr>
            <td class="td01">
                <asp:Literal ID="lblItem" runat="server" Text="${Mes.ByMaterial.Item}:" />
            </td>
            <td class="td02">
                <asp:TextBox ID="tbItem" runat="server" />
            </td>
             <td class="td01">
              
            </td>
            <td class="td02">
            </td>
        </tr>
        <tr>
            <td colspan="3" />
            <td class="ttd02">
                <div class="buttons">
                    <asp:Button ID="btnSearch" runat="server" Text="${Common.Button.Search}" OnClick="btnSearch_Click"
                        CssClass="query" />
                    <asp:Button ID="btnNew" runat="server" Text="${Common.Button.New}" OnClick="btnNew_Click"
                        CssClass="add" />
                </div>
            </td>
        </tr>
    </table>
</fieldset>
