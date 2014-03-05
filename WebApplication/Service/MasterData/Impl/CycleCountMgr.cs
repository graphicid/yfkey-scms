using System;
using System.Collections;
using System.Collections.Generic;
using Castle.Services.Transaction;
using com.Sconit.Entity;
using com.Sconit.Entity.Exception;
using com.Sconit.Entity.MasterData;
using com.Sconit.Persistence.MasterData;
using NHibernate.Expression;
using com.Sconit.Service.Criteria;
using System.Linq;
using com.Sconit.Utility;
using NHibernate.Transform;
using NHibernate.SqlCommand;

//TODO: Add other using statements here.

namespace com.Sconit.Service.MasterData.Impl
{
    [Transactional]
    public class CycleCountMgr : CycleCountBaseMgr, ICycleCountMgr
    {
        private string[] CycleCountResult2CycleCountDetailFields = new string[] 
            { 
                "CycleCount",
                "Item",
                "Hu",
                "LotNo",
                "Qty",
                "StorageBin"
            };
        private string[] LocationLotDetail2CycleCountResultFields = new string[] 
            { 
                "InvQty",
                "StorageBin",
                "ReferenceLocation"
            };

        private ICycleCountDetailMgr cycleCountDetailMgr;
        private ILocationDetailMgr locationDetailMgr;
        private INumberControlMgr numberControlMgr;
        private ICriteriaMgr criteriaMgr;
        private ICycleCountResultMgr cycleCountResultMgr;
        private ILocationMgr locationMgr;
        private IStorageBinMgr storageBinMgr;
        private ILocationLotDetailMgr locationLotDetailMgr;
        private IEntityPreferenceMgr entityPreferenceMgr;

        public CycleCountMgr(ICycleCountDao entityDao,
            ICycleCountDetailMgr cycleCountDetailMgr,
            ILocationDetailMgr locationDetailMgr,
            INumberControlMgr numberControlMgr,
            ICriteriaMgr criteriaMgr,
            ICycleCountResultMgr cycleCountResultMgr,
            ILocationMgr locationMgr,
            IStorageBinMgr storageBinMgr,
            ILocationLotDetailMgr locationLotDetailMgr,
            IEntityPreferenceMgr entityPreferenceMgr)
            : base(entityDao)
        {
            this.cycleCountDetailMgr = cycleCountDetailMgr;
            this.locationDetailMgr = locationDetailMgr;
            this.numberControlMgr = numberControlMgr;
            this.criteriaMgr = criteriaMgr;
            this.cycleCountResultMgr = cycleCountResultMgr;
            this.locationMgr = locationMgr;
            this.storageBinMgr = storageBinMgr;
            this.locationLotDetailMgr = locationLotDetailMgr;
            this.entityPreferenceMgr = entityPreferenceMgr;
        }

        #region Customized Methods
        [Transaction(TransactionMode.Unspecified)]
        public CycleCount LoadCycleCount(string code, bool includeDetails)
        {
            CycleCount cycleCount = LoadCycleCount(code);
            if (includeDetails && cycleCount != null && cycleCount.CycleCountDetails != null && cycleCount.CycleCountDetails.Count > 0)
            {
            }
            return cycleCount;
        }

        [Transaction(TransactionMode.Unspecified)]
        public CycleCount CheckAndLoadCycleCount(string code)
        {
            CycleCount cycleCount = LoadCycleCount(code);
            if (cycleCount == null)
            {
                throw new BusinessErrorException("Common.Business.Error.EntityNotExist", code);
            }
            return cycleCount;
        }

        [Transaction(TransactionMode.Requires)]
        public void CreateCycleCount(CycleCount cycleCount, User user)
        {
            if (cycleCount.Code == null || cycleCount.Code.Trim() == string.Empty)
                cycleCount.Code = numberControlMgr.GenerateNumber(BusinessConstants.CODE_PREFIX_CYCCNT);

            cycleCount.Status = BusinessConstants.CODE_MASTER_STATUS_VALUE_CREATE;
            cycleCount.CreateUser = user.Code;
            cycleCount.CreateDate = DateTime.Now;
            cycleCount.LastModifyUser = user;
            cycleCount.LastModifyDate = DateTime.Now;

            base.CreateCycleCount(cycleCount);
        }

        [Transaction(TransactionMode.Requires)]
        public void CreateCycleCount(StorageBin bin, IList<CycleCountDetail> cycleCountDetailList, User user)
        {
            if (bin == null)
                throw new BusinessErrorException("Location.Error.PutAway.BinEmpty");

            CycleCount cycleCount = new CycleCount();
            cycleCount.Location = bin.Area.Location;

            this.CreateClassifyingCheckCycleCount(cycleCount, cycleCountDetailList, user);
        }

        [Transaction(TransactionMode.Requires)]
        public void CreateCycleCount(Location location, IList<CycleCountDetail> cycleCountDetailList, User user)
        {
            CycleCount cycleCount = new CycleCount();
            cycleCount.Location = location;

            this.CreateClassifyingCheckCycleCount(cycleCount, cycleCountDetailList, user);
        }

        [Transaction(TransactionMode.Requires)]
        public void SubmitCycleCount(string orderNo, User user)
        {
            CycleCount cycleCount = this.LoadCycleCount(orderNo);
            this.SubmitCycleCount(cycleCount, user);
        }

        [Transaction(TransactionMode.Requires)]
        public void SubmitCycleCount(CycleCount cycleCount, User user)
        {
            #region У��
            if (cycleCount == null)
                throw new BusinessErrorException("Common.Business.Error.EntityNotExist", cycleCount.Code);
            if (!this.CheckDetailExist(cycleCount.Code))
                throw new BusinessErrorException("Common.Business.Error.SaveEmpty");
            #endregion

            IList<CycleCountResult> cycleCountResultList = this.CalcCycleCount(cycleCount, user);
            this.cycleCountResultMgr.SaveCycleCountResult(cycleCount.Code, cycleCountResultList);

            cycleCount.Status = BusinessConstants.CODE_MASTER_STATUS_VALUE_SUBMIT;
            cycleCount.ReleaseUser = user.Code;
            cycleCount.ReleaseDate = DateTime.Now;
            cycleCount.LastModifyUser = user;
            cycleCount.LastModifyDate = DateTime.Now;
            this.UpdateCycleCount(cycleCount);
        }

        [Transaction(TransactionMode.Unspecified)]
        public IList<CycleCountResult> CalcCycleCount(string orderNo, User user)
        {
            CycleCount cycleCount = this.LoadCycleCount(orderNo, true);
            if (cycleCount == null)
                throw new BusinessErrorException("Common.Business.Error.EntityNotExist", orderNo);

            return this.CalcCycleCount(cycleCount, user);
        }

        [Transaction(TransactionMode.Unspecified)]
        public IList<CycleCountResult> CalcCycleCount(CycleCount cycleCount, User user)
        {
            #region �����ϸ,��������ͨ�������̵�
            IList<LocationTransaction> locationTransactionList = this.GetLocationTransaction(cycleCount.Location.Code, cycleCount.EffectiveDate);
            IList<LocationLotDetail> itemLocationDetailList = this.GetItemLocationDetail(cycleCount.Location.Code);
            IList<LocationLotDetail> huLocationDetailList = this.GetHuLocationDetail(cycleCount.Location.Code);
            this.GetItemInvHistory(locationTransactionList, itemLocationDetailList);

            IList<LocationLotDetail> locationLotDetailList = new List<LocationLotDetail>();
            IListHelper.AddRange<LocationLotDetail>(locationLotDetailList, itemLocationDetailList);
            IListHelper.AddRange<LocationLotDetail>(locationLotDetailList, huLocationDetailList);
            #endregion

            IList<CycleCountDetail> cycleCountDetailList = cycleCountDetailMgr.GetCycleCountDetail(cycleCount.Code);
            IList<CycleCountResult> cycleCountResultList = this.GetCycleCountResultComparer(cycleCount, cycleCountDetailList, locationLotDetailList);

            IList<string> huIdList = cycleCountDetailList.Where(c => c.HuId != null).Select(c => c.HuId).ToList<string>();
            IList<LocationLotDetail> huLocationLotDetailList = this.GetHuLocationLotDetail(huIdList);

            foreach (var cycleCountResult in cycleCountResultList)
            {
                var qInv = locationLotDetailList.Where(l =>
                            l.Item == cycleCountResult.Item &&
                            l.Hu.HuId == cycleCountResult.HuId &&
                            l.StorageBin.Code == cycleCountResult.StorageBin).SingleOrDefault();

                var qCyccnt = cycleCountDetailList.Where(c =>
                            c.Item == cycleCountResult.Item &&
                            c.HuId == cycleCountResult.HuId &&
                            c.StorageBin == cycleCountResult.StorageBin).SingleOrDefault();

                cycleCountResult.CycleCount = cycleCount;
                cycleCountResult.LotNo = cycleCountResult.HuId != null ? cycleCountResult.LotNo : null;
                cycleCountResult.Qty = qCyccnt != null ? qCyccnt.Qty : 0;
                cycleCountResult.InvQty = qInv != null ? qInv.Qty : 0;
                cycleCountResult.DiffQty = this.GetDiffQty(cycleCountResult);

                #region ������ӯ�������
                if (cycleCountResult.HuId != null && cycleCountResult.DiffQty > 0)
                {
                    var qHu = huLocationLotDetailList.SingleOrDefault(l => l.Hu.HuId == cycleCountResult.HuId);
                    if (qHu != null)
                        cycleCountResult.ReferenceLocation = qHu.Location;
                }
                #endregion
            }

            return cycleCountResultList.Where(c => c.DiffQty != 0).ToList<CycleCountResult>();
        }

