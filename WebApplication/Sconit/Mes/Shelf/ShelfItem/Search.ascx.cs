﻿using System;
using System.Collections;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Xml.Linq;
using com.Sconit.Web;
using NHibernate.Expression;
using System.Collections.Generic;
using com.Sconit.Entity.MasterData;
using com.Sconit.Entity.Mes;

public partial class Mes_Shelf_ShelfItem_Search : SearchModuleBase
{
    public event EventHandler SearchEvent;
    public event EventHandler NewEvent;
    public event EventHandler BackEvent;

    public string ShelfCode
    {
        get
        {
            return (string)ViewState["ShelfCode"];
        }
        set
        {
            ViewState["ShelfCode"] = value;
        }
    }

    protected void Page_Load(object sender, EventArgs e)
    {
    }

    protected override void DoSearch()
    {

        if (SearchEvent != null)
        {
            #region DetachedCriteria
            DetachedCriteria selectCriteria = DetachedCriteria.For(typeof(ShelfItem));
            DetachedCriteria selectCountCriteria = DetachedCriteria.For(typeof(ShelfItem))
                .SetProjection(Projections.Count("Id"));


            selectCriteria.Add(Expression.Eq("Shelf.Code", this.ShelfCode));
            selectCountCriteria.Add(Expression.Eq("Shelf.Code", this.ShelfCode));

            if (this.tbItemCode.Text.Trim() != string.Empty)
            {
                selectCriteria.Add(Expression.Eq("Item.Code", this.tbItemCode.Text.Trim()));
                selectCountCriteria.Add(Expression.Eq("Item.Code", this.tbItemCode.Text.Trim()));

            }

            #endregion

            SearchEvent((new object[] { selectCriteria, selectCountCriteria }), null);
        }
    }

    protected override void InitPageParameter(IDictionary<string, string> actionParameter)
    {
        if (actionParameter.ContainsKey("ShelfCode"))
        {
            this.ShelfCode = actionParameter["ShelfCode"];
        }
        if (actionParameter.ContainsKey("ItemCode"))
        {
            this.tbItemCode.Text = actionParameter["ItemCode"];
        }
    }
    protected void btnNew_Click(object sender, EventArgs e)
    {
        if (NewEvent != null)
        {
            NewEvent(this, e);
        }
    }

    protected void btnBack_Click(object sender, EventArgs e)
    {
        if (BackEvent != null)
        {
            BackEvent(this, e);
        }
    }
    protected void btnSearch_Click(object sender, EventArgs e)
    {
        DoSearch();
    }
}
