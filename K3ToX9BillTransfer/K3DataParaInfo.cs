using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace K3ToX9BillTransfer
{
    public class K3DataParaInfo
    {
        /// <summary>
        /// 单据编号
        /// </summary>
        [XmlAttribute]
        public string BillCode { get; set; }

        /// <summary>
        /// 单据内码(主键）
        /// </summary>
        [XmlAttribute]
        public long InterID { get; set; }

        /// <summary>
        /// 单据明细行内码。在行关闭或行反关闭时会被赋值，其它动作下:EntryID=0.
        /// </summary>
        [XmlAttribute]
        public int EntryID { get; set; }

        /// <summary>
        /// 业务类型:通过K3数据表IcTranstype检索具体的业务类型。
        /// </summary>
        [XmlAttribute]
        public long TransType { get; set; }

        /// <summary>
        /// 红蓝字：1为蓝字，0为红字
        /// </summary>
        [XmlAttribute]
        public int ROB { get; set; }

        /// <summary>
        /// 当前操作人
        /// </summary>
        [XmlAttribute]
        public string CurrentUser { get; set; }

        /// <summary>
        /// 事件名称
        /// </summary>
        [XmlAttribute]
        public string EventName { get; set; }

        /// <summary>
        /// 预留字段。后续若要传入单据周边的其它信息，可以由它传入。
        /// </summary>
        public string Data { get; set; }

        /// <summary>
        /// X9系统单据类型：枚举值。
        /// </summary>
        [XmlAttribute]
        public int X9BillType { get; set; }
    }
}
