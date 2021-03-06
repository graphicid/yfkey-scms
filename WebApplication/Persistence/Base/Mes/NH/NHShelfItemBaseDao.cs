using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Castle.Facilities.NHibernateIntegration;
using NHibernate;
using NHibernate.Type;
using com.Sconit.Entity.Mes;

//TODO: Add other using statmens here.

namespace com.Sconit.Persistence.Mes.NH
{
    public class NHShelfItemBaseDao : NHDaoBase, IShelfItemBaseDao
    {
        public NHShelfItemBaseDao(ISessionManager sessionManager)
            : base(sessionManager)
        {
        }

        #region Method Created By CodeSmith

        public virtual void CreateShelfItem(ShelfItem entity)
        {
            Create(entity);
        }

        public virtual IList<ShelfItem> GetAllShelfItem()
        {
            return FindAll<ShelfItem>();
        }

        public virtual ShelfItem LoadShelfItem(Int32 id)
        {
            return FindById<ShelfItem>(id);
        }

        public virtual void UpdateShelfItem(ShelfItem entity)
        {
            Update(entity);
        }

        public virtual void DeleteShelfItem(Int32 id)
        {
            string hql = @"from ShelfItem entity where entity.Id = ?";
            Delete(hql, new object[] { id }, new IType[] { NHibernateUtil.Int32 });
        }

        public virtual void DeleteShelfItem(ShelfItem entity)
        {
            Delete(entity);
        }

        public virtual void DeleteShelfItem(IList<Int32> pkList)
        {
            StringBuilder hql = new StringBuilder();
            hql.Append("from ShelfItem entity where entity.Id in (");
            hql.Append(pkList[0]);
            for (int i = 1; i < pkList.Count; i++)
            {
                hql.Append(",");
                hql.Append(pkList[i]);
            }
            hql.Append(")");

            Delete(hql.ToString());
        }

        public virtual void DeleteShelfItem(IList<ShelfItem> entityList)
        {
            IList<Int32> pkList = new List<Int32>();
            foreach (ShelfItem entity in entityList)
            {
                pkList.Add(entity.Id);
            }

            DeleteShelfItem(pkList);
        }


        #endregion Method Created By CodeSmith
    }
}