        [Transaction(TransactionMode.Unspecified)]
        public IList<CycleCountResult> ReCalcCycleCount(string orderNo, User user)
        {
            CycleCount cycleCount = this.LoadCycleCount(orderNo, true);
            if (cycleCount == null)
                throw new BusinessErrorException("Common.Business.Error.EntityNotExist", orderNo);

            IList<CycleCountResult> cycleCountResultList = this.CalcCycleCount(cycleCount, user);
            this.cycleCountResultMgr.SaveCycleCountResult(orderNo, cycleCountResultList);

            cycleCount.LastModifyUser = user;
            cycleCount.LastModifyDate = DateTime.Now;
            this.UpdateCycleCount(cycleCount);

            return cycleCountResultList;
        }

        [Transaction(TransactionMode.Requires)]
        public void SaveCycleCount(string orderNo, IList<CycleCountDetail> cycCntDetList, User user)
        {
            CycleCount cycleCount = this.LoadCycleCount(orderNo);
            if (cycleCount == null)
                throw new BusinessErrorException("Common.Business.Error.EntityNotExist", orderNo);

            this.SaveCycleCount(cycleCount, cycCntDetList, user);
        }

        [Transaction(TransactionMode.Requires)]
        public void SaveCycleCount(CycleCount cycleCount, IList<CycleCountDetail> cycleCountDetailList, User user)
        {
            if (cycleCount == null || cycleCountDetailList == null || cycleCountDetailList.Count == 0)
                return;

            if (cycleCount.Code == null || cycleCount.Code.Trim() == string.Empty)
                this.CreateCycleCount(cycleCount, user);

            if (cycleCount.Status != BusinessConstants.CODE_MASTER_STATUS_VALUE_CREATE)
                throw new BusinessErrorException("Common.Business.Error.StatusError", cycleCount.Code, cycleCount.Status);

            foreach (CycleCountDetail cycleCountDetail in cycleCountDetailList)
            {
                #region ����0
                if (cycleCountDetail.Qty == 0)
                {
                    var qCnt = cycleCountDetailList.Where(c => c.Qty > 0 && c.Item == cycleCountDetail.Item
                        && c.HuId == cycleCountDetail.HuId && c.StorageBin == cycleCountDetail.StorageBin).Count();

                    if (qCnt > 0)
                        continue;
                }
                #endregion

                if (cycleCountDetail.Id == 0)
                {
                    cycleCountDetail.CycleCount = cycleCount;
                    cycleCountDetailMgr.CreateCycleCountDetail(cycleCountDetail);
                }
                else
                {
                    CycleCountDetail updateCycleCountDetail = cycleCountDetailMgr.LoadCycleCountDetail(cycleCountDetail.Id);
                    updateCycleCountDetail.Qty = cycleCountDetail.Qty;
                    if (updateCycleCountDetail != null)
                        cycleCountDetailMgr.UpdateCycleCountDetail(updateCycleCountDetail);
                }
            }

            cycleCount.LastModifyUser = user;
            cycleCount.LastModifyDate = DateTime.Now;
            this.UpdateCycleCount(cycleCount);
        }

        [Transaction(TransactionMode.Unspecified)]
        public void CheckHuExistThisCount(string huId)
        {
            DetachedCriteria criteria = DetachedCriteria.For(typeof(CycleCountDetail));
            criteria.CreateAlias("CycleCount", "cc");
            criteria.Add(Expression.In("cc.Status", new string[] { 
                BusinessConstants.CODE_MASTER_STATUS_VALUE_CREATE, BusinessConstants.CODE_MASTER_STATUS_VALUE_SUBMIT }));
            criteria.Add(Expression.Eq("HuId", huId));
            criteria.SetProjection(Projections.Count("Id"));

            IList result = criteriaMgr.FindAll(criteria);
            if (result != null && result.Count > 0)
            {
                if ((int)result[0] > 0)
                    throw new BusinessErrorException("Common.Business.Error.ReScan", huId);
            }
        }
        #endregion Customized Methods

        #region Private Method
        [Transaction(TransactionMode.Unspecified)]
        private bool CheckDetailExist(string orderNo)
        {
            DetachedCriteria criteria = DetachedCriteria.For(typeof(CycleCountDetail));
            criteria.Add(Expression.Eq("CycleCount.Code", orderNo));
            criteria.SetProjection(Projections.Count("Id"));

            IList result = criteriaMgr.FindAll(criteria);
            if (result != null && result.Count > 0)
            {
                if ((int)result[0] > 0)
                    return true;
            }
            return false;
        }

        [Transaction(TransactionMode.Unspecified)]
        private IList<LocationLotDetail> GetItemLocationDetail(string location)
        {
            DetachedCriteria criteria = DetachedCriteria.For(typeof(LocationLotDetail));
            //criteria.Add(Expression.Not(Expression.Eq("Qty", new decimal(0))));
            criteria.Add(Expression.Eq("Location.Code", location));
            criteria.Add(Expression.IsNull("Hu"));
            criteria.SetProjection(Projections.ProjectionList()
                .Add(Projections.Sum("Qty").As("Qty"))
                .Add(Projections.GroupProperty("Location").As("Location"))
                .Add(Projections.GroupProperty("StorageBin").As("StorageBin"))
                .Add(Projections.GroupProperty("Item").As("Item"))
                .Add(Projections.GroupProperty("Hu").As("Hu")));

            criteria.SetResultTransformer(Transformers.AliasToBean(typeof(LocationLotDetail)));

            return criteriaMgr.FindAll<LocationLotDetail>(criteria);
        }

        [Transaction(TransactionMode.Unspecified)]
        private IList<LocationLotDetail> GetHuLocationDetail(string location)
        {
            DetachedCriteria criteria = DetachedCriteria.For(typeof(LocationLotDetail));
            criteria.Add(Expression.Not(Expression.Eq("Qty", new decimal(0))));
            criteria.Add(Expression.Eq("Location.Code", location));
            criteria.Add(Expression.IsNotNull("Hu"));

            return criteriaMgr.FindAll<LocationLotDetail>(criteria);
        }

        [Transaction(TransactionMode.Unspecified)]
        private IList<LocationLotDetail> GetHuLocationLotDetail(IList<string> huIdList)
        {
            DetachedCriteria criteria = DetachedCriteria.For(typeof(LocationLotDetail));
            criteria.Add(Expression.Not(Expression.Eq("Qty", new decimal(0))));
            if (huIdList.Count == 1)
            {
                criteria.Add(Expression.Eq("Hu.HuId", huIdList[0]));
            }
            else
            {
                criteria.Add(Expression.InG<string>("Hu.HuId", huIdList));
            }
            return criteriaMgr.FindAll<LocationLotDetail>(criteria);
        }

        [Transaction(TransactionMode.Unspecified)]
        private IList<LocationTransaction> GetLocationTransaction(string location, DateTime effDate)
        {
            DetachedCriteria criteria = DetachedCriteria.For(typeof(LocationTransaction));
            criteria.Add(Expression.Eq("Location", location));
            criteria.Add(Expression.Gt("EffectiveDate", effDate));

            return criteriaMgr.FindAll<LocationTransaction>(criteria);
        }

        private decimal GetDiffQty(CycleCountResult cycleCountResult)
        {
            return cycleCountResult.Qty - cycleCountResult.InvQty;
        }

        [Transaction(TransactionMode.Requires)]
        private void CreateClassifyingCheckCycleCount(CycleCount cycleCount, IList<CycleCountDetail> cycleCountDetailList, User user)
        {
            cycleCount.EffectiveDate = DateTime.Today;
            cycleCount.Type = BusinessConstants.CODE_MASTER_PHYCNT_TYPE_CLASSIFYINGCHECK;

            this.CreateCycleCount(cycleCount, user);
            this.cycleCountDetailMgr.CreateCycleCountDetail(cycleCount, cycleCountDetailList);
            this.CleanSession();
            cycleCount = this.LoadCycleCount(cycleCount.Code);
            this.SubmitCycleCount(cycleCount, user);

        }

