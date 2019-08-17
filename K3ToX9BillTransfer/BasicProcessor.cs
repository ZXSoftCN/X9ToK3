using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Windows.Forms;
using K3ToX9BillTransfer.UI;
using System.Data.SqlClient;

namespace K3ToX9BillTransfer
{
    /// <summary>
    /// 实现K3与X9系统交互接口的基础类。
    /// 实现了debug的日志记录。
    /// </summary>
    public abstract class BasicProcessor : IK3Intercept
    {
        private StringBuilder strbResult = new StringBuilder();
        
        public K3DataParaInfo innerDocInfo { get; set; }

        #region IInterceptEventProcessor方法实现

        public bool addBefore(K3DataParaInfo docInfo)
        {
            return defaultEventHandle(InterceptEvent.AddBefore, docInfo);
        }
        public bool addAfter(K3DataParaInfo docInfo)
        {
            return defaultEventHandle(InterceptEvent.AddAfter, docInfo);
        }
        public bool deleteBefore(K3DataParaInfo docInfo)
        {
            return defaultEventHandle(InterceptEvent.DeleteBefore, docInfo);
        }
        public bool deleteAfter(K3DataParaInfo docInfo)
        {
            return defaultEventHandle(InterceptEvent.DeleteAfter, docInfo);
        }
        public bool firstApprovedBefore(K3DataParaInfo docInfo)
        {
            return defaultEventHandle(InterceptEvent.FirstApprovedBefore, docInfo);
        }

        public bool firstApprovedAfter(K3DataParaInfo docInfo)
        {
            return defaultEventHandle(InterceptEvent.FirstApprovedAfter, docInfo);
        }

        public bool unFirstApprovedBefore(K3DataParaInfo docInfo)
        {
            return defaultEventHandle(InterceptEvent.UnFirstApprovedBefore, docInfo);
        }

        public bool unFirstApprovedAfter(K3DataParaInfo docInfo)
        {
            return defaultEventHandle(InterceptEvent.UnFirstApprovedAfter, docInfo);
        }
        public bool approvedBefore(K3DataParaInfo docInfo)
        {
            return defaultEventHandle(InterceptEvent.ApprovedBefore, docInfo);
        }

        public bool approvedAfter(K3DataParaInfo docInfo)
        {
            return defaultEventHandle(InterceptEvent.ApprovedAfter, docInfo);
        }

        public bool unApprovedBefore(K3DataParaInfo docInfo)
        {
            return defaultEventHandle(InterceptEvent.UnApprovedBefore, docInfo);
        }

        public bool unApprovedAfter(K3DataParaInfo docInfo)
        {
            return defaultEventHandle(InterceptEvent.UnApprovedAfter, docInfo);
        }

        public bool closedBefore(K3DataParaInfo docInfo)
        {
            return defaultEventHandle(InterceptEvent.ClosedBefore, docInfo);
        }

        public bool closedAfter(K3DataParaInfo docInfo)
        {
            return defaultEventHandle(InterceptEvent.ClosedAfter, docInfo);
        }

        public bool unClosedBefore(K3DataParaInfo docInfo)
        {
            return defaultEventHandle(InterceptEvent.UnClosedBefore, docInfo);
        }

        public bool unClosedAfter(K3DataParaInfo docInfo)
        {
            return defaultEventHandle(InterceptEvent.UnClosedAfter, docInfo);
        }

        public bool entryClosedBefore(K3DataParaInfo docInfo)
        {
            return defaultEventHandle(InterceptEvent.EntryClosedBefore, docInfo);
        }

        public bool entryClosedAfter(K3DataParaInfo docInfo)
        {
            return defaultEventHandle(InterceptEvent.EntryClosedAfter, docInfo);
        }

        public bool unEntryClosedBefore(K3DataParaInfo docInfo)
        {
            return defaultEventHandle(InterceptEvent.UnEntryClosedBefore, docInfo);
        }

        public bool unEntryClosedAfter(K3DataParaInfo docInfo)
        {
            return defaultEventHandle(InterceptEvent.UnEntryClosedAfter, docInfo);
        }

        public bool unKnownEvent(K3DataParaInfo docInfo)
        {
            return defaultEventHandle(InterceptEvent.UnKnownEvent, docInfo);
        }        
        #endregion

