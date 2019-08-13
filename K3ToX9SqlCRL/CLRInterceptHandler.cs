using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using System.Data;
using System.Transactions;
using Microsoft.SqlServer.Server;
using System.Security.Principal;
using System.IO;
using System.Xml.Serialization;
using System.Xml;

namespace K3ToX9SqlCRL
{
    public class CLRInterceptHandler
    {
        public static string strPath = @"C:\Program Files (x86)\Kingdee\K3ERP\CUS\";
        public static string strConfig = string.Format("{0}{1}", strPath, "config.xml");
        public static LOG_TYPE ConfigLogType = LOG_TYPE.LOG_INFO;

        //特别注意：触发器虽定义为For Update，但insert时也会触发。
        [Microsoft.SqlServer.Server.SqlTrigger(Name = @"zz_tr_ICMO_X9Validate", Target = "ICMO", Event = "FOR UPDATE")]
        public static void trICMOX9Validate()
        {
            string billCode = string.Empty;
            long interID = 0L;
            int insFStatus, delFStatus;
            bool bCheckTriggerCol = false;
            SqlTriggerContext triggContext = SqlContext.TriggerContext;
            SqlPipe pipe = SqlContext.Pipe;
            SqlDataReader reader;
            initialConfig();
            
            //pipe.Send(System.Enum.GetName(typeof(LOG_TYPE), ConfigLogType));

            #region 只有生产任务单状态变化时才触发
            using (SqlConnection connection = new SqlConnection(@"context connection=true"))
            {
                connection.Open();
                SqlCommand command = new SqlCommand(@"SELECT * FROM INSERTED;",connection);
                reader = command.ExecuteReader();
                reader.Read();
                //虽reader不为null，但HasRows为false时，调用reader方法或属性会出现NullReferenceException异常。
                if (reader == null || reader.HasRows == false)
                {
                    return;
                }
                
                for (int columnNumber = 0; columnNumber < triggContext.ColumnCount; columnNumber++)
                {
                    try
                    {
                        if (triggContext.IsUpdatedColumn(columnNumber) && reader.GetName(columnNumber).Equals("FStatus", StringComparison.InvariantCultureIgnoreCase))
                        {
                            bCheckTriggerCol = true;
                        }
                    }
                    catch (NullReferenceException ex) {
                        continue;
                    }
                    
                    //pipe.Send("Updated column "
                    //   + reader.GetName(columnNumber) + "? "
                    //   + triggContext.IsUpdatedColumn(columnNumber).ToString());
                }
                pipe.Send(bCheckTriggerCol.ToString());
                if (!bCheckTriggerCol)
                {
                    return;
                }
                billCode = (string)reader["FBillNo"].ToString();
                interID = Convert.ToInt64(reader["FInterID"].ToString());
                insFStatus = Convert.ToInt32(reader["FStatus"].ToString());

                reader.Close();
            }

            #endregion

            using (SqlConnection connection = new SqlConnection(@"context connection=true"))
            {
                connection.Open();
                SqlCommand command = new SqlCommand(@"SELECT * FROM DELETED;", connection);
                reader = command.ExecuteReader();
                reader.Read();
                if (reader == null || reader.HasRows == false)
                {
                    return;
                }
                delFStatus = Convert.ToInt32(reader["FStatus"].ToString());

                reader.Close();
            }
            //结案时
            if (insFStatus == 3 && delFStatus != 3)
            {
                K3DataParaInfo docInfo = new K3DataParaInfo()
                {
                    BillCode = billCode,
                    InterID = interID,
                    TransType = 85,
                    ROB = 1,
                    CurrentUser = "X9Validator",
                    X9BillType = 5,
                    EventName = "ClosedBefore",
                    Data = "",
                };
                if (!icmoClosedHandle(docInfo))
                {
                    try
                    {
                        // Get the current transaction and roll it back.
                        Transaction trans = Transaction.Current;
                        trans.Rollback();
                    }
                    catch (SqlException ex)
                    {
                        return;
                    }

                }
            }
            //反结案时
            if (delFStatus == 3 && insFStatus != 3)
            {
                K3DataParaInfo docInfo = new K3DataParaInfo()
                {
                    BillCode = billCode,
                    InterID = interID,
                    TransType = 85,
                    ROB = 1,
                    CurrentUser = "X9Validator",
                    X9BillType = 5,
                    EventName = "UnClosedBefore",
                    Data = "",
                };

                if (!icmoClosedHandle(docInfo))
                {
                    try
                    {
                        // Get the current transaction and roll it back.
                        Transaction trans = Transaction.Current;
                        trans.Rollback();
                    }
                    catch (SqlException ex)
                    {
                        return;
                    }
                }
            }

        }