        private IList<CycleCountResult> GetCycleCountResultComparer(CycleCount cycleCount, IList<CycleCountDetail> cycleCountDetailList, IList<LocationLotDetail> locationLotDetailList)
        {
            var query = cycleCountDetailList.Select(c => new
            {
                Item = c.Item,
                Hu = c.HuId,
                StorageBin = c.StorageBin
            }).Distinct();

            if (false) //if (cycleCount.Type != BusinessConstants.CODE_MASTER_PHYCNT_TYPE_SPOTCHECK)
            {
                var lquery = locationLotDetailList.Select(l => new
                {
                    Item = l.Item,
                    Hu = l.Hu.HuId,
                    StorageBin = l.StorageBin.Code
                }).Distinct();

                if (cycleCount.Type == BusinessConstants.CODE_MASTER_PHYCNT_TYPE_CLASSIFYINGCHECK)
                {
                    //���̿��򻺳���
                    var bquery = query.Select(q => q.StorageBin).Distinct();
                    var lnewQuery = lquery.Where(l => bquery.Contains(l.StorageBin));
                    lquery = lnewQuery;
                }

                query = query.Union(lquery);
            }

            query.Distinct();

            IList<CycleCountResult> cycleCountResultList = new List<CycleCountResult>();
            foreach (var item in query)
            {
                CycleCountResult cycleCountResult = new CycleCountResult();
                cycleCountResult.Item = item.Item;
                cycleCountResult.HuId = item.Hu;
                cycleCountResult.StorageBin = item.StorageBin;
                cycleCountResultList.Add(cycleCountResult);
            }

            return cycleCountResultList;
        }

        private void GetItemInvHistory(IList<LocationTransaction> locationTransactionList, IList<LocationLotDetail> itemLocationDetailList)
        {
            if (itemLocationDetailList != null && itemLocationDetailList.Count > 0)
            {
                foreach (var item in itemLocationDetailList)
                {
                    item.Qty -= locationTransactionList.Where(l => l.HuId == null && l.Item == item.Item.Code).Sum(l => l.Qty);
                }
            }
        }

        #endregion

        #region �µ��̵㹦��
        [Transaction(TransactionMode.Requires)]
        public CycleCount CreateCycleCount(string type, string locationCode, string bins, string items, User user)
        {
            CycleCount cycleCount = new CycleCount();
            cycleCount.Code = numberControlMgr.GenerateNumber(BusinessConstants.CODE_PREFIX_CYCCNT);
            cycleCount.Type = type;
            cycleCount.Status = BusinessConstants.CODE_MASTER_STATUS_VALUE_CREATE;
            cycleCount.Location = this.locationMgr.LoadLocation(locationCode);
            cycleCount.CreateUser = user.Code;
            cycleCount.CreateDate = DateTime.Now;
            cycleCount.LastModifyUser = user;
            cycleCount.LastModifyDate = DateTime.Now;
            cycleCount.Bins = bins;
            cycleCount.Items = items;

            base.CreateCycleCount(cycleCount);

            return cycleCount;
        }

        [Transaction(TransactionMode.Requires)]
        public void DeleteCycleCount(string orderNo, User user)
        {
            CycleCount cycleCount = this.CheckAndLoadCycleCount(orderNo);
            if (cycleCount.Status != BusinessConstants.CODE_MASTER_STATUS_VALUE_CREATE)
            {
                throw new BusinessErrorException("Common.Business.Error.StatusError", cycleCount.Code, cycleCount.Status);
            }

            base.DeleteCycleCount(orderNo);
        }

        [Transaction(TransactionMode.Requires)]
        public void ReleaseCycleCount(CycleCount cycleCount, User user)
        {
            this.UpdateCycleCount(cycleCount);
            this.ReleaseCycleCount(cycleCount.Code, user);
        }

        [Transaction(TransactionMode.Requires)]
        public void ReleaseCycleCount(string orderNo, User user)
        {
            CycleCount cycleCount = this.CheckAndLoadCycleCount(orderNo);
            if (cycleCount.Status != BusinessConstants.CODE_MASTER_STATUS_VALUE_CREATE)
            {
                throw new BusinessErrorException("Common.Business.Error.StatusError", cycleCount.Code, cycleCount.Status);
            }

            cycleCount.Status = BusinessConstants.CODE_MASTER_STATUS_VALUE_SUBMIT;
            cycleCount.ReleaseUser = user.Code;
            cycleCount.ReleaseDate = DateTime.Now;
            cycleCount.LastModifyUser = user;
            cycleCount.LastModifyDate = cycleCount.ReleaseDate.Value;

            base.UpdateCycleCount(cycleCount);

            StartCycleCount(orderNo, user);
        }

        [Transaction(TransactionMode.Requires)]
        public void StartCycleCount(string orderNo, User user)
        {
            CycleCount cycleCount = this.CheckAndLoadCycleCount(orderNo);
            if (cycleCount.Status != BusinessConstants.CODE_MASTER_STATUS_VALUE_SUBMIT)
            {
                throw new BusinessErrorException("Common.Business.Error.StatusError", cycleCount.Code, cycleCount.Status);
            }

            cycleCount.Status = BusinessConstants.CODE_MASTER_STATUS_VALUE_INPROCESS;
            cycleCount.StartUser = user.Code;
            cycleCount.StartDate = DateTime.Now;
            cycleCount.LastModifyUser = user;
            cycleCount.LastModifyDate = cycleCount.StartDate.Value;

            base.UpdateCycleCount(cycleCount);
        }

        [Transaction(TransactionMode.Requires)]
        public void RecordCycleCountDetail(string orderNo, IList<CycleCountDetail> cycleCountDetailList, User user)
        {
            CycleCount cycleCount = this.CheckAndLoadCycleCount(orderNo);
            if (cycleCount.Status != BusinessConstants.CODE_MASTER_STATUS_VALUE_INPROCESS)
            {
                throw new BusinessErrorException("Common.Business.Error.StatusError", cycleCount.Code, cycleCount.Status);
            }

            #region ��ȡ��ҵ�������ж��Ƿ���ɨ���ʱ���ϼ�
            EntityPreference entityPreference = this.entityPreferenceMgr.LoadEntityPreference(BusinessConstants.ENTITY_PREFERENCE_CODE_PUT_WHEN_CYCLE_COUNT);
            bool isPut = entityPreference != null ? bool.Parse(entityPreference.Value) : false;
            #endregion

            #region �����Ѿ��̵���ļ�¼
            //ɨ������Ļ���HuId
            //��ɨ������Ļ��������
            IList<string> phyCntedList = new List<string>();

            if (cycleCount.IsScanHu)
            {
                DetachedCriteria criteria = DetachedCriteria.For<CycleCountDetail>();
                criteria.CreateAlias("CycleCount", "cc");
                criteria.SetProjection(Projections.ProjectionList().Add(Projections.GroupProperty("HuId")));
                criteria.Add(Expression.Eq("cc.Code", orderNo));
                phyCntedList = this.criteriaMgr.FindAll<string>(criteria);
            }
            else
            {
                DetachedCriteria criteria = DetachedCriteria.For<CycleCountDetail>();
                criteria.CreateAlias("CycleCount", "cc");
                criteria.CreateAlias("Item", "item");
                criteria.SetProjection(Projections.ProjectionList().Add(Projections.GroupProperty("item.Code")));
                criteria.Add(Expression.Eq("cc.Code", orderNo));
                phyCntedList = this.criteriaMgr.FindAll<string>(criteria);
            }
            #endregion

            if (cycleCountDetailList != null && cycleCountDetailList.Count > 0)
            {
                foreach (CycleCountDetail cycleCountDetail in cycleCountDetailList)
                {
                    #region У��
                    if (cycleCount.PhyCntGroupBy == BusinessConstants.CODE_MASTER_PHYCNT_GROUPBY_BIN 
                        && cycleCount.Bins != null && cycleCount.Bins.Trim() != string.Empty
                        && cycleCount.Bins.IndexOf(cycleCountDetail.StorageBin) == -1)
                    {
                        throw new BusinessErrorException("MasterData.Inventory.Stocktaking.Error.LocationNotInOrder", cycleCountDetail.StorageBin);
                    }

                    if (cycleCount.Items != null && cycleCount.Items.Trim() != string.Empty
                        && cycleCount.Items.IndexOf(cycleCountDetail.Item.Code) == -1)
                    {
                        throw new BusinessErrorException("MasterData.Inventory.Stocktaking.Error.ItemNotInOrder", cycleCountDetail.Item.Code);
                    }
                    #endregion

                    #region �ж��̵���ϸ�Ƿ�������
                    if (cycleCount.IsScanHu)
                    {
                        if (phyCntedList.Contains(cycleCountDetail.HuId))
                        {
                            throw new BusinessErrorException("MasterData.Inventory.Stocktaking.Error.BarCodeScaned", cycleCountDetail.HuId);
                        }

                        phyCntedList.Add(cycleCountDetail.HuId);

                        #region �ϼܴ���
                        if (isPut && cycleCountDetail.StorageBin != null && cycleCountDetail.StorageBin != string.Empty)
                        {
                            LocationLotDetail locationLotDetail = this.locationLotDetailMgr.LoadHuLocationLotDetail(cycleCountDetail.HuId);
                            if (locationLotDetail == null) 
                            {
                                //���벻�ڿ�λ�еģ�����Ҫ���ϼܴ���
                            }
                            else if (locationLotDetail.Location.Code == cycleCount.Location.Code
                                && (locationLotDetail.StorageBin == null || locationLotDetail.StorageBin.Code != cycleCountDetail.StorageBin))
                            {
                                locationLotDetail.NewStorageBin = this.storageBinMgr.LoadStorageBin(cycleCountDetail.StorageBin);
                                this.locationMgr.InventoryPut(locationLotDetail, user);
                            }
                        }
                        #endregion
                    }
                    else
                    {
                        if (phyCntedList.Contains(cycleCountDetail.Item.Code))
                        {
                            throw new BusinessErrorException("MasterData.Inventory.Stocktaking.Error.ItemProccessed", cycleCountDetail.Item.Code);
                        }

                        phyCntedList.Add(cycleCountDetail.Item.Code);
                    }
                    #endregion

                    cycleCountDetail.CycleCount = cycleCount;
                    if (cycleCountDetail.HuId != null)
                    {
                        cycleCountDetail.HuId = cycleCountDetail.HuId.ToUpper();
                    }
                    cycleCountDetailMgr.CreateCycleCountDetail(cycleCountDetail);
                }
            }

            cycleCount.LastModifyUser = user;
            cycleCount.LastModifyDate = DateTime.Now;
            this.UpdateCycleCount(cycleCount);
        }

