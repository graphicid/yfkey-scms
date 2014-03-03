using System;
using System.Collections;
using System.Collections.Generic;
using Castle.Services.Transaction;
using com.Sconit.Entity;
using com.Sconit.Entity.MasterData;
using com.Sconit.Persistence.MasterData;
using NHibernate.Expression;
using com.Sconit.Entity.Exception;
using com.Mes.Dss.Persistence;
using com.Mes.Dss.Entity;
using com.Sconit.Entity.Mes;
using com.Sconit.Service.Mes;
using com.Sconit.Service.MasterData;
using com.Sconit.Service.Criteria;

//TODO: Add other using statements here.

namespace com.Mes.Dss.Service.Impl
{
    [Transactional]
    public class MesDssOutMgr : IMesDssOutMgr
    {

        public IScmsTableIndexMgr scmsTableIndexMgr { get; set; }
        public IScmsMaterialBarcodeMgr scmsMaterialBarcodeMgr { get; set; }
        public IScmsWorkOrderMgr scmsWorkOrderMgr { get; set; }
        public IScmsPartMgr scmsPartMgr { get; set; }
        public IScmsBomMgr scmsBomMgr { get; set; }
        public IMesBomDetailMgr mesBomDetailMgr { get; set; }
        public IOrderDetailMgr orderDetailMgr { get; set; }
        public IItemMgr itemMgr { get; set; }
        public IHuMgr huMgr { get; set; }
        public IUserMgr userMgr { get; set; }
        public IMesBomMgr mesBomMgr { get; set; }
        public IFlowMgr flowMgr { get; set; }
        public ICriteriaMgr criteriaMgr { get; set; }

        private static log4net.ILog log = log4net.LogManager.GetLogger("Log.MesDssOut");

        public void ProcessOut(ScmsTableIndex scmsTableIndex)
        {
            if(scmsTableIndex.TableName.Trim().ToUpper()==BusinessConstants.DSS_OUT_HU)
            {
                    this.ProcessHuOut(scmsTableIndex);
            }
            else if(scmsTableIndex.TableName.Trim().ToUpper()==BusinessConstants.DSS_OUT_ITEM)
            {
                    this.ProcessItemOut(scmsTableIndex);
            }
            else if(scmsTableIndex.TableName.Trim().ToUpper()==BusinessConstants.DSS_OUT_ORDERDETAIL)
            {
                    this.ProcessOrderDetailOut(scmsTableIndex);
            }
            else if(scmsTableIndex.TableName.Trim().ToUpper()==BusinessConstants.DSS_OUT_MESBOMDETAIL)
            {
                this.ProcessMesBomDetailOut(scmsTableIndex);
            }
        }

        [Transaction(TransactionMode.Requires)]
        private void ProcessHuOut(ScmsTableIndex scmsTableIndex)
        {
            IList<Hu> huList = GetTransferHu();
            if (huList != null && huList.Count > 0)
            {
                foreach (Hu hu in huList)
                {
                    try
                    {
                        ScmsMaterialBarcode materialBarcode = scmsMaterialBarcodeMgr.LoadScmsMaterialBarcode(hu.HuId);
                        if (materialBarcode == null)
                        {
                            materialBarcode = new ScmsMaterialBarcode();
                            materialBarcode.Flag = MesDssConstants.SCMS_MES_FLAG_SCMS_UPDATED;
                            materialBarcode.HuId = hu.HuId;
                            materialBarcode.ItemCode = hu.Item.Code;
                            materialBarcode.ItemDesc = hu.Item.Desc1;
                            materialBarcode.Qty = Convert.ToInt32(hu.Qty);
                            scmsMaterialBarcodeMgr.CreateScmsMaterialBarcode(materialBarcode);
                        }
                        hu.TransferFlag = false;
                        huMgr.UpdateHu(hu);
                    }
                    catch (Exception e)
                    {
                        log.Error(hu.HuId + " create exception", e);
                        continue;
                    }
                }
            }

            scmsTableIndexMgr.Complete(scmsTableIndex);
        }