        [Microsoft.SqlServer.Server.SqlTrigger(Name = @"zz_tr_PPBOM_X9Validate", Target = "PPBOM", Event = "FOR UPDATE")]
        public static void trPPBOMX9Validate()
        {
            string billCode = string.Empty;
            long interID = 0L;
            int insFStatus, delFStatus;
            bool bCheckTriggerCol = false;
            SqlTriggerContext triggContext = SqlContext.TriggerContext;
            SqlPipe pipe = SqlContext.Pipe;
            SqlDataReader reader;

            initialConfig();

            #region 只有生产投料单状态变化时才触发
            using (SqlConnection connection = new SqlConnection(@"context connection=true"))
            {
                connection.Open();
                SqlCommand command = new SqlCommand(@"SELECT * FROM INSERTED;", connection);
                reader = command.ExecuteReader();
                reader.Read();
                if (reader == null || reader.HasRows == false)
                {
                    return;
                }
                for (int columnNumber = 0; columnNumber < triggContext.ColumnCount; columnNumber++)
                {
                    if (reader.GetName(columnNumber).Equals("FStatus", StringComparison.InvariantCultureIgnoreCase) && triggContext.IsUpdatedColumn(columnNumber))
                    {
                        bCheckTriggerCol = true;
                    }
                    //pipe.Send("Updated column "
                    //   + reader.GetName(columnNumber) + "? "
                    //   + triggContext.IsUpdatedColumn(columnNumber).ToString());
                }
                pipe.Send(bCheckTriggerCol.ToString());
                if (!bCheckTriggerCol)
                {
                    return;
                }

                billCode = (string)reader["FBillNo"].ToString();
                interID = Convert.ToInt64(reader["FInterID"].ToString());
                insFStatus = Convert.ToInt32(reader["FStatus"].ToString());

                reader.Close();
            }

            #endregion

            using (SqlConnection connection = new SqlConnection(@"context connection=true"))
            {
                connection.Open();
                SqlCommand command = new SqlCommand(@"SELECT * FROM DELETED;", connection);
                reader = command.ExecuteReader();
                reader.Read();
                if (reader == null || reader.HasRows == false)
                {
                    return;
                }
                delFStatus = Convert.ToInt32(reader["FStatus"].ToString());

                reader.Close();
            }
            //审核时
            if (insFStatus == 1 && delFStatus == 0)
            {
                K3DataParaInfo docInfo = new K3DataParaInfo()
                {
                    BillCode = billCode,
                    InterID = interID,
                    TransType = 88,
                    ROB = 1,
                    CurrentUser = "X9Validator",
                    X9BillType = 7,
                    EventName = "ApprovedBefore",
                    Data = "",
                };
                if (!icmoClosedHandle(docInfo))
                {
                    try
                    {
                        // Get the current transaction and roll it back.
                        Transaction trans = Transaction.Current;
                        trans.Rollback();
                    }
                    catch (SqlException ex)
                    {
                        return;
                    }

                }
            }
            //弃审时
            if (delFStatus == 1 && insFStatus == 0)
            {
                K3DataParaInfo docInfo = new K3DataParaInfo()
                {
                    BillCode = billCode,
                    InterID = interID,
                    TransType = 88,
                    ROB = 1,
                    CurrentUser = "X9Validator",
                    X9BillType = 7,
                    EventName = "UnApprovedBefore",
                    Data = "",
                };

                if (!ppbomApprovedHandle(docInfo))
                {
                    try
                    {
                        // Get the current transaction and roll it back.
                        Transaction trans = Transaction.Current;
                        trans.Rollback();
                    }
                    catch (SqlException ex)
                    {
                        return;
                    }

                }
            }

        }

