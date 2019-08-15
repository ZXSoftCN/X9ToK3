using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace K3ToX9BillTransfer
{
    interface IK3Intercept
    {
        /// <summary>
        /// K3单据保存时调用。
        /// 注意：K3单据修改（即更新保存)时，从K3中间层消息中会发出两条指令。一个是删除原数据，另一个是保存新记录
        /// </summary>
        /// <param name="k3Connection">K3数据库连接串</param>
        /// 
        /// <param name="tranType">K3业务类型</param>
        /// <param name="rob">K3红蓝字：1是蓝字、-1是红字</param>
        /// <param name="interID">K3单据内码</param>
        /// <param name="billCode">单据编号</param>
        /// <param name="data">发生业务时指令内容</param>
        /// <param name="rltFlag">是否允许K3继续执行的结果。true则继续，false则中断。</param>
        bool addBefore(K3DataParaInfo docInfo);
        bool addAfter(K3DataParaInfo docInfo);
        bool deleteBefore(K3DataParaInfo docInfo);
        bool deleteAfter(K3DataParaInfo docInfo);
        bool approvedBefore(K3DataParaInfo docInfo);
        bool approvedAfter(K3DataParaInfo docInfo);
        bool unApprovedBefore(K3DataParaInfo docInfo);
        bool unApprovedAfter(K3DataParaInfo docInfo);

        bool closedBefore(K3DataParaInfo docInfo);
        bool closedAfter(K3DataParaInfo docInfo);
        bool unClosedBefore(K3DataParaInfo docInfo);
        bool unClosedAfter(K3DataParaInfo docInfo);
        bool entryClosedBefore(K3DataParaInfo docInfo);
        bool entryClosedAfter(K3DataParaInfo docInfo);
        bool unEntryClosedBefore(K3DataParaInfo docInfo);
        bool unEntryClosedAfter(K3DataParaInfo docInfo);

        bool unKnownEvent(K3DataParaInfo docInfo);
    }
}
