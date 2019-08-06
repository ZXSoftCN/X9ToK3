using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Windows.Forms;
using K3ToX9BillTransfer.UI;

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

        public abstract ResultInfo approvedBeforeExtend(K3DataParaInfo docInfo, K3InterceptConfig busiConfig);
        public abstract ResultInfo approvedAfterExtend(K3DataParaInfo docInfo, K3InterceptConfig busiConfig);

        public abstract ResultInfo unApprovedBeforeExtend(K3DataParaInfo docInfo, K3InterceptConfig busiConfig);
        public abstract ResultInfo unApprovedAfterExtend(K3DataParaInfo docInfo, K3InterceptConfig busiConfig);

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
                        bRlt = rltInfo.IsSuccess;//返回结果对象是否校验通过。
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
