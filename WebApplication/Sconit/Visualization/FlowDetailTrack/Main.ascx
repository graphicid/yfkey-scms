﻿<%@ Control Language="C#" AutoEventWireup="true" CodeFile="Main.ascx.cs" Inherits="Visualization__FlowDetailTrack_Main" %>
<%@ Register Src="Search.ascx" TagName="Search" TagPrefix="uc2" %>
<%@ Register Src="List.ascx" TagName="List" TagPrefix="uc2" %>
<%@ Register Src="View.ascx" TagName="View" TagPrefix="uc2" %>
<table class="mtable">
    <tr>
        <td>
            <uc2:Search ID="ucSearch" runat="server" Visible="true" />
            <uc2:List ID="ucList" runat="server" Visible="false" />
            <uc2:View ID="ucView" runat="server" Visible="false" />
        </td>
    </tr>
</table>
