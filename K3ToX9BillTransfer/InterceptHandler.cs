using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using K3ToX9BillTransfer.UI;
using System.Windows.Forms;
using System.Data.SqlClient;
using System.Data;

namespace K3ToX9BillTransfer
{
    /// <summary>
    /// 根据K3中间层消息EventID等，判断调用哪种事件方法响应
    /// </summary>
    public class InterceptHandler
    {
        static string strConfig = AppDomain.CurrentDomain.BaseDirectory + "config.xml";

        /// <summary>
        /// 
        /// </summary>
        /// <param name="k3Connection"></param>
        /// <param name="transType"></param>
        /// <param name="rob"></param>
        /// <param name="operateID"></param>
        /// <param name="eventID"></param>
        /// <param name="interID"></param>
        /// <param name="billCode"></param>
        /// <param name="currUser"></param>
        /// <param name="data"></param>
        /// <param name="rltFlag">特别注意为false时，也就是执行失败时外部分布式事务会回滚所有的数据更新处理。</param>
        public void handle(string k3Connection, int transType, int rob,long operateID, long eventID, long interID, string billCode,string currUser, string data, ref bool rltFlag)
        {
            bool bInnerRlt = true;
            //LogInfoHelp.Log(k3Connection, LOG_TYPE.LOG_DEBUG);
            K3DataParaInfo docInfo = new K3DataParaInfo()
            {
                BillCode = billCode,
                InterID = interID,
                TransType = transType,
                ROB = rob,
                CurrentUser = currUser,
                X9BillType = CommonFunc.ContrastK3TransType(Convert.ToInt64(transType),rob),
                EventName = InterceptEvent.ConvertToEventName(eventID, operateID),
                Data = data,
            };
            LogInfoHelp.debugLog(InterceptEvent.ConvertToEventName(eventID, operateID), docInfo, "进入事件分发响应处理");
            try
            {
                if (ServiceConfig.Initial(k3Connection))
                {
                    LogInfoHelp.debugLog(InterceptEvent.ConvertToEventName(eventID, operateID),
                        docInfo, string.Format("X9对K3中间层拦截器初始化完成，进入事件{0}响应。", InterceptEvent.ConvertToEventName(eventID, operateID)));
                    IK3Intercept processor = new X9BusinessIntercept();//以标准类实现
                    ///     200001 '审核前事件
                    ///     200002 '审核反写事件
                    ///     200003 '审核退出前事件
                    switch (InterceptEvent.ConvertToEventName(eventID,operateID))
                    {
                        case InterceptEvent.AddBefore:
                            bInnerRlt = processor.addBefore(docInfo);
                            break;
                        case InterceptEvent.AddAfter:
                            bInnerRlt = processor.addAfter(docInfo);
                            break;
                        case InterceptEvent.DeleteBefore:
                            bInnerRlt = processor.deleteBefore(docInfo);
                            break;
                        case InterceptEvent.DeleteAfter:
                            bInnerRlt = processor.deleteAfter(docInfo);
                            break;
                        case InterceptEvent.ApprovedBefore:
                            bInnerRlt = processor.approvedBefore(docInfo);
                            break;
                        case InterceptEvent.ApprovedAfter:
                            bInnerRlt = processor.approvedAfter(docInfo);
                            break;
                        case InterceptEvent.UnApprovedBefore:
                            bInnerRlt = processor.unApprovedBefore(docInfo);
                            break;
                        case InterceptEvent.UnApprovedAfter:
                            bInnerRlt = processor.unApprovedAfter(docInfo);
                            break;
                        default:
                            bInnerRlt = processor.unKnownEvent(docInfo);
                            break;
                    }
                }
                else
                {
                    LogInfoHelp.infoLog(InterceptEvent.ConvertToEventName(eventID, operateID), docInfo, "X9对K3中间层拦截器初始化失败。请进行Debug查阅！");
                    LogInfoHelp.debugLog(InterceptEvent.ConvertToEventName(eventID, operateID), docInfo,
                        string.Format("X9对K3中间层拦截器初始化失败。{0}传入的K3数据库连接串：{1}{2}当前环境执行路径：{3}{4}",
                        Environment.NewLine,k3Connection,Environment.NewLine,CommonFunc.strPath,Environment.NewLine));
                    if (CommonFunc.ConfigLogType > LOG_TYPE.LOG_INFO)
                    {
                        frmMessageSingle.Show("X9对K3中间层拦截器初始化失败!", 
                            string.Format("{0}传入的K3数据库连接串：{1}{2}当前环境执行路径：{3}{4}",
                        Environment.NewLine, k3Connection, Environment.NewLine, CommonFunc.strPath, Environment.NewLine));
                    }
                }
            }
            catch (Exception ex)
            {
                //异常时将错误信息保存至日志，并弹窗提示，但不中断K3动作，让其继续执行下去。
                //rltFlag = false;
                bInnerRlt = false;
                LogInfoHelp.infoLog(InterceptEvent.ConvertToEventName(eventID, operateID), docInfo,
                    string.Format("X9对K3中间层拦截器执行异常，可将配置文件设置为‘Debug'进行详查定位！{0}异常消息：{1}\t{2}异常对象来源：{3}\t{4}异常堆栈：{5}",
                     Environment.NewLine, ex.Message, Environment.NewLine, ex.Source, Environment.NewLine, ex.StackTrace));
                //LogInfoHelp.debugLog(InterceptEvent.ConvertToEventName(eventID, operateID), docInfo,
                //    string.Format("X9对K3中间层拦截器执行异常。异常消息：{0}{1}异常对象来源：{2}{3}异常堆栈：{4}{5}{6}",
                //    Environment.NewLine, ex.Message, Environment.NewLine, ex.Source,
                //    Environment.NewLine, ex.StackTrace, Environment.NewLine));

                //if (CommonFunc.ConfigLogType > LOG_TYPE.LOG_INFO)
                //{
                    frmMessageSingle.Show("X9对K3中间层拦截器执行异常!",
                        string.Format("X9对K3中间层拦截器执行异常，可将配置文件设置为‘Debug'进行详查定位！{0}异常消息：{1}\t{2}异常对象来源：{3}\t{4}异常堆栈：{5}",
                     Environment.NewLine, ex.Message, Environment.NewLine, ex.Source, Environment.NewLine, ex.StackTrace));
                //}
            }
            finally
            {
                rltFlag = bInnerRlt;//最终将内部调用结果返回给K3中间件插件
                LogInfoHelp.infoLog(InterceptEvent.ConvertToEventName(eventID, operateID), docInfo, string.Format("K3ToX9拦截器执行结束,结果：{0}", rltFlag.ToString()));
            }
        }

    }
}
