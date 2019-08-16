using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using K3ToX9BillTransfer.UI;
using System.Windows.Forms;
using System.Data.SqlClient;
using System.Data;

namespace K3ToX9BillTransfer
{
    /// <summary>
    /// 根据K3中间层消息EventID等，判断调用哪种事件方法响应
    /// </summary>
    public class InterceptHandler
    {
        static string strConfig = AppDomain.CurrentDomain.BaseDirectory + "config.xml";

        /// <summary>
        /// </summary>
        /// <param name="k3Connection"></param>
        /// <param name="transType"></param>
        /// <param name="rob"></param>
        /// <param name="operateID"></param>
        /// <param name="eventID"></param>
        /// <param name="interID"></param>
        /// <param name="billCode"></param>
        /// <param name="currUser"></param>
        /// <param name="data"></param>
        /// <param name="rltFlag">特别注意为false时，也就是执行失败时外部分布式事务会回滚所有的数据更新处理。</param>
        public void handle(string k3Connection, int transType, int rob,long operateID, long eventID, long interID,int entryID, string billCode,string currUser, string data, ref bool rltFlag)
        {
            bool bInnerRlt = true;
            //LogInfoHelp.Log(k3Connection, LOG_TYPE.LOG_DEBUG);

            int lCheckLevel = 0, lCurrentLevel = 0;
            try
            {
                lCheckLevel = checkMultiLevel(k3Connection, transType);
                if (lCheckLevel > 0)
                {
                    LogInfoHelp.logOnlyDebug(string.Format("业务类型【{0}】,单据内码【{1}】进入多级({2})审核判断拦截", transType.ToString(), interID.ToString(), lCheckLevel.ToString()));
                    lCurrentLevel = getCurrentLevel(k3Connection, interID, transType);
                }
            }
            catch (Exception ex)
            {
                LogInfoHelp.Log(string.Format("查询单据多级审核时异常：{0}", ex.Message), LOG_TYPE.LOG_INFO);
            }
            
            K3DataParaInfo docInfo = new K3DataParaInfo()
            {
                BillCode = billCode,
                InterID = interID,
                EntryID = entryID,
                TransType = transType,
                ROB = rob,
                CurrentUser = currUser,
                X9BillType = CommonFunc.ContrastK3TransType(Convert.ToInt64(transType),rob),
                EventName = InterceptEvent.ConvertToEventName(eventID, operateID, lCheckLevel,lCurrentLevel),
                Data = data,
            };
            LogInfoHelp.debugLog(docInfo.EventName, docInfo, "进入事件分发响应处理");
            try
            {
                if (ServiceConfig.Initial(k3Connection))
                {
                    LogInfoHelp.debugLog(docInfo.EventName,
                        docInfo, string.Format("X9对K3中间层拦截器初始化完成，进入事件{0}响应。", docInfo.EventName));
                    IK3Intercept processor = new X9BusinessIntercept();//以标准类实现
                    ///     200001 '审核前事件
                    ///     200002 '审核反写事件
                    ///     200003 '审核退出前事件
                    switch (docInfo.EventName)
                    {
                        case InterceptEvent.AddBefore:
                            bInnerRlt = processor.addBefore(docInfo);
                            break;
                        case InterceptEvent.AddAfter:
                            bInnerRlt = processor.addAfter(docInfo);
                            break;
                        case InterceptEvent.DeleteBefore:
                            bInnerRlt = processor.deleteBefore(docInfo);
                            break;
                        case InterceptEvent.DeleteAfter:
                            bInnerRlt = processor.deleteAfter(docInfo);
                            break;
                        case InterceptEvent.FirstApprovedBefore:
                            bInnerRlt = processor.firstApprovedBefore(docInfo);
                            break;
                        case InterceptEvent.FirstApprovedAfter:
                            bInnerRlt = processor.firstApprovedAfter(docInfo);
                            break;
                        case InterceptEvent.UnFirstApprovedBefore:
                            bInnerRlt = processor.unFirstApprovedBefore(docInfo);
                            break;
                        case InterceptEvent.UnFirstApprovedAfter:
                            bInnerRlt = processor.unFirstApprovedAfter(docInfo);
                            break;
                        case InterceptEvent.ApprovedBefore:
                            bInnerRlt = processor.approvedBefore(docInfo);
                            break;
                        case InterceptEvent.ApprovedAfter:
                            bInnerRlt = processor.approvedAfter(docInfo);
                            break;
                        case InterceptEvent.UnApprovedBefore:
                            bInnerRlt = processor.unApprovedBefore(docInfo);
                            break;
                        case InterceptEvent.UnApprovedAfter:
                            bInnerRlt = processor.unApprovedAfter(docInfo);
                            break;
                        case InterceptEvent.ClosedBefore:
                            bInnerRlt = processor.closedBefore(docInfo);
                            break;
                        case InterceptEvent.ClosedAfter:
                            bInnerRlt = processor.closedAfter(docInfo);
                            break;
                        case InterceptEvent.UnClosedBefore:
                            bInnerRlt = processor.unClosedBefore(docInfo);
                            break;
                        case InterceptEvent.UnClosedAfter:
                            bInnerRlt = processor.unClosedAfter(docInfo);
                            break;
                        case InterceptEvent.EntryClosedBefore:
                            bInnerRlt = processor.entryClosedBefore(docInfo);
                            break;
                        case InterceptEvent.EntryClosedAfter:
                            bInnerRlt = processor.entryClosedAfter(docInfo);
                            break;
                        case InterceptEvent.UnEntryClosedBefore:
                            bInnerRlt = processor.unEntryClosedBefore(docInfo);
                            break;
                        case InterceptEvent.UnEntryClosedAfter:
                            bInnerRlt = processor.unEntryClosedAfter(docInfo);
                            break;
                        default:
                            bInnerRlt = processor.unKnownEvent(docInfo);
                            break;
                    }
                }
                else
                {
                    LogInfoHelp.infoLog(docInfo.EventName, docInfo, "X9对K3中间层拦截器初始化失败。请进行Debug查阅！");
                    LogInfoHelp.debugLog(docInfo.EventName, docInfo,
                        string.Format("X9对K3中间层拦截器初始化失败。{0}传入的K3数据库连接串：{1}{2}当前环境执行路径：{3}{4}",
                        Environment.NewLine,k3Connection,Environment.NewLine,CommonFunc.strPath,Environment.NewLine));
                    if (CommonFunc.ConfigLogType > LOG_TYPE.LOG_INFO)
                    {
                        frmMessageSingle.Show("X9对K3中间层拦截器初始化失败!", 
                            string.Format("{0}传入的K3数据库连接串：{1}{2}当前环境执行路径：{3}{4}",
                        Environment.NewLine, k3Connection, Environment.NewLine, CommonFunc.strPath, Environment.NewLine));
                    }
                }
            }
            catch (Exception ex)
            {
                //异常时将错误信息保存至日志，并弹窗提示，但不中断K3动作，让其继续执行下去。
                //rltFlag = false;
                bInnerRlt = false;
                LogInfoHelp.infoLog(docInfo.EventName, docInfo,
                    string.Format("X9对K3中间层拦截器执行异常，可将配置文件设置为‘Debug'进行详查定位！{0}异常消息：{1}\t{2}异常对象来源：{3}\t{4}异常堆栈：{5}",
                     Environment.NewLine, ex.Message, Environment.NewLine, ex.Source, Environment.NewLine, ex.StackTrace));
                //LogInfoHelp.debugLog(docInfo.EventName, docInfo,
                //    string.Format("X9对K3中间层拦截器执行异常。异常消息：{0}{1}异常对象来源：{2}{3}异常堆栈：{4}{5}{6}",
                //    Environment.NewLine, ex.Message, Environment.NewLine, ex.Source,
                //    Environment.NewLine, ex.StackTrace, Environment.NewLine));

                //if (CommonFunc.ConfigLogType > LOG_TYPE.LOG_INFO)
                //{
                    frmMessageSingle.Show("X9对K3中间层拦截器执行异常!",
                        string.Format("X9对K3中间层拦截器执行异常，可将配置文件设置为‘Debug'进行详查定位！{0}异常消息：{1}\t{2}异常对象来源：{3}\t{4}异常堆栈：{5}",
                     Environment.NewLine, ex.Message, Environment.NewLine, ex.Source, Environment.NewLine, ex.StackTrace));
                //}
            }
            finally
            {
                //rltFlag = bInnerRlt;//最终将内部调用结果返回给K3中间件插件 //不再中断K3动作 2019-8-13
                LogInfoHelp.infoLog(docInfo.EventName, docInfo, string.Format("K3ToX9拦截器执行结束,结果：{0}", bInnerRlt.ToString()));
            }
        }