        public IList<CycleCountResult> CalcCycleCount(string orderNo)
        {
            return CalcCycleCount(orderNo, true, true, true, null, null);
        }

        public IList<CycleCountResult> CalcCycleCount(string orderNo, bool listShortage, bool listProfit, bool listEqual, IList<string> binList, IList<string> itemList)
        {
            CycleCount cycleCount = this.CheckAndLoadCycleCount(orderNo);

            if (cycleCount.Status != BusinessConstants.CODE_MASTER_STATUS_VALUE_INPROCESS)
            {
                throw new BusinessErrorException("Common.Business.Error.StatusError", cycleCount.Code, cycleCount.Status);
            }

            return DoCalcCycleCount(cycleCount, cycleCount.PhyCntGroupBy, listShortage, listProfit, listEqual, binList, itemList);
        }

        public IList<CycleCountResult> ListCycleCountResult(string orderNo, bool listShortage, bool listProfit, bool listEqual, IList<string> binList, IList<string> itemList)
        {
            CycleCount cycleCount = this.CheckAndLoadCycleCount(orderNo);
            if (cycleCount.Status == BusinessConstants.CODE_MASTER_STATUS_VALUE_CREATE
                || cycleCount.Status == BusinessConstants.CODE_MASTER_STATUS_VALUE_CANCEL)
            {
                throw new BusinessErrorException("Common.Business.Error.StatusError", cycleCount.Code, cycleCount.Status);
            }

            IList<CycleCountResult> cycleCountResultList = new List<CycleCountResult>();

            if (cycleCount.Status == BusinessConstants.CODE_MASTER_STATUS_VALUE_INPROCESS)
            {
                #region ִ���з��ؿ����̵����Ƚ�ֵ
                cycleCountResultList = DoCalcCycleCount(cycleCount, cycleCount.PhyCntGroupBy, listShortage, listProfit, listEqual, binList, itemList); 
                #endregion
            }
            else
            {
                #region ��ɺ�
                if (cycleCount.IsScanHu)
                {
                    #region ������
                    #region �̿�
                    if (listShortage)
                    {
                        DetachedCriteria criteria = DetachedCriteria.For<CycleCountResult>();

                        criteria.SetProjection(Projections.ProjectionList()
                                .Add(Projections.GroupProperty("HuId"))
                                .Add(Projections.GroupProperty("StorageBin"))
                                .Add(Projections.GroupProperty("LotNo"))
                                .Add(Projections.GroupProperty("Item"))
                                .Add(Projections.Sum("Qty"))
                                .Add(Projections.Sum("InvQty"))
                                .Add(Projections.Sum("DiffQty")));

                        criteria.Add(Expression.Eq("CycleCount.Code", cycleCount.Code));
                        criteria.Add(Expression.Lt("DiffQty", decimal.Zero));

                        if (binList != null && binList.Count > 0)
                        {
                            criteria.Add(Expression.In("StorageBin", binList.ToArray<string>()));
                        }

                        if (itemList != null && itemList.Count > 0)
                        {
                            criteria.CreateAlias("Item", "item");
                            criteria.Add(Expression.In("item.Code", itemList.ToArray<string>()));
                        }

                        IList<object[]> shorageList = this.criteriaMgr.FindAll<object[]>(criteria);

                        var shorageResult = from shorage in shorageList
                                            select new CycleCountResult
                                            {
                                                HuId = null,
                                                StorageBin = (string)shorage[1],
                                                LotNo = null,
                                                Item = (Item)shorage[3],
                                                Qty = (decimal)shorage[4],
                                                InvQty = (decimal)shorage[5],
                                                DiffQty = (decimal)shorage[6],
                                                ProfitQty = 0,
                                                ShortageQty = 0 - (decimal)shorage[6],
                                                EqualQty = 0,
                                                Location = cycleCount.Location,
                                            };

                        IListHelper.AddRange<CycleCountResult>(cycleCountResultList, shorageResult.ToList<CycleCountResult>());
                    }
                    #endregion

                    #region ��ӯ
                    if (listProfit)
                    {
                        DetachedCriteria criteria = DetachedCriteria.For<CycleCountResult>();

                        criteria.SetProjection(Projections.ProjectionList()
                                .Add(Projections.GroupProperty("HuId"))
                                .Add(Projections.GroupProperty("StorageBin"))
                                .Add(Projections.GroupProperty("LotNo"))
                                .Add(Projections.GroupProperty("Item"))
                                .Add(Projections.Sum("Qty"))
                                .Add(Projections.Sum("InvQty"))
                                .Add(Projections.Sum("DiffQty")));

                        criteria.Add(Expression.Eq("CycleCount.Code", cycleCount.Code));
                        criteria.Add(Expression.Gt("DiffQty", decimal.Zero));

                        if (binList != null && binList.Count > 0)
                        {
                            criteria.Add(Expression.In("StorageBin", binList.ToArray<string>()));
                        }

                        if (itemList != null && itemList.Count > 0)
                        {
                            criteria.CreateAlias("Item", "item");
                            criteria.Add(Expression.In("item.Code", itemList.ToArray<string>()));
                        }

                        IList<object[]> profitList = this.criteriaMgr.FindAll<object[]>(criteria);

                        var profitResult = from profit in profitList
                                           select new CycleCountResult
                                           {
                                               HuId = null,
                                               StorageBin = (string)profit[1],
                                               LotNo = null,
                                               Item = (Item)profit[3],
                                               Qty = (decimal)profit[4],
                                               InvQty = (decimal)profit[5],
                                               DiffQty = (decimal)profit[6],
                                               ProfitQty = (decimal)profit[6],
                                               ShortageQty = 0,
                                               EqualQty = 0,
                                               Location = cycleCount.Location,
                                           };


                        IListHelper.AddRange<CycleCountResult>(cycleCountResultList, profitResult.ToList<CycleCountResult>());
                    }
                    #endregion

                    #region ��ƽ
                    if (listEqual)
                    {
                        DetachedCriteria criteria = DetachedCriteria.For<CycleCountResult>();

                        criteria.SetProjection(Projections.ProjectionList()
                                .Add(Projections.GroupProperty("HuId"))
                                .Add(Projections.GroupProperty("StorageBin"))
                                .Add(Projections.GroupProperty("LotNo"))
                                .Add(Projections.GroupProperty("Item"))
                                .Add(Projections.Sum("Qty"))
                                .Add(Projections.Sum("InvQty"))
                                .Add(Projections.Sum("DiffQty")));

                        criteria.Add(Expression.Eq("CycleCount.Code", cycleCount.Code));
                        criteria.Add(Expression.Eq("DiffQty", decimal.Zero));

                        if (binList != null && binList.Count > 0)
                        {
                            criteria.Add(Expression.In("StorageBin", binList.ToArray<string>()));
                        }

                        if (itemList != null && itemList.Count > 0)
                        {
                            criteria.CreateAlias("Item", "item");
                            criteria.Add(Expression.In("item.Code", itemList.ToArray<string>()));
                        }

                        IList<object[]> equalList = this.criteriaMgr.FindAll<object[]>(criteria);

                        var equalResult = from equal in equalList
                                          select new CycleCountResult
                                          {
                                              HuId = null,
                                              StorageBin = (string)equal[1],
                                              LotNo = null,
                                              Item = (Item)equal[3],
                                              Qty = (decimal)equal[4],
                                              InvQty = (decimal)equal[5],
                                              DiffQty = (decimal)equal[6],
                                              ProfitQty = 0,
                                              ShortageQty = 0,
                                              EqualQty = (decimal)equal[4],
                                              Location = cycleCount.Location,
                                          };


                        IListHelper.AddRange<CycleCountResult>(cycleCountResultList, equalResult.ToList<CycleCountResult>());
                    }
                    #endregion

                    #region ����
                    var sumList = from cycleCountResult in cycleCountResultList
                                  group cycleCountResult by new { StorageBin = cycleCountResult.StorageBin, Item = cycleCountResult.Item } into result
                                  select new CycleCountResult
                                  {
                                      HuId = null,
                                      StorageBin = (string)result.Key.StorageBin,
                                      LotNo = null,
                                      Item = (Item)result.Key.Item,
                                      Qty = result.Sum(cycleCountResult => cycleCountResult.Qty),
                                      InvQty = result.Sum(cycleCountResult => cycleCountResult.InvQty),
                                      DiffQty = result.Sum(cycleCountResult => cycleCountResult.DiffQty),
                                      ProfitQty = result.Sum(cycleCountResult => cycleCountResult.ProfitQty),
                                      ShortageQty = result.Sum(cycleCountResult => cycleCountResult.ShortageQty),
                                      EqualQty = result.Sum(cycleCountResult => cycleCountResult.EqualQty),
                                      Location = cycleCount.Location,
                                  };

                    cycleCountResultList = sumList.ToList<CycleCountResult>();
                    #endregion

                    #endregion
                }
                else
                {
                    #region ������
                    cycleCountResultList = FindCycleCountResult(orderNo, listShortage, listProfit, listEqual, binList, itemList);
                    #endregion
                }
                #endregion

            }

            return cycleCountResultList;
        }

