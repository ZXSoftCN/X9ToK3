﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace K3ToX9BillTransfer
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
            //获取运行程序的路径。
            //【作废】避免在singleton中日志文件名一直使用初次加载时日期名，而改用在WriteInfoToLogFile中获取最新的日志。
            //getNewLogName();
        }

        private void refreshLogName()
        {
            string logFileName = (DateTime.Now.Year).ToString() + '-'
                + (DateTime.Now.Month).ToString() + '-' + (DateTime.Now.Day).ToString() + "_Log.log";
            string logFilePath = CommonFunc.strPath + "logFile\\";
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
            refreshLogName();
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

        public static void logOnlyDebug(string loginfo)
        {
            if (CommonFunc.ConfigLogType >= LOG_TYPE.LOG_DEBUG)
            {
                Log(loginfo, CommonFunc.ConfigLogType);
            }
        }

        /// <summary>
        /// 日志级别高于等于LOG_DEBUG，进行日志记录
        /// </summary>
        public static void debugLog(string interceptEvent, K3DataParaInfo docInfo, string msg)
        {
            if (CommonFunc.ConfigLogType >= LOG_TYPE.LOG_DEBUG)
            {
                string strPreLog = String.Format("-----------日志消息：{0}---------------" + Environment.NewLine
                + "\tK3传递数据内容：X9业务类型：【{1}】；K3业务类型【{2}】；红蓝字【{3}】；触发事件【{4}】；单据内码【{5}】；明细行ID【{6}】；单据编码【{7}】；额外数据【{8}】；操作人【{9}】",
                msg,docInfo.X9BillType.ToString(), docInfo.TransType.ToString(), docInfo.ROB.ToString(), interceptEvent, docInfo.InterID.ToString(), docInfo.EntryID.ToString(), 
                docInfo.BillCode, docInfo.Data, docInfo.CurrentUser);

                Log(strPreLog, CommonFunc.ConfigLogType);
            }
        }

        /// <summary>
        /// 日志级别高于等于LOG_INFO，进行日志记录
        /// </summary>
        public static void infoLog(string interceptEvent, K3DataParaInfo docInfo, string msg)
        {
            if (CommonFunc.ConfigLogType >= LOG_TYPE.LOG_INFO)
            {
                string strPreLog = String.Format("-----------日志消息：{0}---------------" + Environment.NewLine
                + "\tK3传递数据内容：X9业务类型：【{1}】；K3业务类型【{2}】；红蓝字【{3}】；触发事件【{4}】；单据内码【{5}】；明细行ID【{6}】；单据编码【{7}】；额外数据【{8}】；操作人【{9}】",
                msg, docInfo.X9BillType.ToString(), docInfo.TransType.ToString(), docInfo.ROB.ToString(), interceptEvent, docInfo.InterID.ToString(), docInfo.EntryID.ToString(),
                docInfo.BillCode, docInfo.Data, docInfo.CurrentUser);

                Log(strPreLog, CommonFunc.ConfigLogType);
            }
        }

    }
}