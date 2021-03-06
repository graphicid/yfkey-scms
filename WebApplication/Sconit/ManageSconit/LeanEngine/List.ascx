﻿<%@ Control Language="C#" AutoEventWireup="true" CodeFile="List.ascx.cs" Inherits="ManageSconit_LeanEngine_List" %>
<%@ Register Assembly="com.Sconit.Control" Namespace="com.Sconit.Control" TagPrefix="cc1" %>
<%@ Register Src="View.ascx" TagName="View" TagPrefix="uc" %>
<fieldset>
    <div class="GridView">
        <cc1:GridView ID="GV_List" runat="server" AutoGenerateColumns="False" DataKeyNames="Code"
            AllowMultiColumnSorting="false" AutoLoadStyle="false" SeqNo="0" SeqText="No."
            ShowSeqNo="true" AllowSorting="True" AllowPaging="True" PagerID="gp" Width="100%"
            TypeName="com.Sconit.Web.CriteriaMgrProxy" SelectMethod="FindAll" SelectCountMethod="FindCount"
            OnRowDataBound="GV_List_RowDataBound">
            <Columns>
                <asp:BoundField DataField="Code" HeaderText="${MasterData.Flow.Code}" SortExpression="Code" />
                <asp:BoundField DataField="Description" HeaderText="${MasterData.Flow.Description}"
                    SortExpression="Description" />
                <asp:TemplateField HeaderText="${MasterData.Flow.Party.From.Supplier}" SortExpression="PartyFrom">
                    <ItemTemplate>
                        <%# DataBinder.Eval(Container.DataItem, "PartyFrom.Name")%>
                    </ItemTemplate>
                </asp:TemplateField>
                <asp:TemplateField HeaderText="${MasterData.Flow.Party.To.Customer}" SortExpression="PartyTo">
                    <ItemTemplate>
                        <%# DataBinder.Eval(Container.DataItem, "PartyTo.Name")%>
                    </ItemTemplate>
                </asp:TemplateField>
                <asp:TemplateField HeaderText="${MasterData.Flow.Location.To}" SortExpression="LocationTo">
                    <ItemTemplate>
                        <%# DataBinder.Eval(Container.DataItem, "LocationTo.Name")%>
                    </ItemTemplate>
                </asp:TemplateField>
                <asp:BoundField DataField="FlowStrategy" SortExpression="FlowStrategy" HeaderStyle-Wrap="false"
                    HeaderText="${MasterData.Flow.Strategy.Procurement}" />
                <asp:BoundField DataField="NextOrderTime" SortExpression="NextOrderTime" HeaderStyle-Wrap="false"
                    HeaderText="${MasterData.Flow.Strategy.NextOrderTime}" DataFormatString="{0:yyyy-MM-dd HH:mm}" />
                <asp:BoundField DataField="NextWinTime" SortExpression="NextWinTime" HeaderStyle-Wrap="false"
                    HeaderText="${MasterData.Flow.Strategy.NextWinTime}" DataFormatString="{0:yyyy-MM-dd HH:mm}" />
                <asp:TemplateField HeaderText="${Common.GridView.Action}">
                    <ItemTemplate>
                        <cc1:LinkButton ID="lbtnView" runat="server" CommandArgument='<%# DataBinder.Eval(Container.DataItem, "Code") %>'
                            Text="${Common.Button.View}" OnClick="lbtnView_Click" FunctionId="ViewOrderDetail">
                        </cc1:LinkButton>
                    </ItemTemplate>
                </asp:TemplateField>
            </Columns>
        </cc1:GridView>
        <cc1:GridPager ID="gp" runat="server" GridViewID="GV_List" PageSize="10">
        </cc1:GridPager>
    </div>
</fieldset>
<uc:View ID="ucView" runat="server" Visible="false" />