        //生产任务单结案、反结案一并处理。
        private static bool icmoClosedHandle(K3DataParaInfo docInfo)
        {
            SqlPipe pipe = SqlContext.Pipe;
            bool bRlt = true;
            try
            {
                debugLogger(docInfo, string.Format("进入基类{0}事件响应", docInfo.EventName));
                K3InterceptConfig itemConfig = validateBusinessEnable(docInfo);
                //K3InterceptConfig itemConfig = new K3InterceptConfig()
                //{
                //    ServiceAddress = "192.168.1.100/WebService/WebService.asmx",
                //    X9BusinessType = 5,
                //    InterceptEvent = "ClosedBefore",
                //    IsEnable = 1,
                //};
                if (itemConfig != null)
                {
                    ResultInfo rltInfo = defaultEventHandle(docInfo, itemConfig);
                    if (rltInfo != null)
                    {
                        bRlt = rltInfo.IsSuccess;
                    }
                    infoLogger(docInfo, string.Format("X9系统业务校验事件{0}服务，返回结果为{1}。", docInfo.EventName, rltInfo.IsSuccess.ToString()));
                }
                else
                {
                    infoLogger(docInfo, string.Format("未启用X9系统对K3事件{0}的拦截", docInfo.EventName));
                }
            }
            catch (Exception ex)
            {
                infoLogger(docInfo, string.Format("执行基类缺省拦截处理：{0}事件。异常：{1}", docInfo.EventName, ex.Message));
                bRlt = true;
            }
            return bRlt;
        }

        //生产投料单结案、反结案一并处理。
        private static bool ppbomApprovedHandle(K3DataParaInfo docInfo)
        {
            SqlPipe pipe = SqlContext.Pipe;
            bool bRlt = true;
            try
            {
                debugLogger(docInfo, string.Format("进入基类{0}事件响应", docInfo.EventName));
                K3InterceptConfig itemConfig = validateBusinessEnable(docInfo);
                if (itemConfig != null)
                {
                    ResultInfo rltInfo = defaultEventHandle(docInfo, itemConfig);
                    if (rltInfo != null)
                    {
                        bRlt = rltInfo.IsSuccess;
                        infoLogger(docInfo, string.Format("X9系统业务校验事件{0}服务，返回结果为{1}。", docInfo.EventName, rltInfo.IsSuccess.ToString()));
                    }
                    else
                    {
                        infoLogger(docInfo, string.Format("未启用X9系统对K3事件{0}的拦截", docInfo.EventName));
                    }
                }
            }
            catch (Exception ex)
            {
                infoLogger(docInfo, string.Format("执行基类缺省拦截处理：{0}事件。异常：{1}", docInfo.EventName, ex.Message));
                bRlt = true;
            }

            return bRlt;
        }

        private static ResultInfo defaultEventHandle( K3DataParaInfo docInfo, K3InterceptConfig busiConfig)
        {
            SqlPipe pipe = SqlContext.Pipe;
            try
            {
                string strRlt = string.Empty;
                debugLogger(docInfo, string.Format("进入X9系统业务校验事件{0}服务中", docInfo.EventName));
                //WindowsIdentity windowsIdentity = null;
                //WindowsImpersonationContext userImpersonated = null;
                //windowsIdentity = SqlContext.WindowsIdentity;

                //// Switch the context to local service account user (a domain user) 
                //// by getting its identity in order to call the web service 
                //userImpersonated = windowsIdentity.Impersonate();
                
                //if (userImpersonated != null)
                //{
                //    // Create the instance of a web service to be called. // Remember this is not actual // CAAT search web service but a similar fake web service just for testing. 
                //    pipe.Send("create service");
                //    X9WebService.WebService svValidateBM = new X9WebService.WebService();
                //    pipe.Send("end service");
                //    svValidateBM.Url = string.Format("http://{0}", busiConfig.ServiceAddress);
                //    string strDocInfo = SimpleConfig.XmlSerialize<K3DataParaInfo>(docInfo, Encoding.Unicode);
                //    pipe.Send(strDocInfo);

                //    // Execute the web service and get the IEnumerable type results 
                //    strRlt = svValidateBM.SynchBillFromK3ToX9(strDocInfo);
                //    // Switch the context back to the SQL Server 
                //    userImpersonated.Undo();
                //}
                //else
                //{
                //    pipe.Send("userImpersonated is null");
                //}

                X9WebService.WebService svValidateBM = new X9WebService.WebService();
                svValidateBM.Url = string.Format("http://{0}", busiConfig.ServiceAddress);
                string strDocInfo = XmlSerialize<K3DataParaInfo>(docInfo, Encoding.Unicode);
                pipe.Send(strDocInfo);
                //string strHttpEncoding = HttpUtility.HtmlEncode(strDocInfo);
                strRlt = svValidateBM.SynchBillFromK3ToX9(strDocInfo);
                
                if (!string.IsNullOrEmpty(strRlt))
                {
                    pipe.Send(strRlt);
                    //strRlt = strRlt.Replace(" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\"", "");
                    //string strHttpDecoding = HttpUtility.HtmlDecode(strRlt);
                    ResultInfo rltInfo = XmlDeserialize<ResultInfo>(strRlt, Encoding.Unicode);
                    return rltInfo;
                }
                return null;
            }
            catch (Exception ex)
            {
                //LogInfoHelp.infoLog(eventName, docInfo, string.Format("调用X9系统服务时，异常：{0}", ex.Message));
                infoLogger(docInfo, string.Format("调用X9系统服务时，异常：{0}", ex.Message));
                docInfo.Data = ex.Message;
                cacheDocInfo(docInfo, busiConfig);
                return null;
            }
        }