        [Transaction(TransactionMode.Requires)]
        private void ProcessItemOut(ScmsTableIndex scmsTableIndex)
        {
            IList<Item> itemList = GetTransferItem();
            if (itemList != null && itemList.Count > 0)
            {
                foreach (Item item in itemList)
                {
                    try
                    {
                        ScmsPart scmsPart = scmsPartMgr.LoadScmsPart(item.Code);
                        if (scmsPart == null)
                        {
                            scmsPart = new ScmsPart();
                            scmsPart.Code = item.Code;
                            scmsPart.Des = item.Description;
                            scmsPart.Uom = item.Uom.Code;
                            scmsPart.LastModifyDate = item.LastModifyDate;
                            scmsPart.LastModifyUser = item.LastModifyUser.Code;
                            scmsPart.Flag = MesDssConstants.SCMS_MES_FLAG_SCMS_UPDATED;
                            scmsPartMgr.CreateScmsPart(scmsPart);
                        }
                        else
                        {
                            scmsPart.Des = item.Description;
                            scmsPart.Uom = item.Uom.Code;
                            scmsPart.LastModifyDate = item.LastModifyDate;
                            scmsPart.LastModifyUser = item.LastModifyUser.Code;
                            scmsPart.Flag = MesDssConstants.SCMS_MES_FLAG_SCMS_UPDATED;
                            scmsPartMgr.UpdateScmsPart(scmsPart);
                        }

                        item.TransferFlag = false;
                        itemMgr.UpdateItem(item);
                    }
                    catch (Exception e)
                    {
                        log.Error(item.Code + " create exception", e);
                        continue;
                    }
                }
            }

            scmsTableIndexMgr.Complete(scmsTableIndex);
        }


        [Transaction(TransactionMode.Requires)]
        private void ProcessOrderDetailOut(ScmsTableIndex scmsTableIndex)
        {
            IList<OrderDetail> orderDetailList = GetTransferOrderDetail();
            if (orderDetailList != null && orderDetailList.Count > 0)
            {
                foreach (OrderDetail orderDetail in orderDetailList)
                {
                    try
                    {
                        ScmsWorkOrder workOrder = scmsWorkOrderMgr.LoadScmsWorkOrder(orderDetail.OrderHead.OrderNo, orderDetail.Item.Code);
                         if (orderDetail.OrderHead.SubType != "Rwo")//����������������  djin 2012-4-28
                         {
                            if (workOrder == null)
                            {
                                workOrder = new ScmsWorkOrder();
                                workOrder.Bom = orderDetail.Bom.Code;
                                workOrder.Flag = MesDssConstants.SCMS_MES_FLAG_SCMS_UPDATED;
                                workOrder.IsAdditional = orderDetail.OrderHead.IsAdditional ? "1" : "0";
                                workOrder.ItemCode = orderDetail.Item.Code;
                                workOrder.LastModifyDate = orderDetail.OrderHead.LastModifyDate;
                                workOrder.LastModifyUser = orderDetail.OrderHead.LastModifyUser.Code;
                                workOrder.OrderNo = orderDetail.OrderHead.OrderNo;
                                workOrder.ProductLine = flowMgr.LoadFlow(orderDetail.OrderHead.Flow).Description;
                                workOrder.Qty = Convert.ToInt32(orderDetail.OrderedQty);
                                workOrder.RefItemCode = orderDetail.ReferenceItemCode;
                                workOrder.RefOrderNo = orderDetail.OrderHead.ReferenceOrderNo;
                                workOrder.Shift = orderDetail.OrderHead.Shift.ShiftName;
                                workOrder.StartTime = orderDetail.OrderHead.StartTime;
                                workOrder.UC = Convert.ToInt32(orderDetail.UnitCount);
                                //workOrder.Version = orderDetail.ItemVersion;
                                workOrder.WindowTime = orderDetail.OrderHead.WindowTime;
                                workOrder.ShiftCode = orderDetail.OrderHead.Shift.Code;
                                scmsWorkOrderMgr.CreateScmsWorkOrder(workOrder);
                            }
                            orderDetail.TransferFlag = false;
                            orderDetailMgr.UpdateOrderDetail(orderDetail);
                         }
                    }
                    catch (Exception e)
                    {
                        log.Error(orderDetail.OrderHead.OrderNo + "," + orderDetail.Item.Code + " create exception", e);
                        continue;
                    }
                }
            }

            scmsTableIndexMgr.Complete(scmsTableIndex);
        }



