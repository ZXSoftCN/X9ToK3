using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using System.Data;
using System.Diagnostics;
using System.Windows.Forms;
using K3ToX9BillTransfer.UI;

namespace K3ToX9BillTransfer
{
    sealed class ServiceConfig
    {
        private ServiceConfig() { }

        private ServiceConfig(string sqlConn)
        {
            K3ConnectString = sqlConn;

            string strViewXml = string.Empty;
            using (SqlConnection sqlconn = new SqlConnection(sqlConn))
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
                    BusiConfigs = XmlSerializerHelper.XmlDeserialize<List<K3InterceptConfig>>(strViewXml, Encoding.UTF8);
                    //Debug.WriteLine(lst3.Count());
                }
                string strK3DB = sqlconn.Database;
                //TempDBConnectString = sqlConn.Replace(strK3DB, "tempdb");
                TempDBConnectString = "User ID=sa;Password=as;Data Source=KINGYU-9EB62423;Initial Catalog=tempdb";
            }
        }

        public List<K3InterceptConfig> BusiConfigs { get; private set; }

        public string K3ConnectString { get; private set; }
        public string TempDBConnectString { get; set; }

        /// <summary>
        /// 返回false时，表示接口功能不能初始化。未建立后台表、检索数据异常等会造成异常。
        /// </summary>
        /// <param name="sqlConn"></param>
        /// <returns><\returns>
        public static bool Initial(string sqlConn)
        {
            try
            {
                if (Instance == null)
                {
                    lock (typeof(ServiceConfig))
                    {
                        if (Instance == null)
                        {
                            Instance = new ServiceConfig(sqlConn);
                        }
                    }
                }
                return true;
            }
            catch(Exception ex)
            {
                LogInfoHelp.Log(string.Format("初始化X9配置K3业务类型时异常：{0}", ex.Message), LOG_TYPE.LOG_INFO);
                if (CommonFunc.ConfigLogType > LOG_TYPE.LOG_INFO)
                {
                    frmMessageSingle.Show("初始化X9配置K3业务类型时异常：ServiceConfig.Initial", string.Format("初始化X9配置K3业务类型时异常：{0}", ex.Message));
                }
                return false;
            }
        }

        public static ServiceConfig Instance { get; private set; }

    }
}
