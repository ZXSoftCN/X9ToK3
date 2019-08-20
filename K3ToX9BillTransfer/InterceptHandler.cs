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
        public void handle(string k3Connection, int transType, int rob,long operateID, long eventID, long interID,int entryID, string billCode,string currUser, string data, ref bool rltFlag,ref string rltMsg)
        {
            ResultInfo rltInfo = null;
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
                            rltInfo = processor.addBefore(docInfo);
                            break;
                        case InterceptEvent.AddAfter:
                            rltInfo = processor.addAfter(docInfo);
                            break;
                        case InterceptEvent.DeleteBefore:
                            rltInfo = processor.deleteBefore(docInfo);
                            break;
                        case InterceptEvent.DeleteAfter:
                            rltInfo = processor.deleteAfter(docInfo);
                            break;
                        case InterceptEvent.FirstApprovedBefore:
                            rltInfo = processor.firstApprovedBefore(docInfo);
                            break;
                        case InterceptEvent.FirstApprovedAfter:
                            rltInfo = processor.firstApprovedAfter(docInfo);
                            break;
                        case InterceptEvent.UnFirstApprovedBefore:
                            rltInfo = processor.unFirstApprovedBefore(docInfo);
                            break;
                        case InterceptEvent.UnFirstApprovedAfter:
                            rltInfo = processor.unFirstApprovedAfter(docInfo);
                            break;
                        case InterceptEvent.ApprovedBefore:
                            rltInfo = processor.approvedBefore(docInfo);
                            break;
                        case InterceptEvent.ApprovedAfter:
                            rltInfo = processor.approvedAfter(docInfo);
                            break;
                        case InterceptEvent.UnApprovedBefore:
                            rltInfo = processor.unApprovedBefore(docInfo);
                            break;
                        case InterceptEvent.UnApprovedAfter:
                            rltInfo = processor.unApprovedAfter(docInfo);
                            break;
                        case InterceptEvent.ClosedBefore:
                            rltInfo = processor.closedBefore(docInfo);
                            break;
                        case InterceptEvent.ClosedAfter:
                            rltInfo = processor.closedAfter(docInfo);
                            break;
                        case InterceptEvent.UnClosedBefore:
                            rltInfo = processor.unClosedBefore(docInfo);
                            break;
                        case InterceptEvent.UnClosedAfter:
                            rltInfo = processor.unClosedAfter(docInfo);
                            break;
                        case InterceptEvent.EntryClosedBefore:
                            rltInfo = processor.entryClosedBefore(docInfo);
                            break;
                        case InterceptEvent.EntryClosedAfter:
                            rltInfo = processor.entryClosedAfter(docInfo);
                            break;
                        case InterceptEvent.UnEntryClosedBefore:
                            rltInfo = processor.unEntryClosedBefore(docInfo);
                            break;
                        case InterceptEvent.UnEntryClosedAfter:
                            rltInfo = processor.unEntryClosedAfter(docInfo);
                            break;
                        default:
                            rltInfo = processor.unKnownEvent(docInfo);
                            break;
                    }
                    if (rltInfo == null)
                    {
                        //当未启用、单据表头标记为‘不进入X9系统’或服务调用异常时，rltInfo为null,这时K3业务继续。
                        LogInfoHelp.debugLog(docInfo.EventName, docInfo, string.Format("X9系统业务校验事件{0}服务，返回结果为空值(null),K3动作继续进行。", docInfo.EventName));
                        rltFlag = true;
                        //throw new Exception(string.Format("X9系统业务校验事件{0}服务，返回结果为空值(null),K3动作继续进行。", docInfo.EventName));
                    }
                    else
                    {
                        if (docInfo.EventName.IndexOf("After", 0, StringComparison.OrdinalIgnoreCase) > 0)
                        {
                            rltFlag = true;
                        }
                        else
                        {
                            rltFlag = rltInfo.IsSuccess;//(2019-8-17取消)返回结果对象是否校验通过。2019-8-13 改为：不管X9服务认定是否通过，都不再中断K3动作。
                        }
                        if (!rltFlag)
                        {
                            //X9服务返回false时，将异常消息传出
                            StringBuilder strbError = new StringBuilder();
                            foreach (var item in rltInfo.Errors)
                            {
                                if (!String.IsNullOrEmpty(item.ErrorText))
                                {
                                    strbError.AppendLine(item.ErrorText);
                                }
                            }
                            rltMsg = strbError.ToString();
                        }

                        LogInfoHelp.infoLog(docInfo.EventName, docInfo, string.Format("X9系统业务校验事件{0}服务，返回结果为{1}。", docInfo.EventName, rltInfo.IsSuccess.ToString()));
                        LogInfoHelp.debugLog(docInfo.EventName, docInfo, string.Format("X9系统业务校验事件{0}服务，返回结果为{1}。", docInfo.EventName,
                            XmlSerializerHelper.XmlSerialize<ResultInfo>(rltInfo, Encoding.Unicode)));
                        #region 当标记为Debug时，显示结果返回窗。
                        if (CommonFunc.ConfigLogType > LOG_TYPE.LOG_INFO)
                        {
                            StringBuilder strbInfo = new StringBuilder();
                            StringBuilder strbError = new StringBuilder();
                            foreach (var item in rltInfo.Errors)
                            {
                                if (!String.IsNullOrEmpty(item.ErrorText))
                                {
                                    strbError.AppendLine(item.ErrorText);
                                }
                            }
                            foreach (var item in rltInfo.Results)
                            {
                                if (!String.IsNullOrEmpty(item.MsgText))
                                {
                                    strbInfo.AppendLine(item.MsgText);
                                }
                            }
                            string strEventMsg = string.Empty;
                            if (rltFlag)
                            {
                                strEventMsg = string.Format("X9系统检查{0}通过！", InterceptEvent.ConvertToCNZHName(docInfo.EventName));
                            }
                            else
                            {
                                strEventMsg = string.Format("X9系统检查{0}不通过！", InterceptEvent.ConvertToCNZHName(docInfo.EventName));
                            }
                            string strRlt = string.Format("消息：{0}{1}", strEventMsg, Environment.NewLine);
                            if (!string.IsNullOrEmpty(strbInfo.ToString()))
                            {
                                strRlt += string.Format("{0}{1}", strbInfo.ToString(), Environment.NewLine);
                            }
                            if (!string.IsNullOrEmpty(strbError.ToString()))
                            {
                                strRlt += string.Format("异常提示：{0}", strbError.ToString());
                            }
                            frmMessageSingle.Show(strRlt, XmlSerializerHelper.XmlSerialize<ResultInfo>(rltInfo, Encoding.Unicode));
                        }
                        #endregion
                    }
                    LogInfoHelp.debugLog(docInfo.EventName, docInfo, string.Format("完成X9系统业务校验事件{0}服务中", docInfo.EventName));
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
                //(2019-8-17取消)异常时将错误信息保存至日志，并弹窗提示，但不中断K3动作，让其继续执行下去。
                //rltFlag = false;
                rltFlag = false;
                rltMsg = string.Format("请查看日志进行详查定位！异常消息：{0}",ex.Message);
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
                //rltFlag = rltInfo;//(2019-8-17取消)最终将内部调用结果返回给K3中间件插件 //不再中断K3动作 2019-8-13
                LogInfoHelp.infoLog(docInfo.EventName, docInfo, string.Format("K3ToX9拦截器执行结束,结果：{0}", rltInfo.ToString()));
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
