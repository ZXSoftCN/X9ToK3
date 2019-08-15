using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Windows.Forms;
using System.Data.SqlClient;
using System.Data;
using System.Diagnostics;

namespace K3ToX9BillTransfer
{
    public class CommonFunc
    {
        public static string strPath = @"C:\Program Files (x86)\Kingdee\K3ERP\CUS\";
        //public static string strPath = AppDomain.CurrentDomain.BaseDirectory;

        public static string strConfig = string.Format("{0}{1}", strPath , "config.xml");

        static List<KeyValuePair<string, int>> K3TransTypeToX9BillType = new List<KeyValuePair<string, int>>();
        public static LOG_TYPE ConfigLogType = LOG_TYPE.LOG_INFO;
        public const string K3TypeFormat = "{0}({1})"; 
        static CommonFunc()
        {
            //K3TransTypeToX9BillType.Add(new KeyValuePair<string, int>(string.Format(k3BillFormat, 71, 1), 2));//采购订单
            //K3TransTypeToX9BillType.Add(new KeyValuePair<string, int>(string.Format(k3BillFormat, 1007105, 1), 3));//委外订单
            //K3TransTypeToX9BillType.Add(new KeyValuePair<string, int>(string.Format(k3BillFormat, 28, 1), 15));//委外领料单→委外领料申请
            //K3TransTypeToX9BillType.Add(new KeyValuePair<string, int>(string.Format(k3BillFormat, 28, -1), 16));//红字委外领料单→委外退料单
            //K3TransTypeToX9BillType.Add(new KeyValuePair<string, int>(string.Format(k3BillFormat, 81, 1), 4));//销售订单→销售订单
            //K3TransTypeToX9BillType.Add(new KeyValuePair<string, int>(string.Format(k3BillFormat, 83, 1), 17));//发货单→销售发货通知
            //K3TransTypeToX9BillType.Add(new KeyValuePair<string, int>(string.Format(k3BillFormat, 82, 1), 18));//退货单→销售退货通知
            //K3TransTypeToX9BillType.Add(new KeyValuePair<string, int>(string.Format(k3BillFormat, 85, 1), 5));//生产任务单→生产工单
            //K3TransTypeToX9BillType.Add(new KeyValuePair<string, int>(string.Format(k3BillFormat, 88, 1), 7));//投料单→工单用料[IcClassType:-88]
            //K3TransTypeToX9BillType.Add(new KeyValuePair<string, int>(string.Format(k3BillFormat, 24, 1), 8));//领料单→生产领料申请
            //K3TransTypeToX9BillType.Add(new KeyValuePair<string, int>(string.Format(k3BillFormat, 24, -1), 9));//红字领料单→生产退料申请

            //K3TransTypeToX9BillType.Add(new KeyValuePair<string, int>(string.Format(k3BillFormat, 29, 1), 10));//其他出库单→其他出库申请
            //K3TransTypeToX9BillType.Add(new KeyValuePair<string, int>(string.Format(k3BillFormat, 29, -1), 11));//其他出库单（红字）->X9为其他入库单
            //K3TransTypeToX9BillType.Add(new KeyValuePair<string, int>(string.Format(k3BillFormat, 10, 1), 12));//其他入库单→其他入库申请单
            //K3TransTypeToX9BillType.Add(new KeyValuePair<string, int>(string.Format(k3BillFormat, 10, -1), 13));//红字其他入库单→X9为其他出库单

            //K3TransTypeToX9BillType.Add(new KeyValuePair<string, int>(string.Format(k3BillFormat, 41, 1), 19));//调拨单 →调拨申请
            //MessageBox.Show(AppDomain.CurrentDomain.BaseDirectory);
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(CommonFunc.strConfig);

            XmlNodeList lstNode = xmlDoc.GetElementsByTagName("K3TransTypeToX9BillType");
            foreach (XmlNode item in lstNode)
            {
                string keyFormat = string.Format(K3TypeFormat, item.Attributes[0].Value, item.Attributes[1].Value);
                K3TransTypeToX9BillType.Add(new KeyValuePair<string, int>(keyFormat, Convert.ToInt32(item.FirstChild.InnerText)));
            }

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

        public static string ConvertByEventID(long eventId)
        {
            string eventName = String.Empty;
            switch (eventId)
            {
                case 200001:
                    eventName = "审核前事件";
                    break;
                case 200002:
                    eventName = "审核反写事件";
                    break;
                case 200003:
                    eventName = "审核退出前事件";
                    break;
                case 300007:
                    eventName = "关闭前事件";
                    break;
                case 300008:
                    eventName = "关闭后事件";
                    break;
                case 300015:
                    eventName = "行关闭前事件";
                    break;
                case 300016:
                    eventName = "反行关闭后事件";
                    break;
                default:
                    break;
            }
            return eventName;
        }

        public static int ContrastK3TransType(long _transtype, int _rob)
        {
            string k3BillKey = string.Format(K3TypeFormat, _transtype, _rob);
            if (K3TransTypeToX9BillType.Exists(k => k.Key == k3BillKey))
            {
                return K3TransTypeToX9BillType.Find(t => t.Key == k3BillKey).Value;
            }
            return 0;
        }

    }
}