        public IList<CycleCountResult> ListCycleCountResultDetail(string orderNo, bool listShortage, bool listProfit, bool listEqual, IList<string> binList, IList<string> itemList)
        {
            return ListCycleCountResultDetail(orderNo, listShortage, listProfit, listEqual, binList, itemList, false);
        }

        public IList<CycleCountResult> ListCycleCountResultDetail(string orderNo, bool listShortage, bool listProfit, bool listEqual, IList<string> binList, IList<string> itemList, bool isOnFloor)
        {
            CycleCount cycleCount = this.CheckAndLoadCycleCount(orderNo);
            if (cycleCount.Status == BusinessConstants.CODE_MASTER_STATUS_VALUE_CREATE
                || cycleCount.Status == BusinessConstants.CODE_MASTER_STATUS_VALUE_CANCEL)
            {
                throw new BusinessErrorException("Common.Business.Error.StatusError", cycleCount.Code, cycleCount.Status);
            }

            IList<CycleCountResult> cycleCountResultList = new List<CycleCountResult>();
            if (cycleCount.Status == BusinessConstants.CODE_MASTER_STATUS_VALUE_INPROCESS)
            {
                #region In-Process
                cycleCountResultList = DoCalcCycleCount(cycleCount, BusinessConstants.CODE_MASTER_PHYCNT_GROUPBY_NOGROUP, listShortage, listProfit, listEqual, binList, itemList, isOnFloor);
                #endregion
            }
            else
            {
                #region ��ɺ�
                cycleCountResultList = FindCycleCountResult(orderNo, listShortage, listProfit, listEqual, binList, itemList, isOnFloor);
                #endregion
            }

            return cycleCountResultList;
        }

        [Transaction(TransactionMode.Requires)]
        public void CompleteCycleCount(string orderNo, User user)
        {
            CycleCount cycleCount = this.CheckAndLoadCycleCount(orderNo);
            if (cycleCount.Status != BusinessConstants.CODE_MASTER_STATUS_VALUE_INPROCESS)
            {
                throw new BusinessErrorException("Common.Business.Error.StatusError", cycleCount.Code, cycleCount.Status);
            }

            IList<CycleCountResult> resultList = DoCalcCycleCount(cycleCount, BusinessConstants.CODE_MASTER_PHYCNT_GROUPBY_NOGROUP, true, true, true, null, null);
            if (resultList != null && resultList.Count > 0)
            {
                foreach (CycleCountResult result in resultList)
                {
                    result.CycleCount = cycleCount;
                    this.cycleCountResultMgr.CreateCycleCountResult(result);
                }
            }

            cycleCount.CompleteUser = user.Code;
            cycleCount.CompleteDate = DateTime.Now;
            cycleCount.LastModifyUser = user;
            cycleCount.LastModifyDate = cycleCount.CompleteDate.Value;

            DetachedCriteria criteria = DetachedCriteria.For<CycleCountResult>();
            criteria.CreateCriteria("CycleCount", "cc");
            criteria.SetProjection(Projections.Count("Id"));
            criteria.Add(Expression.Eq("cc.Code", cycleCount.Code));
            criteria.Add(Expression.Not(Expression.Eq("DiffQty", decimal.Zero)));
            criteria.Add(Expression.IsNull("IsProcessed"));

            IList<int> count = this.criteriaMgr.FindAll<int>(criteria);

            if (count[0] == 0)
            {
                cycleCount.CloseUser = user;
                cycleCount.CloseDate = cycleCount.LastModifyDate;
                cycleCount.Status = BusinessConstants.CODE_MASTER_STATUS_VALUE_CLOSE;
            }
            else
            {
                cycleCount.Status = BusinessConstants.CODE_MASTER_STATUS_VALUE_COMPLETE;
            }

            this.UpdateCycleCount(cycleCount);
        }

        [Transaction(TransactionMode.Requires)]
        public void ManualCloseCycleCount(string orderNo, User user)
        {
            CycleCount cycleCount = this.CheckAndLoadCycleCount(orderNo);
            if (cycleCount.Status != BusinessConstants.CODE_MASTER_STATUS_VALUE_COMPLETE)
            {
                throw new BusinessErrorException("Common.Business.Error.StatusError", cycleCount.Code, cycleCount.Status);
            }

            cycleCount.Status = BusinessConstants.CODE_MASTER_STATUS_VALUE_CLOSE;
            cycleCount.CloseUser = user;
            cycleCount.CloseDate = DateTime.Now;
            cycleCount.LastModifyUser = user;
            cycleCount.LastModifyDate = cycleCount.CloseDate.Value;
            this.UpdateCycleCount(cycleCount);
        }

        [Transaction(TransactionMode.Requires)]
        public void CancelCycleCount(string orderNo, User user)
        {
            CycleCount cycleCount = this.CheckAndLoadCycleCount(orderNo);
            if (cycleCount.Status != BusinessConstants.CODE_MASTER_STATUS_VALUE_INPROCESS)
            {
                throw new BusinessErrorException("Common.Business.Error.StatusError", cycleCount.Code, cycleCount.Status);
            }

            cycleCount.Status = BusinessConstants.CODE_MASTER_STATUS_VALUE_CANCEL;
            cycleCount.CancelUser = user.Code;
            cycleCount.CancelDate = DateTime.Now;
            cycleCount.LastModifyUser = user;
            cycleCount.LastModifyDate = cycleCount.CancelDate.Value;
            this.UpdateCycleCount(cycleCount);
        }

        [Transaction(TransactionMode.Requires)]
        public void ProcessCycleCountResult(string orderNo, User user)
        {
            CycleCount cycleCount = this.CheckAndLoadCycleCount(orderNo);
            if (cycleCount.Status != BusinessConstants.CODE_MASTER_STATUS_VALUE_COMPLETE)
            {
                throw new BusinessErrorException("Common.Business.Error.StatusError", cycleCount.Code, cycleCount.Status);
            }

            DetachedCriteria criteria = DetachedCriteria.For<CycleCountResult>();

            criteria.CreateAlias("CycleCount", "cc");

            criteria.SetProjection(Projections.ProjectionList()
                    .Add(Projections.GroupProperty("Id")));

            criteria.Add(Expression.Eq("cc.Code", cycleCount.Code));
            criteria.Add(Expression.Not(Expression.Eq("DiffQty", decimal.Zero)));

            IList<int> cycleCountResultIdList = this.criteriaMgr.FindAll<int>(criteria);

            this.ProcessCycleCountResult(cycleCountResultIdList, user);
        }

        [Transaction(TransactionMode.Requires)]
        public void ProcessCycleCountResult(CycleCount cycCnt, User user)
        {
            this.ProcessCycleCountResult(cycCnt.Code, user);
        }