        #region 下级实现类扩展方法
        public abstract ResultInfo addBeforeExtend(K3DataParaInfo docInfo, K3InterceptConfig busiConfig);
        public abstract ResultInfo addAfterExtend(K3DataParaInfo docInfo, K3InterceptConfig busiConfig);
        public abstract ResultInfo deleteBeforeExtend(K3DataParaInfo docInfo, K3InterceptConfig busiConfig);
        public abstract ResultInfo deleteAfterExtend(K3DataParaInfo docInfo, K3InterceptConfig busiConfig);

        public abstract ResultInfo firstApprovedBeforeExtend(K3DataParaInfo docInfo, K3InterceptConfig busiConfig);
        public abstract ResultInfo firstApprovedAfterExtend(K3DataParaInfo docInfo, K3InterceptConfig busiConfig);
        public abstract ResultInfo unFirstApprovedBeforeExtend(K3DataParaInfo docInfo, K3InterceptConfig busiConfig);
        public abstract ResultInfo unFirstApprovedAfterExtend(K3DataParaInfo docInfo, K3InterceptConfig busiConfig);
        public abstract ResultInfo approvedBeforeExtend(K3DataParaInfo docInfo, K3InterceptConfig busiConfig);
        public abstract ResultInfo approvedAfterExtend(K3DataParaInfo docInfo, K3InterceptConfig busiConfig);
        public abstract ResultInfo unApprovedBeforeExtend(K3DataParaInfo docInfo, K3InterceptConfig busiConfig);
        public abstract ResultInfo unApprovedAfterExtend(K3DataParaInfo docInfo, K3InterceptConfig busiConfig);

        public abstract ResultInfo closedBeforeExtend(K3DataParaInfo docInfo, K3InterceptConfig busiConfig);
        public abstract ResultInfo closedAfterExtend(K3DataParaInfo docInfo, K3InterceptConfig busiConfig);
        public abstract ResultInfo unClosedBeforeExtend(K3DataParaInfo docInfo, K3InterceptConfig busiConfig);
        public abstract ResultInfo unClosedAfterExtend(K3DataParaInfo docInfo, K3InterceptConfig busiConfig);
        public abstract ResultInfo entryClosedBeforeExtend(K3DataParaInfo docInfo, K3InterceptConfig busiConfig);
        public abstract ResultInfo entryClosedAfterExtend(K3DataParaInfo docInfo, K3InterceptConfig busiConfig);
        public abstract ResultInfo unEntryClosedBeforeExtend(K3DataParaInfo docInfo, K3InterceptConfig busiConfig);
        public abstract ResultInfo unEntryClosedAfterExtend(K3DataParaInfo docInfo, K3InterceptConfig busiConfig);
        
        public abstract ResultInfo unKnownExtend(K3DataParaInfo docInfo, K3InterceptConfig busiConfig);
        #endregion

