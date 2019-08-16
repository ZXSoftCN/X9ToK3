using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace K3ToX9SqlCRL
{
    /// <summary>
    /// 作事件响应常量类使用
    /// </summary>
    class InterceptEvent
    {
        public const string AddBefore = "AddBefore";//新增前
        public const string AddAfter = "AddAfter";//新增后
        public const string DeleteBefore = "DeleteBefore";//删除前
        public const string DeleteAfter = "DeleteAfter";//删除后

        public const string FirstApprovedBefore = "FirstApprovedBefore";//一级审核前
        public const string FirstApprovedAfter = "FirstApprovedAfter";//一级审核后
        public const string UnFirstApprovedBefore = "UnFirstApprovedBefore";//一级弃审前
        public const string UnFirstApprovedAfter = "UnFirstApprovedAfter";//一级弃审后

        public const string ApprovedBefore = "ApprovedBefore";//审核前
        public const string ApprovedAfter = "ApprovedAfter";//审核后
        public const string UnApprovedBefore = "UnApprovedBefore";//弃审前
        public const string UnApprovedAfter = "UnApprovedAfter";//弃审后

        public const string ClosedBefore = "ClosedBefore";//关闭前
        public const string ClosedAfter = "ClosedAfter";//关闭后
        public const string UnClosedBefore = "UnClosedBefore";//反关闭前
        public const string UnClosedAfter = "UnClosedAfter";//反关闭后

        public const string EntryClosedBefore = "EntryClosedBefore";//行关闭前
        public const string EntryClosedAfter = "EntryClosedAfter";//行关闭后
        public const string UnEntryClosedBefore = "UnEntryClosedBefore";//反行关闭前
        public const string UnEntryClosedAfter = "UnEntryClosedAfter";//反行关闭后

        public const string UnKnownEvent = "UnKnownEvent";//未知事件

        private InterceptEvent() { }

        public static string ConvertToEventName(long eventID,long operateID)
        {
            if (operateID == 4)
            {
                //新增
                if (eventID == 200001)
                {
                    return AddBefore;
                }
                else if (eventID == 200003)
                {
                    return AddAfter;
                }
                else
                {
                    return UnKnownEvent;
                }
            }
            else if (operateID == 8)
            {
                //删除
                if (eventID == 200001)
                {
                    return DeleteBefore;
                }
                else if (eventID == 200003)
                {
                    return DeleteAfter;
                }
                else
                {
                    return UnKnownEvent;
                }
            }
            else if (operateID == 1)
            {
                //审核
                if (eventID == 200001)
                {
                    return ApprovedBefore;
                }
                else if (eventID == 200003)
                {
                    return ApprovedAfter;
                }
                else
                {
                    return UnKnownEvent;
                }
            }
            else if (operateID == 2)
            {
                //反审核
                if (eventID == 200001)
                {
                    return UnApprovedBefore;
                }
                else if (eventID == 200003)
                {
                    return UnApprovedAfter;
                }
                else
                {
                    return UnKnownEvent;
                }
            }
            else if (operateID == 64)
            {
                //关闭
                if (eventID == 300007)
                {
                    return ClosedBefore;
                }
                else if (eventID == 300008)
                {
                    return ClosedAfter;
                }
                else if (eventID == 300015)
                {
                    //行关闭
                    return EntryClosedBefore;
                }
                else if (eventID == 300016)
                {
                    return EntryClosedAfter;
                }
                else
                {
                    return UnKnownEvent;
                }
            }
            else if (operateID == 128)
            {
                //反关闭
                if (eventID == 300007)
                {
                    return UnClosedBefore;
                }
                else if (eventID == 300008)
                {
                    return UnClosedAfter;
                }
                else if (eventID == 300015)
                {
                    //反行关闭
                    return UnEntryClosedBefore;
                }
                else if (eventID == 300016)
                {
                    return UnEntryClosedAfter;
                }
                else
                {
                    return UnKnownEvent;
                }
            }
            else
            {
                return UnKnownEvent;
            }
        }

        public static string ConvertToCNZHName(string eventName)
        {
            string eventName_CNZH = string.Empty;
            switch (eventName)
            {
                case AddBefore:
                    eventName_CNZH = "新增前";
                    break;
                case AddAfter:
                    eventName_CNZH = "新增后";
                    break;
                case DeleteBefore:
                    eventName_CNZH = "删除前";
                    break;
                case DeleteAfter:
                    eventName_CNZH = "删除后";
                    break;
                case FirstApprovedBefore:
                    eventName_CNZH = "一级审核前";
                    break;
                case FirstApprovedAfter:
                    eventName_CNZH = "一级审核后";
                    break;
                case UnFirstApprovedBefore:
                    eventName_CNZH = "一级弃审前";
                    break;
                case UnFirstApprovedAfter:
                    eventName_CNZH = "一级弃审后";
                    break;
                case ApprovedBefore:
                    eventName_CNZH = "审核前";
                    break;
                case ApprovedAfter:
                    eventName_CNZH = "审核后";
                    break;
                case UnApprovedBefore:
                    eventName_CNZH = "弃审前";
                    break;
                case UnApprovedAfter:
                    eventName_CNZH = "弃审后";
                    break;
                case EntryClosedBefore:
                    eventName_CNZH = "行关闭前";
                    break;
                case EntryClosedAfter:
                    eventName_CNZH = "行关闭后";
                    break;
                case UnEntryClosedBefore:
                    eventName_CNZH = "反行关闭前";
                    break;
                case UnEntryClosedAfter:
                    eventName_CNZH = "反行关闭后";
                    break;
                case ClosedBefore:
                    eventName_CNZH = "关闭前";
                    break;
                case ClosedAfter:
                    eventName_CNZH = "关闭后";
                    break;
                case UnClosedBefore:
                    eventName_CNZH = "反关闭前";
                    break;
                case UnClosedAfter:
                    eventName_CNZH = "反关闭后";
                    break;
                default:
                    eventName_CNZH = "未知事件";
                    break;
            }
            return eventName_CNZH;
        }
    }
}