        private static void cacheDocInfo(K3DataParaInfo docInfo, K3InterceptConfig busiConfig)
        {
            try
            {
                using (SqlConnection sqlconn = new SqlConnection(@"context connection=true"))
                {
                    sqlconn.Open();
                    using (SqlCommand sqlcommCache = new SqlCommand("zz_pr_X9WebSrvBackup_Save", sqlconn))
                    {
                        sqlcommCache.CommandType = CommandType.StoredProcedure;
                        string strDocInfo = XmlSerialize<K3DataParaInfo>(docInfo, Encoding.Unicode);
                        SqlParameter sqlparamDocInfo = new SqlParameter("@DocInfo", SqlDbType.Xml);
                        sqlparamDocInfo.Value = strDocInfo;
                        SqlParameter sqlparamUrl = new SqlParameter("@Url", SqlDbType.NVarChar);
                        sqlparamUrl.Value = string.Format("http://{0}", busiConfig.ServiceAddress);
                        sqlcommCache.Parameters.Add(sqlparamDocInfo);
                        sqlcommCache.Parameters.Add(sqlparamUrl);

                        sqlcommCache.ExecuteNonQuery();
                    }
                }
                infoLogger(docInfo, string.Format("X9系统服务调用异常，事件{0}数据被缓存。", docInfo.EventName));
            }
            catch (Exception ex)
            {
                //throw new RankException(string.Format("暂存DocInfo存储异常：{0}", string.Empty), ex);
                //MessageBox.Show(string.Format("{0}\t暂存DocInfo存储异常：{1}", Environment.NewLine, ex.Message));
                infoLogger(docInfo, string.Format("暂存DocInfo存储异常：{0}", ex.Message));
                throw new Exception(string.Format("{0}\t暂存DocInfo存储异常：{1}", Environment.NewLine, ex.Message), ex);

            }
        }

        //做debug日志收录时，只有日志级别是Debug才需要保存，用于记录程序详细的执行输出。
        private static void debugLogger(K3DataParaInfo docInfo, string logMsg)
        {
            if (ConfigLogType == LOG_TYPE.LOG_DEBUG || ConfigLogType == LOG_TYPE.LOG_INFO)
            {
                logInner(docInfo,logMsg);
            }
        }

        //做Info日志收录时，不管日志级别是Debug还是Info，都保存记录。
        private static void infoLogger(K3DataParaInfo docInfo,string logMsg)
        {
            if (ConfigLogType == LOG_TYPE.LOG_INFO || ConfigLogType == LOG_TYPE.LOG_DEBUG)
            {
                logInner(docInfo,logMsg);
            }
        }

