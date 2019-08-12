using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml.Serialization;
using System.Data.SqlClient;
using System.Data;
using System.Xml;
using Microsoft.SqlServer.Server;

namespace K3ToX9SqlCRL
{
    public static class SimpleConfig
    {
        public static string strPath = @"C:\Program Files (x86)\Kingdee\K3ERP\CUS\";
        public static LOG_TYPE ConfigLogType = LOG_TYPE.LOG_DEBUG;

        public static T XmlDeserialize<T>(string xml, Encoding encoding)
        {
            using (var memoryStream = new MemoryStream(encoding.GetBytes(xml)))
            {
                var serializer = XmlSerializer.FromTypes(new[] { typeof(T) }).FirstOrDefault();
                var obj = serializer.Deserialize(memoryStream);
                return obj == null ? default(T) : (T)obj;
            }
        }

        public static string XmlSerialize<T>(this T obj, Encoding encoding)
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
                    var serializer = XmlSerializer.FromTypes(new[] { typeof(T) }).FirstOrDefault();

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

        public static K3InterceptConfig validateBusinessEnable(K3DataParaInfo docInfo)
        {
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
                    BusiConfigs = SimpleConfig.XmlDeserialize<List<K3InterceptConfig>>(strViewXml, Encoding.UTF8);
                }
            }

            List<K3InterceptConfig> lstConfig = (from s in BusiConfigs
                                                 where s.InterceptEvent == docInfo.EventName && s.X9BusinessType == docInfo.X9BillType && s.IsEnable == 1
                orderby s.Id descending
                select s).ToList<K3InterceptConfig>();
            pipe.Send(lstConfig.Count.ToString());
            foreach (var item in lstConfig)
            {
                if (DateTime.Now.Date >= item.EnableDate && DateTime.Now.Date <= item.DisableDate)
                {
                    return item;
                }
            }
            return null; ;
        }

    }
}