        //Select FCheckCtlLevel, * From t_MultiCheckOption Where FBillType in (10,29) and FOptionValue = 20
        //获取单据多级审核时的终审级别
        internal int checkMultiLevel(string k3ConnectString, int tranTypeID)
        {
            using (SqlConnection sqlconn = new SqlConnection(k3ConnectString))
            {
                sqlconn.Open();
                using (SqlCommand sqlcommCheckLevel = new SqlCommand(string.Format("Select isnull(FCheckCtlLevel,0) From t_MultiCheckOption where FOptionValue > 4 and FBillType = '{0}'",
                    tranTypeID.ToString()), sqlconn))
                {
                    Object objIsExists = sqlcommCheckLevel.ExecuteScalar();

                    if (objIsExists != null && Convert.ToInt32(objIsExists.ToString()) > 0)
                    {
                        return Convert.ToInt32(objIsExists.ToString());
                    }
                }
            }
            return 0;
        }

        //select FStatus,FMultiCheckLevel1,FCurCheckLevel,* from icstockbill where FInterID = 2100
        //得到单据当前操作时的审核级别（只限于出入库单据）
        internal int getCurrentLevel(string k3ConnectString, long interID, int tranTypeID)
        {
            using (SqlConnection sqlconn = new SqlConnection(k3ConnectString))
            {
                sqlconn.Open();
                bool isStock = false;
                using (SqlCommand sqlcommIsStock = new SqlCommand(string.Format("select isnull(FHeadTable,'') from ictranstype where FID = {0}",
                    tranTypeID.ToString()), sqlconn))
                {
                    Object objStockTable = sqlcommIsStock.ExecuteScalar();

                    if (objStockTable != null && !String.IsNullOrEmpty(objStockTable.ToString()))
                    {
                        if (string.Equals(objStockTable.ToString(),"IcStockBill",StringComparison.OrdinalIgnoreCase))
                        {
                            isStock = true;
                        }
                    }
                }

                if (!isStock)
                {
                    return 0;
                }
                using (SqlCommand sqlcommLevel = new SqlCommand(string.Format("select FCurCheckLevel from icstockbill where FInterID = {0}",
                    interID.ToString()), sqlconn))
                {
                    Object objCurrLevel = sqlcommLevel.ExecuteScalar();

                    if (objCurrLevel != null && !String.IsNullOrEmpty(objCurrLevel.ToString()))
                    {
                        return Convert.ToInt32(objCurrLevel.ToString());
                    }
                }
            }
            return 0;
        }
    }
}