        private static void logInner(K3DataParaInfo docInfo, string logMsg)
        {
            try
            {
                docInfo.Data = logMsg;
                using (SqlConnection sqlconn = new SqlConnection(@"context connection=true"))
                {
                    sqlconn.Open();
                    using (SqlCommand sqlcommCache = new SqlCommand("zz_pr_X9ToK3Log_Save", sqlconn))
                    {
                        sqlcommCache.CommandType = CommandType.StoredProcedure;
                        string strDocInfo = XmlSerialize<K3DataParaInfo>(docInfo, Encoding.Unicode);
                        SqlParameter sqlparamDocInfo = new SqlParameter("@DocInfo", SqlDbType.Xml);
                        sqlparamDocInfo.Value = strDocInfo;
                        SqlParameter sqlparamType = new SqlParameter("@Type", SqlDbType.NVarChar);
                        sqlparamType.Value = System.Enum.GetName(typeof(LOG_TYPE), ConfigLogType);
                        sqlcommCache.Parameters.Add(sqlparamDocInfo);
                        sqlcommCache.Parameters.Add(sqlparamType);

                        sqlcommCache.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                //throw new RankException(string.Format("暂存DocInfo存储异常：{0}", string.Empty), ex);
                //MessageBox.Show(string.Format("{0}\t暂存DocInfo存储异常：{1}", Environment.NewLine, ex.Message));
                throw new Exception(string.Format("{0}\t日志存储异常：{1}", Environment.NewLine, ex.Message), ex);
            }
        }

        private static void initialConfig()
        {
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(strConfig);

            XmlNodeList lstLog = xmlDoc.GetElementsByTagName("LogInfoType");
            if (lstLog.Count > 0)
            {
                switch (lstLog[0].Attributes["level"].Value.ToUpper())
                {
                    case "DEBUG":
                        ConfigLogType = LOG_TYPE.LOG_DEBUG;
                        break;
                    default:
                        ConfigLogType = LOG_TYPE.LOG_INFO;
                        break;
                }
            }
        }

        public static K3InterceptConfig validateBusinessEnable(K3DataParaInfo docInfo)
        {
            SqlPipe pipe = SqlContext.Pipe;
            K3InterceptConfig busiConfig = null;
            string strViewXml = string.Empty;
            using (SqlConnection sqlconn = new SqlConnection(@"context connection=true"))
            {
                sqlconn.Open();
                using (SqlCommand sqlcommPOView = new SqlCommand("zz_pr_BusiConfigSingle_View", sqlconn))
                {
                    sqlcommPOView.CommandType = CommandType.StoredProcedure;

                    SqlParameter sqlparaResult = new SqlParameter("@Infos", SqlDbType.Xml, 5000);
                    sqlparaResult.Direction = ParameterDirection.Output;
                    SqlParameter sqlparaDocInfo = new SqlParameter("@DocInfo", SqlDbType.Xml);
                    sqlparaDocInfo.Direction = ParameterDirection.Input;
                    sqlparaDocInfo.Value = XmlSerialize<K3DataParaInfo>(docInfo, Encoding.Unicode);
                    sqlcommPOView.Parameters.Add(sqlparaDocInfo);
                    sqlcommPOView.Parameters.Add(sqlparaResult);

                    sqlcommPOView.ExecuteNonQuery();
                    strViewXml = sqlparaResult.Value.ToString();

                    busiConfig = XmlDeserialize<K3InterceptConfig>(strViewXml, Encoding.UTF8);
                }
            }

            return busiConfig; ;
        }

        public static string XmlSerialize<T>(T obj, Encoding encoding)
        {
            using (var memoryStream = new MemoryStream())
            {
                XmlWriterSettings settings = new XmlWriterSettings();
                settings.Indent = true;
                settings.NewLineChars = "\r\n";
                settings.Encoding = encoding;
                settings.IndentChars = "";

                XmlSerializerNamespaces _defaultNamespace;
                _defaultNamespace = new XmlSerializerNamespaces();
                //_defaultNamespace.Add(string.Empty, string.Empty);
                //*轩辕天恩 序列化命名空间
                _defaultNamespace.Add("xsi", @"http://www.w3.org/2001/XMLSchema-instance");
                _defaultNamespace.Add("xsd", @"http://www.w3.org/2001/XMLSchema");

                using (XmlWriter writer = XmlWriter.Create(memoryStream, settings))
                {
                    var serializer = new XmlSerializer(typeof(T));

                    serializer.Serialize(writer, obj, _defaultNamespace);
                    writer.Close();
                }

                memoryStream.Position = 0;
                using (StreamReader reader = new StreamReader(memoryStream, encoding))
                {
                    return reader.ReadToEnd();
                }
            }
        }

        public static T XmlDeserialize<T>(string xml, Encoding encoding)
        {
            using (var memoryStream = new MemoryStream(encoding.GetBytes(xml)))
            {
                var serializer = new XmlSerializer(typeof(T));
                var obj = serializer.Deserialize(memoryStream);
                return obj == null ? default(T) : (T)obj;
            }
        }
    }
}
