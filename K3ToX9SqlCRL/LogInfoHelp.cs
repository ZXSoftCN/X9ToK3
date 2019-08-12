using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace K3ToX9SqlCRL
{
    class LogInfoHelp
    {
        private StreamWriter LogFile = null;
        private static LogInfoHelp _instance = null;
        private string LogFilePath = null;

        public static LogInfoHelp GetInstance()
        {
            if (null == _instance)
            {
                _instance = new LogInfoHelp();
            }

            return _instance;
        }
        private LogInfoHelp() {
            CreateLogFile();
        }

        /// <summary>
        /// 创建日志文件
        /// </summary>
        public void CreateLogFile()
        {
            //获取运行程序的路径
            string logFileName = (DateTime.Now.Year).ToString() + '-'
                + (DateTime.Now.Month).ToString() + '-' + (DateTime.Now.Day).ToString() + "_Log.log";
            string logFilePath = SimpleConfig.strPath + "logFile\\";
            if (!Directory.Exists(logFilePath))
            {
                Directory.CreateDirectory(logFilePath);
            }
            this.LogFilePath = logFilePath + logFileName;
        }

        /// <summary>
        /// 信息写入日志
        /// </summary>
        /// <param name="strMsg"></param>
        private void WriteInfoToLogFile(string strLogInfo, LOG_TYPE logType)
        {
            LogFile = new StreamWriter(LogFilePath, true);//文件保存位置
            switch (logType)
            {
                case LOG_TYPE.LOG_DEBUG:
                    {
                        strLogInfo = String.Format("/*************************/" + Environment.NewLine 
                            + "【调试日志】:{0}" + Environment.NewLine 
                            + " {1} ", DateTime.Now.ToString(), strLogInfo);
                    }
                    break;
                default:
                    {
                        strLogInfo = String.Format("/*************************/" + Environment.NewLine 
                        + "【标准日志】:{0}" + Environment.NewLine + " {1} ", DateTime.Now.ToString(), strLogInfo);
                    }
                    break;
            }
            LogFile.WriteLine(strLogInfo);
            LogFile.Close();
        }

        public static void Log(string loginfo, LOG_TYPE logType)
        {
            LogInfoHelp.GetInstance().WriteInfoToLogFile(loginfo, logType);
        }

        /// <summary>
        /// 日志级别高于等于LOG_DEBUG，进行日志记录
        /// </summary>
        public static void debugLog(string interceptEvent, K3DataParaInfo docInfo, string msg)
        {
            if (SimpleConfig.ConfigLogType >= LOG_TYPE.LOG_DEBUG)
            {
                string strPreLog = String.Format("-----------日志消息：{0}---------------" + Environment.NewLine
                + "\tK3传递数据内容：业务类型【{1}】；红蓝字【{2}】；触发事件【{3}】；单据内码【{4}】；单据编码【{5}】；额外数据【{6}】；操作人【{7}】",
                msg, docInfo.TransType.ToString(), docInfo.ROB.ToString(), interceptEvent, docInfo.InterID.ToString(), docInfo.BillCode, docInfo.Data,docInfo.CurrentUser);

                Log(strPreLog, SimpleConfig.ConfigLogType);
            }

        }

        /// <summary>
        /// 日志级别高于等于LOG_INFO，进行日志记录
        /// </summary>
        public static void infoLog(string interceptEvent, K3DataParaInfo docInfo, string msg)
        {
            if (SimpleConfig.ConfigLogType >= LOG_TYPE.LOG_INFO)
            {
                string strPreLog = String.Format("-----------日志消息：{0}---------------" + Environment.NewLine
                + "\tK3传递数据内容：业务类型【{1}】；红蓝字【{2}】；触发事件【{3}】；单据内码【{4}】；单据编码【{5}】；额外数据【{6}】；操作人【{7}】",
                msg,docInfo.TransType.ToString(),docInfo.ROB.ToString(), interceptEvent,docInfo.InterID.ToString(),docInfo.BillCode,docInfo.Data,docInfo.CurrentUser);

                Log(strPreLog, SimpleConfig.ConfigLogType);
            }
        }

    }
}