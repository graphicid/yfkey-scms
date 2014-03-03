using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Castle.Facilities.NHibernateIntegration;
using NHibernate;
using NHibernate.Type;
using com.Mes.Dss.Entity;
using com.Mes.Dss.Persistence;

//TODO: Add other using statmens here.

namespace com.Mes.Dss.Persistence.NH
{
    public class NHMesScmsBomDao : NHDaoBase, IMesScmsBomDao
    {
        public NHMesScmsBomDao(ISessionManager sessionManager)
            : base(sessionManager)
        {
        }

    }
}