        [Transaction(TransactionMode.Requires)]
        public void ProcessCycleCountResult(IList<int> cycleCountResultIdList, User user)
        {
            if (cycleCountResultIdList != null && cycleCountResultIdList.Count > 0)
            {
                IDictionary<string, CycleCount> cycleCountDic = new Dictionary<string, CycleCount>();
                foreach (int id in cycleCountResultIdList)
                {
                    CycleCountResult cycleCountResult = this.cycleCountResultMgr.LoadCycleCountResult(id);

                    if (cycleCountResult.DiffQty != 0)
                    {
                        if (!cycleCountResult.IsProcessed.HasValue || !cycleCountResult.IsProcessed.Value)
                        {
                            if (!cycleCountDic.ContainsKey(cycleCountResult.CycleCount.Code))
                            {
                                if (cycleCountResult.CycleCount.Status != BusinessConstants.CODE_MASTER_STATUS_VALUE_COMPLETE)
                                {
                                    throw new BusinessErrorException("Common.Business.Error.StatusError", cycleCountResult.CycleCount.Code, cycleCountResult.CycleCount.Status);
                                }
                                cycleCountDic.Add(cycleCountResult.CycleCount.Code, cycleCountResult.CycleCount);
                            }

                            this.locationMgr.InventoryAdjust(cycleCountResult, user);

                            cycleCountResult.IsProcessed = true;
                            this.cycleCountResultMgr.UpdateCycleCountResult(cycleCountResult);
                        }
                        else
                        {
                            if (cycleCountResult.HuId != null && cycleCountResult.HuId.Trim() != string.Empty)
                            {
                                throw new BusinessErrorException("MasterData.Inventory.Stocktaking.Error.BarCodeAdjested", cycleCountResult.CycleCount.Code, cycleCountResult.HuId);
                            }
                            else
                            {
                                throw new BusinessErrorException("MasterData.Inventory.Stocktaking.Error.ItemAdjested", cycleCountResult.CycleCount.Code, cycleCountResult.Item.Code);
                            }
                        }
                    }
                }

                this.FlushSession();
                this.CleanSession();

                if (cycleCountDic.Count > 0)
                {
                    foreach (CycleCount cycleCount in cycleCountDic.Values)
                    {
                        DetachedCriteria criteria = DetachedCriteria.For<CycleCountResult>();
                        criteria.CreateCriteria("CycleCount", "cc");
                        criteria.SetProjection(Projections.Count("Id"));
                        criteria.Add(Expression.Eq("cc.Code", cycleCount.Code));
                        criteria.Add(Expression.Not(Expression.Eq("DiffQty", decimal.Zero)));
                        criteria.Add(Expression.IsNull("IsProcessed"));

                        IList<int> count = this.criteriaMgr.FindAll<int>(criteria);

                        cycleCount.LastModifyUser = user;
                        cycleCount.LastModifyDate = DateTime.Now;
                        if (count[0] == 0)
                        {
                            cycleCount.CloseUser = user;
                            cycleCount.CloseDate = cycleCount.LastModifyDate;
                            cycleCount.Status = BusinessConstants.CODE_MASTER_STATUS_VALUE_CLOSE;
                        }

                        this.UpdateCycleCount(cycleCount);
                    }
                }
            }
        }

        #region Private Method
        public IList<CycleCountResult> DoCalcCycleCount(CycleCount cycleCount, string phyCntGroupBy, bool listShortage, bool listProfit, bool listEqual, IList<string> binList, IList<string> itemList)
        {
            return DoCalcCycleCount(cycleCount, phyCntGroupBy, listShortage, listProfit, listEqual, binList, itemList, false);
        }

