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

        public ResultInfo addBefore(K3DataParaInfo docInfo)
        {
            return defaultEventHandle(InterceptEvent.AddBefore, docInfo);
        }
        public ResultInfo addAfter(K3DataParaInfo docInfo)
        {
            return defaultEventHandle(InterceptEvent.AddAfter, docInfo);
        }
        public ResultInfo deleteBefore(K3DataParaInfo docInfo)
        {
            return defaultEventHandle(InterceptEvent.DeleteBefore, docInfo);
        }
        public ResultInfo deleteAfter(K3DataParaInfo docInfo)
        {
            return defaultEventHandle(InterceptEvent.DeleteAfter, docInfo);
        }
        public ResultInfo firstApprovedBefore(K3DataParaInfo docInfo)
        {
            return defaultEventHandle(InterceptEvent.FirstApprovedBefore, docInfo);
        }

        public ResultInfo firstApprovedAfter(K3DataParaInfo docInfo)
        {
            return defaultEventHandle(InterceptEvent.FirstApprovedAfter, docInfo);
        }

        public ResultInfo unFirstApprovedBefore(K3DataParaInfo docInfo)
        {
            return defaultEventHandle(InterceptEvent.UnFirstApprovedBefore, docInfo);
        }

        public ResultInfo unFirstApprovedAfter(K3DataParaInfo docInfo)
        {
            return defaultEventHandle(InterceptEvent.UnFirstApprovedAfter, docInfo);
        }
        public ResultInfo approvedBefore(K3DataParaInfo docInfo)
        {
            return defaultEventHandle(InterceptEvent.ApprovedBefore, docInfo);
        }

        public ResultInfo approvedAfter(K3DataParaInfo docInfo)
        {
            return defaultEventHandle(InterceptEvent.ApprovedAfter, docInfo);
        }

        public ResultInfo unApprovedBefore(K3DataParaInfo docInfo)
        {
            return defaultEventHandle(InterceptEvent.UnApprovedBefore, docInfo);
        }

        public ResultInfo unApprovedAfter(K3DataParaInfo docInfo)
        {
            return defaultEventHandle(InterceptEvent.UnApprovedAfter, docInfo);
        }

        public ResultInfo closedBefore(K3DataParaInfo docInfo)
        {
            return defaultEventHandle(InterceptEvent.ClosedBefore, docInfo);
        }

        public ResultInfo closedAfter(K3DataParaInfo docInfo)
        {
            return defaultEventHandle(InterceptEvent.ClosedAfter, docInfo);
        }

        public ResultInfo unClosedBefore(K3DataParaInfo docInfo)
        {
            return defaultEventHandle(InterceptEvent.UnClosedBefore, docInfo);
        }

        public ResultInfo unClosedAfter(K3DataParaInfo docInfo)
        {
            return defaultEventHandle(InterceptEvent.UnClosedAfter, docInfo);
        }

        public ResultInfo entryClosedBefore(K3DataParaInfo docInfo)
        {
            return defaultEventHandle(InterceptEvent.EntryClosedBefore, docInfo);
        }

        public ResultInfo entryClosedAfter(K3DataParaInfo docInfo)
        {
            return defaultEventHandle(InterceptEvent.EntryClosedAfter, docInfo);
        }

        public ResultInfo unEntryClosedBefore(K3DataParaInfo docInfo)
        {
            return defaultEventHandle(InterceptEvent.UnEntryClosedBefore, docInfo);
        }

        public ResultInfo unEntryClosedAfter(K3DataParaInfo docInfo)
        {
            return defaultEventHandle(InterceptEvent.UnEntryClosedAfter, docInfo);
        }

        public ResultInfo unKnownEvent(K3DataParaInfo docInfo)
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
        private ResultInfo defaultEventHandle(string eventName, K3DataParaInfo docInfo)
        {
            ResultInfo rltInfo = null;
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
                        return null;
                    }
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
                }
                else
                {
                    LogInfoHelp.debugLog(eventName, docInfo, string.Format("未启用X9系统对K3事件{0}的拦截", eventName));
                }
            }
            catch (Exception ex)
            {
                LogInfoHelp.infoLog(docInfo.EventName, docInfo,
                    string.Format("执行基类缺省拦截处理：{0}事件。异常：{1}", eventName, ex.Message));
                //throw new Exception(string.Format("{0}\t{1}", Environment.NewLine,
                //    string.Format("执行基类缺省拦截处理：{0}事件。异常：{1}", eventName, ex.Message)),ex);
                //throw new RankException(string.Format("执行基类缺省拦截处理：{0}事件。异常：{1}", eventName, string.Empty), ex);
            }
            return rltInfo;
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
        /// 对过滤字段的设定，可能是以枚举方式，这样造成保存到数据库后台的值是不可预定。所以后续可考虑加入ExcludedValue字段
        /// 保存“排除调用服务”的字段值，如N、false或其他值。
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

                using (SqlCommand sqlcommKeyExists = new SqlCommand(string.Format("select 1 from sys.columns where [object_id] = object_id('{0}') and name = '{1}'", itemConfig.ConditionTable, itemConfig.KeyField), sqlconn))
                {
                    Object objIsExists = sqlcommKeyExists.ExecuteScalar();
                    if (objIsExists == null || Convert.ToInt32(objIsExists.ToString()) != 1)
                    {
                        return bRlt;
                    }
                }

                string strExcludedValue = "N";
                if (!string.IsNullOrEmpty(itemConfig.ExcludedValue))
                {
                    strExcludedValue = itemConfig.ExcludedValue;
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
                        if (string.Equals(strExcludedValue, objValue.ToString(), StringComparison.OrdinalIgnoreCase))
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
