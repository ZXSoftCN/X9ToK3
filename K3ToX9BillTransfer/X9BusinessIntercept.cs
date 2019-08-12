using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using System.Data;
using System.Windows.Forms;
using System.Web;

namespace K3ToX9BillTransfer
{
    public class X9BusinessIntercept : BasicProcessor
    {
        public override ResultInfo addBeforeExtend(K3DataParaInfo docInfo, K3InterceptConfig busiConfig)
        {
            return defaultEventHandle(InterceptEvent.AddBefore, docInfo, busiConfig); ;
        }

        public override ResultInfo addAfterExtend(K3DataParaInfo docInfo, K3InterceptConfig busiConfig)
        {
            return defaultEventHandle(InterceptEvent.AddAfter, docInfo, busiConfig);
        }

        public override ResultInfo deleteBeforeExtend(K3DataParaInfo docInfo, K3InterceptConfig busiConfig)
        {
            return defaultEventHandle(InterceptEvent.DeleteBefore, docInfo, busiConfig);
        }

        public override ResultInfo deleteAfterExtend(K3DataParaInfo docInfo, K3InterceptConfig busiConfig)
        {
            return defaultEventHandle(InterceptEvent.DeleteAfter, docInfo, busiConfig);
        }
        public override ResultInfo approvedBeforeExtend(K3DataParaInfo docInfo, K3InterceptConfig busiConfig)
        {
            return defaultEventHandle(InterceptEvent.ApprovedBefore,docInfo, busiConfig);
        }
        public override ResultInfo approvedAfterExtend(K3DataParaInfo docInfo, K3InterceptConfig busiConfig)
        {
            return defaultEventHandle(InterceptEvent.ApprovedAfter, docInfo, busiConfig);
        }

        public override ResultInfo unApprovedBeforeExtend(K3DataParaInfo docInfo, K3InterceptConfig busiConfig)
        {
            return defaultEventHandle(InterceptEvent.UnApprovedBefore, docInfo, busiConfig);
        }

        public override ResultInfo unApprovedAfterExtend(K3DataParaInfo docInfo, K3InterceptConfig busiConfig)
        {
            return defaultEventHandle(InterceptEvent.UnApprovedAfter, docInfo, busiConfig);
        }
        public override ResultInfo unKnownExtend(K3DataParaInfo docInfo, K3InterceptConfig busiConfig)
        {
            return defaultEventHandle(InterceptEvent.UnKnownEvent, docInfo, busiConfig);
        }

        private ResultInfo defaultEventHandle(string eventName, K3DataParaInfo docInfo, K3InterceptConfig busiConfig)
        {
            try
            {
                X9WebService.WebService svValidateBM = new X9WebService.WebService();
                svValidateBM.Url = string.Format("http://{0}", busiConfig.ServiceAddress);
                string strDocInfo = XmlSerializerHelper.XmlSerialize<K3DataParaInfo>(docInfo, Encoding.Unicode);
                //string strHttpEncoding = HttpUtility.HtmlEncode(strDocInfo);
                string strRlt = svValidateBM.SynchBillFromK3ToX9(strDocInfo);
                if (!string.IsNullOrEmpty(strRlt))
                {
                    //strRlt = strRlt.Replace(" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\"", "");
                    //string strHttpDecoding = HttpUtility.HtmlDecode(strRlt);
                    ResultInfo rltInfo = XmlSerializerHelper.XmlDeserialize<ResultInfo>(strRlt, Encoding.Unicode);
                    return rltInfo;
                }
                return null;
            }
            catch (Exception ex)
            {
                //LogInfoHelp.infoLog(eventName, docInfo, string.Format("调用X9系统服务时，异常：{0}", ex.Message));
                docInfo.Data = ex.Message;
                cacheDocInfo(docInfo, busiConfig);
                throw new Exception(string.Format("调用X9系统服务时，{0}", ex.Message),ex);
            }
        }

        private void cacheDocInfo(K3DataParaInfo docInfo, K3InterceptConfig busiConfig)
        {
            try
            {
                using (SqlConnection sqlconn = new SqlConnection(ServiceConfig.Instance.K3ConnectString))
                {
                    sqlconn.Open();
                    using (SqlCommand sqlcommCache = new SqlCommand("zz_pr_X9WebSrvBackup_Save", sqlconn))
                    {
                        sqlcommCache.CommandType = CommandType.StoredProcedure;
                        string strDocInfo = XmlSerializerHelper.XmlSerialize<K3DataParaInfo>(docInfo, Encoding.Unicode);
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
                throw new Exception(string.Format("{0}\t暂存DocInfo存储异常：{1}", Environment.NewLine, ex.Message),ex);
            }
        }
        
    }
}