        public IList<CycleCountResult> DoCalcCycleCount(CycleCount cycleCount, string phyCntGroupBy, bool listShortage, bool listProfit, bool listEqual, IList<string> binList, IList<string> itemList, bool isOnFloor)
        {
            IList<CycleCountResult> cycleCountResultList = new List<CycleCountResult>();

            if (cycleCount.IsScanHu)
            {
                #region �������̵�
                #region ���ҿ��
                DetachedCriteria criteria = DetachedCriteria.For<LocationLotDetail>();

                criteria.CreateAlias("Location", "loc");
                criteria.CreateAlias("Hu", "hu");
                criteria.CreateAlias("StorageBin", "bin", JoinType.LeftOuterJoin);
                criteria.CreateAlias("Item", "item");

                criteria.SetProjection(Projections.ProjectionList()
                        .Add(Projections.GroupProperty("hu.HuId"))
                        .Add(Projections.GroupProperty("bin.Code"))
                        .Add(Projections.GroupProperty("hu.LotNo"))
                        .Add(Projections.GroupProperty("Item"))
                        .Add(Projections.Sum("Qty")));

                criteria.Add(Expression.Eq("loc.Code", cycleCount.Location.Code));
                criteria.Add(Expression.IsNotNull("Hu"));
                criteria.Add(Expression.Not(Expression.Eq("Qty", decimal.Zero)));
                if (cycleCount.Type == BusinessConstants.CODE_MASTER_PHYCNT_TYPE_CLASSIFYINGCHECK)
                {
                    criteria.Add(Expression.IsNotNull("StorageBin"));
                }

                if (isOnFloor && phyCntGroupBy == BusinessConstants.CODE_MASTER_PHYCNT_GROUPBY_BIN)
                {
                    criteria.Add(Expression.IsNull("StorageBin"));
                }

                if (binList != null && binList.Count > 0)
                {
                    criteria.Add(Expression.In("bin.Code", binList.ToArray<string>()));
                }
                else if (cycleCount.Bins != null && cycleCount.Bins.Trim() != string.Empty)
                {
                    criteria.Add(Expression.In("bin.Code", cycleCount.Bins.Split('|')));
                }

                if (itemList != null && itemList.Count > 0)
                {
                    criteria.Add(Expression.In("item.Code", itemList.ToArray<string>()));
                }
                else if (cycleCount.Items != null && cycleCount.Items.Trim() != string.Empty)
                {
                    criteria.Add(Expression.In("item.Code", cycleCount.Items.Split('|')));
                }

                IList<object[]> invList = this.criteriaMgr.FindAll<object[]>(criteria);
                #endregion

                #region �����̵���
                criteria = DetachedCriteria.For<CycleCountDetail>();

                criteria.SetProjection(Projections.ProjectionList()
                        .Add(Projections.GroupProperty("HuId"))
                        .Add(Projections.GroupProperty("StorageBin"))
                        .Add(Projections.GroupProperty("LotNo"))
                        .Add(Projections.GroupProperty("Item"))
                         .Add(Projections.Sum("Qty")));

                criteria.Add(Expression.Eq("CycleCount.Code", cycleCount.Code));

                if (isOnFloor && phyCntGroupBy == BusinessConstants.CODE_MASTER_PHYCNT_GROUPBY_BIN)
                {
                    criteria.Add(Expression.IsNull("StorageBin"));
                }

                if (binList != null && binList.Count > 0)
                {
                    criteria.Add(Expression.In("StorageBin", binList.ToArray<string>()));
                }

                if (itemList != null && itemList.Count > 0)
                {
                    criteria.CreateAlias("Item", "item");
                    criteria.Add(Expression.In("item.Code", itemList.ToArray<string>()));
                }

                IList<object[]> cycDetList = this.criteriaMgr.FindAll<object[]>(criteria);
                #endregion

                #region �̿�\��Ӯ
                IEqualityComparer<object[]> comparer;
                if (phyCntGroupBy == BusinessConstants.CODE_MASTER_PHYCNT_GROUPBY_LOCATION)
                {
                    //�������ܣ��������
                    comparer = new HuComparer();
                }
                else if (phyCntGroupBy == BusinessConstants.CODE_MASTER_PHYCNT_GROUPBY_BIN)
                {
                    //��������
                    comparer = new HuBinComparer();
                }
                else
                {
                    //������ = �����������Ƚ�
                    if (cycleCount.PhyCntGroupBy == BusinessConstants.CODE_MASTER_PHYCNT_GROUPBY_LOCATION)
                    {
                        comparer = new HuComparer();
                    }
                    else
                    {
                        comparer = new HuBinComparer();
                    }
                }

                IEnumerable<object[]> shortageList = listShortage ? invList.Except(cycDetList, comparer) : null;  //�̿�
                IEnumerable<object[]> profitList = listProfit ? cycDetList.Except(invList, comparer) : null;    //��Ӯ
                IEnumerable<object[]> intersectList = listEqual ? invList.Intersect(cycDetList, comparer) : null;  //����                

                #region �̿�
                if (shortageList != null)
                {
                    if (phyCntGroupBy == BusinessConstants.CODE_MASTER_PHYCNT_GROUPBY_LOCATION)
                    {
                        #region ����λ���������
                        var shortageResult = from shortage in shortageList
                                             group shortage by shortage[3] into result
                                             select new CycleCountResult
                                             {
                                                 HuId = null,
                                                 StorageBin = null,
                                                 LotNo = null,
                                                 Item = (Item)result.Key,
                                                 Qty = 0,
                                                 InvQty = result.Sum(shortage => (Decimal)shortage[4]),
                                                 DiffQty = 0 - result.Sum(shortage => (Decimal)shortage[4]),
                                                 ShortageQty = result.Sum(shortage => (Decimal)shortage[4]),
                                                 Location = cycleCount.Location,
                                             };

                        IListHelper.AddRange<CycleCountResult>(cycleCountResultList, shortageResult.ToList<CycleCountResult>());
                        #endregion
                    }
                    else if (phyCntGroupBy == BusinessConstants.CODE_MASTER_PHYCNT_GROUPBY_BIN)
                    {
                        #region ������������
                        var shortageResult = from shortage in shortageList
                                             group shortage by new { Bin = shortage[1], Item = shortage[3] } into result
                                             select new CycleCountResult
                                             {
                                                 HuId = null,
                                                 StorageBin = (string)result.Key.Bin,
                                                 LotNo = null,
                                                 Item = (Item)result.Key.Item,
                                                 Qty = 0,
                                                 InvQty = result.Sum(shortage => (Decimal)shortage[4]),
                                                 DiffQty = 0 - result.Sum(shortage => (Decimal)shortage[4]),
                                                 ShortageQty = result.Sum(shortage => (Decimal)shortage[4]),
                                                 Location = cycleCount.Location,
                                             };

                        IListHelper.AddRange<CycleCountResult>(cycleCountResultList, shortageResult.ToList<CycleCountResult>());
                        #endregion
                    }
                    else
                    {
                        #region ������
                        var shortageResult = from shortage in shortageList
                                             select new CycleCountResult
                                             {
                                                 HuId = (string)shortage[0],
                                                 StorageBin = (string)shortage[1],
                                                 LotNo = (string)shortage[2],
                                                 Item = (Item)shortage[3],
                                                 Qty = 0,
                                                 InvQty = (Decimal)shortage[4],
                                                 DiffQty = 0 - (Decimal)shortage[4],
                                                 ShortageQty = 0 - (Decimal)shortage[4],
                                                 Location = cycleCount.Location,
                                             };

                        IListHelper.AddRange<CycleCountResult>(cycleCountResultList, shortageResult.ToList<CycleCountResult>());
                        #endregion
                    }
                }
                #endregion

                #region ��ӯ
                if (profitList != null)
                {
                    if (phyCntGroupBy == BusinessConstants.CODE_MASTER_PHYCNT_GROUPBY_LOCATION)
                    {
                        #region ����λ���������
                        var profitResult = from profit in profitList
                                           group profit by profit[3] into result
                                           select new CycleCountResult
                                           {
                                               HuId = null,
                                               StorageBin = null,
                                               LotNo = null,
                                               Item = (Item)result.Key,
                                               Qty = result.Sum(profit => (Decimal)profit[4]),
                                               InvQty = 0,
                                               DiffQty = result.Sum(profit => (Decimal)profit[4]),
                                               ProfitQty = result.Sum(profit => (Decimal)profit[4]),
                                               Location = cycleCount.Location,

                                           };

                        IListHelper.AddRange<CycleCountResult>(cycleCountResultList, profitResult.ToList<CycleCountResult>());
                        #endregion
                    }
                    else if (phyCntGroupBy == BusinessConstants.CODE_MASTER_PHYCNT_GROUPBY_BIN)
                    {
                        #region ������������
                        var profitResult = from profit in profitList
                                           group profit by new { Bin = profit[1], Item = profit[3] } into result
                                           select new CycleCountResult
                                           {
                                               HuId = null,
                                               StorageBin = (string)result.Key.Bin,
                                               LotNo = null,
                                               Item = (Item)result.Key.Item,
                                               Qty = result.Sum(profit => (Decimal)profit[4]),
                                               InvQty = 0,
                                               DiffQty = result.Sum(profit => (Decimal)profit[4]),
                                               ProfitQty = result.Sum(profit => (Decimal)profit[4]),
                                               Location = cycleCount.Location,
                                           };

                        IListHelper.AddRange<CycleCountResult>(cycleCountResultList, profitResult.ToList<CycleCountResult>());
                        #endregion
                    }
                    else
                    {
                        #region ������
                        var profitResult = from profit in profitList
                                           select new CycleCountResult
                                           {
                                               HuId = (string)profit[0],
                                               StorageBin = (string)profit[1],
                                               LotNo = (string)profit[2],
                                               Item = (Item)profit[3],
                                               Qty = (Decimal)profit[4],
                                               InvQty = 0,
                                               DiffQty = (Decimal)profit[4],
                                               ProfitQty = (Decimal)profit[4],
                                               Location = cycleCount.Location,
                                           };

                        IListHelper.AddRange<CycleCountResult>(cycleCountResultList, profitResult.ToList<CycleCountResult>());
                        #endregion
                    }

                }
                #endregion

                #region ��ƽ
                if (intersectList != null)
                {
                    if (phyCntGroupBy == BusinessConstants.CODE_MASTER_PHYCNT_GROUPBY_LOCATION)
                    {
                        #region ����λ���������
                        var intersectResult = from intersect in intersectList
                                              group intersect by intersect[3] into result
                                              select new CycleCountResult
                                              {
                                                  HuId = null,
                                                  StorageBin = null,
                                                  LotNo = null,
                                                  Item = (Item)result.Key,
                                                  Qty = result.Sum(intersect => (Decimal)intersect[4]),
                                                  InvQty = result.Sum(intersect => (Decimal)intersect[4]),
                                                  DiffQty = 0,
                                                  EqualQty = result.Sum(intersect => (Decimal)intersect[4]),
                                                  Location = cycleCount.Location,
                                              };

                        IListHelper.AddRange<CycleCountResult>(cycleCountResultList, intersectResult.ToList<CycleCountResult>());
                        #endregion
                    }
                    else if (phyCntGroupBy == BusinessConstants.CODE_MASTER_PHYCNT_GROUPBY_BIN)
                    {
                        #region ������������
                        var intersectResult = from intersect in intersectList
                                              group intersect by new { Bin = intersect[1], Item = intersect[3] } into result
                                              select new CycleCountResult
                                              {
                                                  HuId = null,
                                                  StorageBin = (string)result.Key.Bin,
                                                  LotNo = null,
                                                  Item = (Item)result.Key.Item,
                                                  Qty = result.Sum(intersect => (Decimal)intersect[4]),
                                                  InvQty = 0,
                                                  DiffQty = result.Sum(intersect => (Decimal)intersect[4]),
                                                  EqualQty = result.Sum(intersect => (Decimal)intersect[4]),
                                                  Location = cycleCount.Location,
                                              };

                        IListHelper.AddRange<CycleCountResult>(cycleCountResultList, intersectResult.ToList<CycleCountResult>());
                        #endregion
                    }
                    else
                    {
                        #region ������
                        var intersectResult = from intersect in intersectList
                                              select new CycleCountResult
                                              {
                                                  HuId = (string)intersect[0],
                                                  StorageBin = (string)intersect[1],
                                                  LotNo = (string)intersect[2],
                                                  Item = (Item)intersect[3],
                                                  Qty = (Decimal)intersect[4],
                                                  InvQty = (Decimal)intersect[4],
                                                  DiffQty = 0,
                                                  EqualQty = (Decimal)intersect[4],
                                                  Location = cycleCount.Location,
                                              };

                        IListHelper.AddRange<CycleCountResult>(cycleCountResultList, intersectResult.ToList<CycleCountResult>());
                        #endregion
                    }

                }
                #endregion

                #region ����

                if (cycleCountResultList != null)
                {
                    if (phyCntGroupBy == BusinessConstants.CODE_MASTER_PHYCNT_GROUPBY_LOCATION)
                    {
                        #region ����λ���������
                        var totalCycleCountResult = from cycleCountResult in cycleCountResultList
                                                    group cycleCountResult by cycleCountResult.Item into result
                                                    select new CycleCountResult
                                                    {
                                                        HuId = null,
                                                        StorageBin = null,
                                                        LotNo = null,
                                                        Item = (Item)result.Key,
                                                        Qty = result.Sum(cycleCountResult => cycleCountResult.Qty),
                                                        InvQty = result.Sum(cycleCountResult => cycleCountResult.InvQty),
                                                        DiffQty = result.Sum(cycleCountResult => cycleCountResult.Qty) - result.Sum(cycleCountResult => (Decimal)cycleCountResult.InvQty),
                                                        ProfitQty = result.Sum(cycleCountResult => cycleCountResult.ProfitQty),
                                                        ShortageQty = result.Sum(cycleCountResult => cycleCountResult.ShortageQty),
                                                        EqualQty = result.Sum(cycleCountResult => cycleCountResult.EqualQty),
                                                        Location = cycleCount.Location,

                                                    };

                        cycleCountResultList = totalCycleCountResult.ToList<CycleCountResult>();
                        #endregion
                    }
                    else if (phyCntGroupBy == BusinessConstants.CODE_MASTER_PHYCNT_GROUPBY_BIN)
                    {
                        #region ������������
                        var totalCycleCountResult = from cycleCountResult in cycleCountResultList
                                                    group cycleCountResult by new { StorageBin = cycleCountResult.StorageBin, Item = cycleCountResult.Item } into result
                                                    select new CycleCountResult
                                                    {
                                                        HuId = null,
                                                        StorageBin = (string)result.Key.StorageBin,
                                                        LotNo = null,
                                                        Item = (Item)result.Key.Item,
                                                        Qty = result.Sum(cycleCountResult => cycleCountResult.Qty),
                                                        InvQty = result.Sum(cycleCountResult => cycleCountResult.InvQty),
                                                        DiffQty = result.Sum(cycleCountResult => cycleCountResult.Qty) - result.Sum(cycleCountResult => (Decimal)cycleCountResult.InvQty),
                                                        ProfitQty = result.Sum(cycleCountResult => cycleCountResult.ProfitQty),
                                                        ShortageQty = result.Sum(cycleCountResult => cycleCountResult.ShortageQty),
                                                        EqualQty = result.Sum(cycleCountResult => cycleCountResult.EqualQty),
                                                        Location = cycleCount.Location,
                                                    };


                        cycleCountResultList = totalCycleCountResult.ToList<CycleCountResult>();
                        #endregion
                    }
                    else
                    {
                        #region ������

                        #endregion
                    }

                }

                #endregion

                #endregion
                #endregion
            }
            else
            {
                #region �������̵�
                #region ���ҿ��
                DetachedCriteria criteria = DetachedCriteria.For<LocationLotDetail>();

                criteria.CreateAlias("Location", "loc");
                criteria.CreateAlias("Item", "item");

                criteria.SetProjection(Projections.ProjectionList()
                    .Add(Projections.GroupProperty("Item"))
                    .Add(Projections.Sum("Qty")));

                criteria.Add(Expression.Eq("loc.Code", cycleCount.Location.Code));
                criteria.Add(Expression.IsNull("Hu"));
                criteria.Add(Expression.Not(Expression.Eq("Qty", decimal.Zero)));

                if (itemList != null && itemList.Count > 0)
                {
                    criteria.Add(Expression.In("item.Code", itemList.ToArray<string>()));
                }
                else if (cycleCount.Items != null && cycleCount.Items.Trim() != string.Empty)
                {
                    criteria.Add(Expression.In("item.Code", cycleCount.Items.Split('|')));
                }

                IList<object[]> invList = this.criteriaMgr.FindAll<object[]>(criteria);
                #endregion

                #region �����̵���
                criteria = DetachedCriteria.For<CycleCountDetail>();

                criteria.SetProjection(Projections.ProjectionList()
                    .Add(Projections.GroupProperty("Item"))
                    .Add(Projections.Sum("Qty")));

                criteria.Add(Expression.Eq("CycleCount.Code", cycleCount.Code));

                if (itemList != null && itemList.Count > 0)
                {
                    criteria.CreateAlias("Item", "item");
                    criteria.Add(Expression.In("item.Code", itemList.ToArray<string>()));
                }

                IList<object[]> cycDetList = this.criteriaMgr.FindAll<object[]>(criteria);
                #endregion

                #region �ϲ���ѯ���
                var result = from inv in invList
                             join cycDet in cycDetList
                             on ((Item)inv[0]).Code.Trim() equals ((Item)cycDet[0]).Code.Trim()
                             select new CycleCountResult
                             {
                                 Item = (Item)inv[0],
                                 Qty = (decimal)cycDet[1],
                                 InvQty = (decimal)inv[1],
                                 DiffQty = (decimal)cycDet[1] - (decimal)inv[1],
                                 Location = cycleCount.Location,
                             };

                IListHelper.AddRange<CycleCountResult>(cycleCountResultList, result.ToList<CycleCountResult>());

                if (!(listShortage && listProfit && listEqual))
                {
                    if (listShortage)
                    {
                        if (listProfit)
                        {
                            cycleCountResultList = cycleCountResultList.Where(c => c.DiffQty != 0).ToList<CycleCountResult>();
                        }
                        else if (listEqual)
                        {
                            cycleCountResultList = cycleCountResultList.Where(c => c.DiffQty <= 0).ToList<CycleCountResult>();
                        }
                        else
                        {
                            cycleCountResultList = cycleCountResultList.Where(c => c.DiffQty < 0).ToList<CycleCountResult>();
                        }
                    }
                    else if (listProfit)
                    {
                        if (listEqual)
                        {
                            cycleCountResultList = cycleCountResultList.Where(c => c.DiffQty >= 0).ToList<CycleCountResult>();
                        }
                        else
                        {
                            cycleCountResultList = cycleCountResultList.Where(c => c.DiffQty > 0).ToList<CycleCountResult>();
                        }
                    }
                    else
                    {
                        cycleCountResultList = cycleCountResultList.Where(c => c.DiffQty == 0).ToList<CycleCountResult>();
                    }
                }
                #endregion

                #region �̿��������ȫû���̵�
                IEnumerable<object[]> shortageList = listShortage ? invList.Except(cycDetList, new ItemComparer()) : null;      //�̿�

                if (shortageList != null)
                {
                    var shortageResult = from shortage in shortageList
                                         select new CycleCountResult
                                         {
                                             Item = (Item)shortage[0],
                                             Qty = 0,
                                             InvQty = (decimal)shortage[1],
                                             DiffQty = 0 - (decimal)shortage[1],
                                             Location = cycleCount.Location,
                                         };

                    IListHelper.AddRange<CycleCountResult>(cycleCountResultList, shortageResult.ToList<CycleCountResult>());
                }
                #endregion

                #region ��Ӯ�����û�����
                IEnumerable<object[]> profitList = listProfit ? cycDetList.Except(invList, new ItemComparer()) : null;      //��Ӯ

                if (profitList != null)
                {
                    var profitResult = from profit in profitList
                                       select new CycleCountResult
                                       {
                                           Item = (Item)profit[0],
                                           Qty = (decimal)profit[1],
                                           InvQty = decimal.Zero,
                                           DiffQty = (decimal)profit[1],
                                           Location = cycleCount.Location,
                                       };

                    IListHelper.AddRange<CycleCountResult>(cycleCountResultList, profitResult.ToList<CycleCountResult>());
                }
                #endregion
                #endregion
            }

            return cycleCountResultList;
        }

