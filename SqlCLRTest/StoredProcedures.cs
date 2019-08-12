using System;
using System.Data;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using Microsoft.SqlServer.Server;
using System.Xml.Serialization;
using System.Linq;
using System.Text;
using System.IO;
using SqlCLRTest;
using System.Xml;
using System.Collections.Generic;
using System.Security.Principal;

public partial class StoredProcedures
{
    [Microsoft.SqlServer.Server.SqlFunction]
    public static SqlString XMLTest()
    {
        Person p = new Person();
        return new SqlString(p.GetXml());
    }

    [Microsoft.SqlServer.Server.SqlFunction]
    public static SqlString XMLTestRe()
    {
        Person p = new Person();
        string str = p.GetXml();
        Person p1 = XmlDeserialize<Person>(str, Encoding.Unicode);
        return new SqlString("ok");
    }

    [Microsoft.SqlServer.Server.SqlFunction]
    public static SqlString K3ParaInfoTest()
    {
        
        K3DataParaInfo info = new K3DataParaInfo()
        {
            BillCode = "abc001",
            EventName = "ApprovedBefore",
            X9BillType = 5,
        };

        XmlSerializer ser = new XmlSerializer(typeof(K3DataParaInfo));
        StringBuilder sb = new StringBuilder();
        StringWriter wr = new StringWriter(sb);
        ser.Serialize(wr, info);

        return new SqlString(sb.ToString());
    }

    [Microsoft.SqlServer.Server.SqlFunction]
    public static SqlString K3ParaInfoAdvTest()
    {

        K3DataParaInfo info = new K3DataParaInfo()
        {
            BillCode = "abc001",
            EventName = "ApprovedBefore",
            X9BillType = 5,
        };

        string str = XmlSerialize<K3DataParaInfo>(info, Encoding.Unicode);

        return new SqlString(str);
    }

    [Microsoft.SqlServer.Server.SqlProcedure]
    public static void StoredProcedure1()
    {
        // 在此处放置代码
        SqlPipe pipe = SqlContext.Pipe;
        List<K3InterceptConfig> BusiConfigs = null;
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
                BusiConfigs = XmlDeserialize<List<K3InterceptConfig>>(strViewXml, Encoding.UTF8);
            }
        }

        List<K3InterceptConfig> lstConfig = (from s in BusiConfigs
                                             where s.InterceptEvent == "ClosedBefore" && s.X9BusinessType == 5 && s.IsEnable == 1
                                             orderby s.Id descending
                                             select s).ToList<K3InterceptConfig>();
        pipe.Send(lstConfig.Count.ToString());
    }

    [Microsoft.SqlServer.Server.SqlProcedure]
    public static void StoredProcedure2()
    {
        // 在此处放置代码
        SqlPipe pipe = SqlContext.Pipe;
        K3InterceptConfig BusiConfigs = null;
        string strViewXml = string.Empty;

        K3DataParaInfo docInfo = new K3DataParaInfo()
        {
            BillCode = "abc",
            InterID = 1233,
            TransType = 85,
            ROB = 1,
            CurrentUser = "X9Validator",
            X9BillType = 5,
            EventName = "UnClosedBefore",
            Data = "",
        };

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
                pipe.Send("begin");
                sqlcommPOView.ExecuteNonQuery();
                strViewXml = sqlparaResult.Value.ToString();
                pipe.Send(strViewXml);
                BusiConfigs = XmlDeserialize<K3InterceptConfig>(strViewXml, Encoding.UTF8);
            }
        }
        

        defaultEventHandle(docInfo, BusiConfigs);
        cacheDocInfo(docInfo, BusiConfigs);
    }

    private static ResultInfo defaultEventHandle(K3DataParaInfo docInfo, K3InterceptConfig busiConfig)
    {
        SqlPipe pipe = SqlContext.Pipe;
        try
        {
            pipe.Send("defaultEventHandle");
            string strRlt = string.Empty;
            SqlCLRTest.X9WebService.WebService svValidateBM = new SqlCLRTest.X9WebService.WebService();
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
            pipe.Send(string.Format("Exception cacheDocInfo:{0}", ex.Message));
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
        }
        catch (Exception ex)
        {
            //throw new RankException(string.Format("暂存DocInfo存储异常：{0}", string.Empty), ex);
            //MessageBox.Show(string.Format("{0}\t暂存DocInfo存储异常：{1}", Environment.NewLine, ex.Message));
            throw new Exception(string.Format("{0}\t暂存DocInfo存储异常：{1}", Environment.NewLine, ex.Message), ex);
        }
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

    public class Person
    {
        public String m_FirstName = "Jane";
        public String m_LastName = "Dow";

        public String GetXml()
        {
            XmlSerializer ser = new XmlSerializer(typeof(Person));
            StringBuilder sb = new StringBuilder();
            StringWriter wr = new StringWriter(sb);
            ser.Serialize(wr, this);

            return sb.ToString();
        }

    }
};

