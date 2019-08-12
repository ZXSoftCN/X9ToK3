using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using System.Data;
using System.Transactions;
using Microsoft.SqlServer.Server;
using System.Security.Principal;

namespace SqlCLRTest
{
    public class CLRInterceptHandler
    {
        [Microsoft.SqlServer.Server.SqlTrigger(Name = @"tr_ICMOX9Validate", Target = "t_Test", Event = "FOR UPDATE")]
        public static void trICMOX9Validate()
        {
            string billCode = string.Empty;
            long interID = 0L;
            int insFStatus, delFStatus;
            bool bCheckTriggerCol = false;
            SqlTriggerContext triggContext = SqlContext.TriggerContext;
            SqlPipe pipe = SqlContext.Pipe;
            SqlDataReader reader;

            //WindowsIdentity windowsIdentity = null;
            //WindowsImpersonationContext userImpersonated = null;

            //pipe.Send("1");
            //windowsIdentity = SqlContext.WindowsIdentity;
            //pipe.Send(windowsIdentity == null ? "null" : windowsIdentity.AuthenticationType);
            //// Switch the context to local service account user (a domain user) 
            //// by getting its identity in order to call the web service 
            //userImpersonated = windowsIdentity.Impersonate();
            //pipe.Send("2");

            //if (userImpersonated != null)
            //{
            //    // Create the instance of a web service to be called. // Remember this is not actual // CAAT search web service but a similar fake web service just for testing. 
            //    pipe.Send("create service");
            //    LogInfoHelp.Log("Begin Login", LOG_TYPE.LOG_DEBUG);

            //    userImpersonated.Undo();
            //}
            //else
            //{
            //    pipe.Send("userImpersonated is null");
            //}

            //只有生产任务单状态变化时才触发
            using (SqlConnection connection = new SqlConnection(@"context connection=true"))
            {
                connection.Open();
                SqlCommand command = new SqlCommand(@"SELECT * FROM INSERTED;",connection);
                reader = command.ExecuteReader();
                reader.Read();

                for (int columnNumber = 0; columnNumber < triggContext.ColumnCount; columnNumber++)
                {
                    if (reader.GetName(columnNumber).Equals("FStatus",StringComparison.InvariantCultureIgnoreCase) && triggContext.IsUpdatedColumn(columnNumber))
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
                pipe.Send("FBillNo");
                billCode = (string)reader["FBillNo"].ToString();
                interID = Convert.ToInt64(reader["FInterID"].ToString());
                pipe.Send("FStatus");
                pipe.Send(reader["FStatus"].ToString());
                insFStatus = Convert.ToInt32(reader["FStatus"].ToString());

                reader.Close();
            }

            using (SqlConnection connection = new SqlConnection(@"context connection=true"))
            {
                connection.Open();
                SqlCommand command = new SqlCommand(@"SELECT * FROM DELETED;", connection);
                reader = command.ExecuteReader();
                reader.Read();

                delFStatus = Convert.ToInt32(reader["FStatus"].ToString());

                reader.Close();
            }
            //结案时
            if (insFStatus == 3 && delFStatus != 3)
            {
                pipe.Send("if (insFStatus == 3 && delFStatus != 3)");
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
                if (!icmoCloseHandle(docInfo))
                {
                    pipe.Send("!icmoCloseHandle(docInfo)");
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
                pipe.Send("delFStatus == 3 && insFStatus != 3");
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

                if (!icmoCloseHandle(docInfo))
                {
                    pipe.Send("!icmoCloseHandle(docInfo)");
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

        //结案、反结案一并处理。
        private static bool icmoCloseHandle(K3DataParaInfo docInfo)
        {
            SqlPipe pipe = SqlContext.Pipe;
            bool bRlt = true;
            try
            {
                pipe.Send("icmoCloseHandle");
                K3InterceptConfig itemConfig = SimpleConfig.validateBusinessEnable(docInfo);
                //K3InterceptConfig itemConfig = new K3InterceptConfig()
                //{
                //    ServiceAddress = "192.168.1.100/WebService/WebService.asmx",
                //    X9BusinessType = 5,
                //    InterceptEvent = "ClosedBefore",
                //    IsEnable = 1,
                //};
                if (itemConfig != null)
                {
                    pipe.Send("itemConfig != null");
                    ResultInfo rltInfo = defaultEventHandle(docInfo, itemConfig);
                    if (rltInfo != null)
                    {
                        bRlt = rltInfo.IsSuccess;
                    }
                }
            }
            catch (Exception ex)
            {
                pipe.Send(ex.Message);
                bRlt = true;
            }

            return bRlt;
        }

        private static ResultInfo defaultEventHandle( K3DataParaInfo docInfo, K3InterceptConfig busiConfig)
        {
            SqlPipe pipe = SqlContext.Pipe;
            try
            {
                pipe.Send("defaultEventHandle");
                string strRlt = string.Empty;

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
                string strDocInfo = SimpleConfig.XmlSerialize<K3DataParaInfo>(docInfo, Encoding.Unicode);
                pipe.Send(strDocInfo);
                //string strHttpEncoding = HttpUtility.HtmlEncode(strDocInfo);
                strRlt = svValidateBM.SynchBillFromK3ToX9(strDocInfo);
                
                if (!string.IsNullOrEmpty(strRlt))
                {
                    pipe.Send(strRlt);
                    //strRlt = strRlt.Replace(" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\"", "");
                    //string strHttpDecoding = HttpUtility.HtmlDecode(strRlt);
                    ResultInfo rltInfo = SimpleConfig.XmlDeserialize<ResultInfo>(strRlt, Encoding.Unicode);
                    return rltInfo;
                }
                return null;
            }
            catch (Exception ex)
            {
                //LogInfoHelp.infoLog(eventName, docInfo, string.Format("调用X9系统服务时，异常：{0}", ex.Message));
                pipe.Send(string.Format("Exception cacheDocInfo:{0}",ex.Message));
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
                        string strDocInfo = SimpleConfig.XmlSerialize<K3DataParaInfo>(docInfo, Encoding.Unicode);
                        SqlParameter sqlparamDocInfo = new SqlParameter("@DocInfo", SqlDbType.Xml, 5000);
                        sqlparamDocInfo.Value = strDocInfo;
                        SqlParameter sqlparamUrl = new SqlParameter("@Url", SqlDbType.NVarChar, 100);
                        sqlparamUrl.Value = string.Format("http://{0}", busiConfig.ServiceAddress);
                        sqlcommCache.Parameters.Add(sqlparamDocInfo);
                        sqlcommCache.Parameters.Add(sqlparamUrl);

                        sqlcommCache.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                //throw new RankException(string.Format("暂存DocInfo存储异常：{0}", string.Empty), ex);
                //MessageBox.Show(string.Format("{0}\t暂存DocInfo存储异常：{1}", Environment.NewLine, ex.Message));
                throw new Exception(string.Format("{0}\t暂存DocInfo存储异常：{1}", Environment.NewLine, ex.Message), ex);
            }
        }

        [Microsoft.SqlServer.Server.SqlTrigger(Name = @"tr_Test", Target = "t_Test", Event = "FOR UPDATE")]
        public static void trTest()
        {
            //using (SqlConnection connection = new SqlConnection(@"context connection=true"))
            //{
            //    SqlContext.Pipe.Send(string.Format("Current ConnectString is :{0}", connection.ConnectionString));
            //}
            string strViewXml = string.Empty;
            using (SqlConnection sqlconn = new SqlConnection(@"context connection=true"))
            {
                sqlconn.Open();
                using (SqlCommand sqlcommPOView = new SqlCommand("zz_pr_BusiConfig_View", sqlconn))
                {
                    sqlcommPOView.CommandType = CommandType.StoredProcedure;

                    SqlParameter sqlparaResult = new SqlParameter("@Infos", SqlDbType.Xml, 5000);
                    sqlparaResult.Direction = ParameterDirection.Output;
                    sqlcommPOView.Parameters.Add(sqlparaResult);

                    sqlcommPOView.ExecuteNonQuery();
                    strViewXml = sqlparaResult.Value.ToString();
                    //MessageBox.Show(strViewXml);
                    //特别注意：Encoding.Unicode编码在COM封装调用时会提示:XML文档(1,2)中有错误。必须改成UTF8

                    SqlContext.Pipe.Send(string.Format("Current ConnectString is :{0}", strViewXml));
                    //Debug.WriteLine(lst3.Count());
                }
                //TempDBConnectString = sqlConn.Replace(strK3DB, "tempdb");
            }
        }
    }
}
