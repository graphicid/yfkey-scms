﻿<%@ Control Language="C#" AutoEventWireup="true" CodeFile="List.ascx.cs" Inherits="MasterData_Location_List" %>
<%@ Register Assembly="com.Sconit.Control" Namespace="com.Sconit.Control" TagPrefix="cc1" %>
<fieldset>
    <div class="GridView">
        <cc1:GridView ID="GV_List" runat="server" AutoGenerateColumns="False" DataKeyNames="Code"
            SkinID="GV" AllowMultiColumnSorting="false" AutoLoadStyle="false" SeqNo="0" SeqText="No."
            ShowSeqNo="true" AllowSorting="true" AllowPaging="True" PagerID="gp" Width="100%"
            CellMaxLength="10" TypeName="com.Sconit.Web.CriteriaMgrProxy" SelectMethod="FindAll"
            SelectCountMethod="FindCount" OnRowDataBound="GV_List_RowDataBound">
            <Columns>
                <asp:BoundField DataField="Code" HeaderText="${MasterData.Location.Code}" SortExpression="Code" />
                <asp:BoundField DataField="Name" HeaderText="${MasterData.Location.Name}" SortExpression="Name" />
                <asp:TemplateField HeaderText="${MasterData.Location.Region}" SortExpression="Region.Code">
                    <ItemTemplate>
                        <%# DataBinder.Eval(Container.DataItem, "Region.Code")%>
                    </ItemTemplate>
                </asp:TemplateField>
                <asp:CheckBoxField DataField="AllowNegativeInventory" HeaderText="${MasterData.Location.AllowNegativeInventory}"
                    SortExpression="AllowNegativeInventory" />
                <asp:CheckBoxField DataField="IsSettleConsignment" HeaderText="${MasterData.Location.IsSettleConsignment}"
                    SortExpression="IsSettleConsignment" />
                <asp:CheckBoxField DataField="IsActive" HeaderText="${MasterData.Location.IsActive}"
                    SortExpression="IsActive" />
                <asp:TemplateField HeaderText="${Common.GridView.Action}">
                    <ItemTemplate>
                        <asp:LinkButton ID="lbtnEdit" runat="server" CommandArgument='<%# Eval("Code") +"," + Eval("EnableAdvWM") %>'
                            Text="${Common.Button.Edit}" OnClick="lbtnEdit_Click" />
                        <asp:LinkButton ID="lbtnDelete" runat="server" CommandArgument='<%# DataBinder.Eval(Container.DataItem, "Code") %>'
                            Text="${Common.Button.Delete}" OnClick="lbtnDelete_Click" OnClientClick="return confirm('${Common.Button.Delete.Confirm}')" />
                    </ItemTemplate>
                </asp:TemplateField>
            </Columns>
        </cc1:GridView>
        <cc1:GridPager ID="gp" runat="server" GridViewID="GV_List" PageSize="10">
        </cc1:GridPager>
    </div>
</fieldset>