        [Transaction(TransactionMode.Requires)]
        private void ProcessMesBomDetailOut(ScmsTableIndex scmsTableIndex)
        {
            IList<MesBom> bomList = GetTransferBom();
            if (bomList != null && bomList.Count > 0)
            {
                foreach (MesBom mesBom in bomList)
                {
                    try
                    {
                        IList<MesBomDetail> scmsBomDetailList = mesBomDetailMgr.GetBomDetailList(mesBom);
                        if (scmsBomDetailList != null && scmsBomDetailList.Count > 0)
                        {
                            foreach (MesBomDetail mesBomDetail in scmsBomDetailList)
                            {
                                ScmsBom scmsBom = scmsBomMgr.LoadScmsBom(mesBomDetail.Bom.Code, mesBomDetail.Item.Code);
                                if (scmsBom == null)
                                {
                                    scmsBom = new ScmsBom();
                                    scmsBom.Bom = mesBomDetail.Bom.Code;
                                    scmsBom.Flag = mesBomDetail.IsActive ? MesDssConstants.SCMS_MES_FLAG_SCMS_UPDATED : MesDssConstants.SCMS_MES_FLAG_SCMS_DELETE;
                                    scmsBom.ItemCode = mesBomDetail.Item.Code;
                                    scmsBom.LastModifyDate = DateTime.Now;
                                    scmsBom.LastModifyUser = userMgr.GetMonitorUser().Code;
                                    scmsBom.Qty = mesBomDetail.RateQty;

                                    scmsBomMgr.CreateScmsBom(scmsBom);
                                }
                                else
                                {
                                    //DJIN 20120817
                                    scmsBom.Flag = mesBomDetail.IsActive||!mesBomDetail.EndDate.HasValue ? MesDssConstants.SCMS_MES_FLAG_SCMS_UPDATED : MesDssConstants.SCMS_MES_FLAG_SCMS_DELETE;
                                    scmsBom.LastModifyDate = DateTime.Now;
                                    scmsBom.LastModifyUser = userMgr.GetMonitorUser().Code;
                                    scmsBom.Qty = mesBomDetail.RateQty;
                                   
                                    scmsBomMgr.UpdateScmsBom(scmsBom);
                                }
                            }
                        }
                        mesBom.TransferFlag = false;

                        mesBomMgr.UpdateBom(mesBom);

                    }
                    catch (Exception e)
                    {
                        log.Error(mesBom.Code + " create exception", e);
                        continue;
                    }
                }
            }

            scmsTableIndexMgr.Complete(scmsTableIndex);
        }


        [Transaction(TransactionMode.Unspecified)]
        private IList<Hu> GetTransferHu()
        {
            DetachedCriteria criteria = DetachedCriteria.For(typeof(Hu));
            criteria.Add(Expression.Eq("TransferFlag", true));
            return criteriaMgr.FindAll<Hu>(criteria);
        }


        [Transaction(TransactionMode.Unspecified)]
        private IList<Item> GetTransferItem()
        {
            DetachedCriteria criteria = DetachedCriteria.For(typeof(Item));
            criteria.Add(Expression.Eq("TransferFlag", true));
            return criteriaMgr.FindAll<Item>(criteria);
        }

        [Transaction(TransactionMode.Unspecified)]
        private IList<OrderDetail> GetTransferOrderDetail()
        {
            DetachedCriteria criteria = DetachedCriteria.For(typeof(OrderDetail));
            criteria.Add(Expression.Eq("TransferFlag", true));
            
            return criteriaMgr.FindAll<OrderDetail>(criteria);
        }

        [Transaction(TransactionMode.Unspecified)]
        private IList<MesBom> GetTransferBom()
        {
            //�����ڻ�ȡqad��bom��ʱ��ֻ�������bom��tflag��Ǹ���
            DetachedCriteria criteria = DetachedCriteria.For(typeof(MesBom));
            criteria.Add(Expression.Eq("TransferFlag", true));
            IList<MesBom> mesBomList = criteriaMgr.FindAll<MesBom>(criteria);
            return mesBomList;
        }

    }
}