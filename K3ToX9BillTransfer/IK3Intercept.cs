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
        ResultInfo addBefore(K3DataParaInfo docInfo);
        ResultInfo addAfter(K3DataParaInfo docInfo);
        ResultInfo deleteBefore(K3DataParaInfo docInfo);
        ResultInfo deleteAfter(K3DataParaInfo docInfo);

        ResultInfo firstApprovedBefore(K3DataParaInfo docInfo);
        ResultInfo firstApprovedAfter(K3DataParaInfo docInfo);
        ResultInfo unFirstApprovedBefore(K3DataParaInfo docInfo);
        ResultInfo unFirstApprovedAfter(K3DataParaInfo docInfo);
        ResultInfo approvedBefore(K3DataParaInfo docInfo);
        ResultInfo approvedAfter(K3DataParaInfo docInfo);
        ResultInfo unApprovedBefore(K3DataParaInfo docInfo);
        ResultInfo unApprovedAfter(K3DataParaInfo docInfo);

        ResultInfo closedBefore(K3DataParaInfo docInfo);
        ResultInfo closedAfter(K3DataParaInfo docInfo);
        ResultInfo unClosedBefore(K3DataParaInfo docInfo);
        ResultInfo unClosedAfter(K3DataParaInfo docInfo);
        ResultInfo entryClosedBefore(K3DataParaInfo docInfo);
        ResultInfo entryClosedAfter(K3DataParaInfo docInfo);
        ResultInfo unEntryClosedBefore(K3DataParaInfo docInfo);
        ResultInfo unEntryClosedAfter(K3DataParaInfo docInfo);

        ResultInfo unKnownEvent(K3DataParaInfo docInfo);
    }
}