        private IList<CycleCountResult> FindCycleCountResult(string orderNo, bool listShortage, bool listProfit, bool listEqual, IList<string> binList, IList<string> itemList)
        {
            return FindCycleCountResult(orderNo, listShortage, listProfit, listEqual, binList, itemList, false);
        }

        private IList<CycleCountResult> FindCycleCountResult(string orderNo, bool listShortage, bool listProfit, bool listEqual, IList<string> binList, IList<string> itemList, bool isOnFloor)
        {
            DetachedCriteria criteria = DetachedCriteria.For<CycleCountResult>();

            criteria.Add(Expression.Eq("CycleCount.Code", orderNo));

            if (!(listShortage && listProfit && listEqual))
            {
                if (listShortage)
                {
                    if (listProfit)
                    {
                        criteria.Add(Expression.Not(Expression.Eq("DiffQty", decimal.Zero)));
                    }
                    else if (listEqual)
                    {
                        criteria.Add(Expression.Le("DiffQty", decimal.Zero));
                    }
                    else
                    {
                        criteria.Add(Expression.Lt("DiffQty", decimal.Zero));
                    }
                }
                else if (listProfit)
                {
                    if (listEqual)
                    {
                        criteria.Add(Expression.Ge("DiffQty", decimal.Zero));
                    }
                    else
                    {
                        criteria.Add(Expression.Gt("DiffQty", decimal.Zero));
                    }
                }
                else
                {
                    criteria.Add(Expression.Eq("DiffQty", decimal.Zero));
                }
            }

            if (binList != null && binList.Count > 0)
            {
                criteria.Add(Expression.In("StorageBin", binList.ToArray<string>()));
            }

            if (isOnFloor)
            {
                criteria.Add(Expression.IsNull("StorageBin"));
            }

            if (itemList != null && itemList.Count > 0)
            {
                criteria.CreateAlias("Item", "item");
                criteria.Add(Expression.In("item.Code", itemList.ToArray<string>()));
            }

            return this.criteriaMgr.FindAll<CycleCountResult>(criteria);
        }
        #endregion
        #endregion

    }

    class HuBinComparer : IEqualityComparer<object[]>
    {
        public bool Equals(object[] x, object[] y)
        {
            return (string)x[0] == (string)y[0] && (string)x[1] == (string)y[1];
        }

        public int GetHashCode(object[] obj)
        {
            string hCode = (string)obj[0] + "|" + (string)obj[1];
            return hCode.GetHashCode();
        }
    }

    class HuComparer : IEqualityComparer<object[]>
    {
        public bool Equals(object[] x, object[] y)
        {
            return (string)x[0] == (string)y[0];

        }

        public int GetHashCode(object[] obj)
        {
            return obj[0].GetHashCode();
        }
    }

    class ItemComparer : IEqualityComparer<object[]>
    {
        public bool Equals(object[] x, object[] y)
        {
            return ((Item)x[0]).Code.Trim() == ((Item)y[0]).Code.Trim();
        }

        public int GetHashCode(object[] obj)
        {
            string hCode = ((Item)obj[0]).Code.Trim();
            return hCode.GetHashCode();
        }
    }
}