        /// <summary>
        /// 基类缺省处理事件方法
        /// </summary>
        /// <param name="eventName"></param>
        /// <param name="docInfo"></param>
        /// <param name="rltFlag"></param>
        /// <returns><\returns>
        private bool defaultEventHandle(string eventName, K3DataParaInfo docInfo)
        {
            bool bRlt = true;
            try
            {
                LogInfoHelp.debugLog(eventName, docInfo, string.Format("进入基类{0}事件响应",eventName));

                K3InterceptConfig itemConfig = validateBusinessEnable(docInfo, eventName);
                
                if (itemConfig != null)
                {
                    LogInfoHelp.debugLog(eventName, docInfo, string.Format("进入X9系统业务校验事件{0}服务中", eventName));
                    if (!isCalledFilter(itemConfig, docInfo))
                    {
                        LogInfoHelp.infoLog(eventName, docInfo, string.Format("X9系统业务校验事件{0}服务，单据【{1}]表头标记为“不进入X9系统”。", eventName,docInfo.InterID.ToString()));
                        return false;
                    }
                    ResultInfo rltInfo = null;
                    switch (eventName)
                    {
                        case InterceptEvent.AddBefore:
                            rltInfo = addBeforeExtend(docInfo, itemConfig);
                            break;
                        case InterceptEvent.AddAfter:
                            rltInfo = addAfterExtend(docInfo, itemConfig);
                            break;
                        case InterceptEvent.DeleteBefore:
                            rltInfo = deleteBeforeExtend(docInfo, itemConfig);
                            break;
                        case InterceptEvent.DeleteAfter:
                            rltInfo = deleteAfterExtend(docInfo, itemConfig);
                            break;
                        case InterceptEvent.FirstApprovedBefore:
                            rltInfo = firstApprovedBeforeExtend(docInfo, itemConfig);
                            break;
                        case InterceptEvent.FirstApprovedAfter:
                            rltInfo = firstApprovedAfterExtend(docInfo, itemConfig);
                            break;
                        case InterceptEvent.UnFirstApprovedBefore:
                            rltInfo = unFirstApprovedBeforeExtend(docInfo, itemConfig);
                            break;
                        case InterceptEvent.UnFirstApprovedAfter:
                            rltInfo = unFirstApprovedAfterExtend(docInfo, itemConfig);
                            break;
                        case InterceptEvent.ApprovedBefore:
                            rltInfo = approvedBeforeExtend(docInfo, itemConfig);
                            break;
                        case InterceptEvent.ApprovedAfter:
                            rltInfo = approvedAfterExtend(docInfo, itemConfig);
                            break;
                        case InterceptEvent.UnApprovedBefore:
                            rltInfo = unApprovedBeforeExtend(docInfo, itemConfig);
                            break;
                        case InterceptEvent.UnApprovedAfter:
                            rltInfo = unApprovedAfterExtend(docInfo, itemConfig);
                            break;
                        case InterceptEvent.ClosedBefore:
                            rltInfo = closedBeforeExtend(docInfo, itemConfig);
                            break;
                        case InterceptEvent.ClosedAfter:
                            rltInfo = closedAfterExtend(docInfo, itemConfig);
                            break;
                        case InterceptEvent.UnClosedBefore:
                            rltInfo = unClosedBeforeExtend(docInfo, itemConfig);
                            break;
                        case InterceptEvent.UnClosedAfter:
                            rltInfo = unClosedAfterExtend(docInfo, itemConfig);
                            break;
                        case InterceptEvent.EntryClosedBefore:
                            rltInfo = entryClosedBeforeExtend(docInfo, itemConfig);
                            break;
                        case InterceptEvent.EntryClosedAfter:
                            rltInfo = entryClosedAfterExtend(docInfo, itemConfig);
                            break;
                        case InterceptEvent.UnEntryClosedBefore:
                            rltInfo = unEntryClosedBeforeExtend(docInfo, itemConfig);
                            break;
                        case InterceptEvent.UnEntryClosedAfter:
                            rltInfo = unEntryClosedAfterExtend(docInfo, itemConfig);
                            break;
                        default:
                            rltInfo = unKnownExtend(docInfo, itemConfig);
                            break;
                    }
                    
                    if (rltInfo == null)
                    {
                        //LogInfoHelp.infoLog(eventName, docInfo, string.Format("X9系统业务校验事件{0}服务，返回结果为空值(null),K3动作继续进行。", eventName));
                        throw new Exception(string.Format("X9系统业务校验事件{0}服务，返回结果为空值(null),K3动作继续进行。", eventName));
                    }
                    else
                    {
                        bRlt = rltInfo.IsSuccess;//(2019-8-17取消)返回结果对象是否校验通过。2019-8-13 改为：不管X9服务认定是否通过，都不再中断K3动作。

                        LogInfoHelp.infoLog(eventName, docInfo, string.Format("X9系统业务校验事件{0}服务，返回结果为{1}。", eventName,rltInfo.IsSuccess.ToString()));
                        LogInfoHelp.debugLog(eventName, docInfo, string.Format("X9系统业务校验事件{0}服务，返回结果为{1}。", eventName, 
                            XmlSerializerHelper.XmlSerialize<ResultInfo>(rltInfo,Encoding.Unicode)));
                        //当标记为Debug时，显示结果返回窗。
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
                            if (bRlt)
                            {
                                strEventMsg = string.Format("X9系统检查{0}通过！", InterceptEvent.ConvertToCNZHName(eventName));
                            }
                            else
                            {
                                strEventMsg = string.Format("X9系统检查{0}不通过！", InterceptEvent.ConvertToCNZHName(eventName));
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
                    }
                    LogInfoHelp.debugLog(eventName, docInfo, string.Format("完成X9系统业务校验事件{0}服务中", eventName));
                }
                else
                {
                    LogInfoHelp.debugLog(eventName, docInfo, string.Format("未启用X9系统对K3事件{0}的拦截", eventName));
                }
            }
            catch (Exception ex)
            {
                //LogInfoHelp.infoLog(InterceptEvent.ApprovedBefore, docInfo,
                //    string.Format("执行基类缺省拦截处理：{0}事件。异常：{1}",eventName, ex.Message));
                throw new Exception(string.Format("{0}\t{1}", Environment.NewLine,
                    string.Format("执行基类缺省拦截处理：{0}事件。异常：{1}", eventName, ex.Message)),ex);
                //throw new RankException(string.Format("执行基类缺省拦截处理：{0}事件。异常：{1}", eventName, string.Empty), ex);
            }
            return bRlt;
        }

        /// <summary>
        /// 检测基础业务是否可启用
        /// </summary>
        /// <param name="transType"></param>
        /// <param name="rob"></param>
        /// <param name="eventID"></param>
        /// <returns><\returns>
        internal K3InterceptConfig validateBusinessEnable(K3DataParaInfo docInfo, string interceptEvent)
        {
            List<K3InterceptConfig> lstConfig = (from s in ServiceConfig.Instance.BusiConfigs
                                                 where s.InterceptEvent == interceptEvent && s.X9BusinessType == docInfo.X9BillType && s.IsEnable == 1
                orderby s.Id descending
                select s).ToList<K3InterceptConfig>();

            foreach (var item in lstConfig)
            {
                if (DateTime.Now.Date >= item.EnableDate && DateTime.Now.Date <= item.DisableDate)
                {
                    return item;
                }
            }
            return null; ;
        }

        /// <summary>
        /// 所单据表头增加了“是否进X9”字段，并在zz_t_K3InterceptConfig配置表设置好对应字段则会进行过滤调用。
        /// </summary>
        /// <param name="itemConfig"></param>
        /// <param name="docInfo"></param>
        /// <returns></returns>
        internal bool isCalledFilter(K3InterceptConfig itemConfig,K3DataParaInfo docInfo)
        {
            bool bRlt = true;
            if (itemConfig == null || string.IsNullOrEmpty(itemConfig.ConditionTable) || string.IsNullOrEmpty(itemConfig.ConditionField) || string.IsNullOrEmpty(itemConfig.KeyField))
            {
                return bRlt;
            }
            using (SqlConnection sqlconn = new SqlConnection(ServiceConfig.Instance.K3ConnectString))
            {
                sqlconn.Open();
                using (SqlCommand sqlcommExists = new SqlCommand(string.Format("select 1 from sys.columns where [object_id] = object_id('{0}') and name = '{1}'", itemConfig.ConditionTable, itemConfig.ConditionField), sqlconn))
                {
                    Object objIsExists = sqlcommExists.ExecuteScalar();
                    if (objIsExists == null || Convert.ToInt32(objIsExists.ToString()) != 1)
                    {
                        return bRlt;
                    }
                }

                using (SqlCommand sqlcommKeyExists = new SqlCommand(string.Format("select 1 from sys.columns where [object_id] = object_id('{0}') and name = '{1}'", itemConfig.ConditionTable, itemConfig.ConditionField), sqlconn))
                {
                    Object objIsExists = sqlcommKeyExists.ExecuteScalar();
                    if (objIsExists == null || Convert.ToInt32(objIsExists.ToString()) != 1)
                    {
                        return bRlt;
                    }
                }

                using (SqlCommand sqlcommCondition = new SqlCommand(string.Format("select isnull({1},'Y') from {0} where {2} = {3}", itemConfig.ConditionTable, itemConfig.ConditionField,itemConfig.KeyField,docInfo.InterID.ToString()), sqlconn))
                {
                    Object objValue = sqlcommCondition.ExecuteScalar();
                    if (objValue == null)
                    {
                        return bRlt;
                    }
                    else
                    {
                        if (string.Equals("N",objValue.ToString(),StringComparison.OrdinalIgnoreCase) 
                            || string.Equals("否",objValue.ToString(),StringComparison.OrdinalIgnoreCase)
                            || string.Equals("false", objValue.ToString(), StringComparison.OrdinalIgnoreCase))
                        {
                            bRlt = false;
                        }
                    }
                }
            }

            return bRlt;
        }

        
        public LOG_TYPE logInfoType
        {
            get
            {
                LOG_TYPE configLog = LOG_TYPE.LOG_INFO;
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.Load(CommonFunc.strConfig);

                XmlNodeList lstNode = xmlDoc.GetElementsByTagName("LogInfoType");
                if (lstNode.Count > 0)
                {
                    XmlNode xNodeItem = xmlDoc.GetElementsByTagName("LogInfoType")[0];
                    if (xNodeItem.Attributes.Count > 0)
                    {
                        try
                        {
                            configLog = (LOG_TYPE)Enum.Parse(typeof(LOG_TYPE), xNodeItem.Attributes[0].Value);
                        }
                        catch
                        {
                            //配置文件提供的日志类型错误时，吃掉转换异常保持初始值LOG_TYPE.LOG_INFO。
                        }
                    }
                }
                return configLog;
            }
        }

    }
}
