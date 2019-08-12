using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace K3ToX9SqlCRL
{
    /// <summary>
    /// K3同X9系统交互配置表实体。
    /// 通过查找检索的Xml结果进行反序列化。
    /// </summary>
    [XmlRoot("K3InterceptConfig", Namespace = "", IsNullable = true)]
    public class K3InterceptConfig
    {
        [XmlAttribute]
        public int Id { get; set; }

        [XmlAttribute]
        public int X9BusinessType { get; set; }//X9业务类型，从配置文件config中查询对应到K3业务类型

        [XmlAttribute]
        public string InterceptEvent { get; set; }//拦截事件名

        [XmlAttribute]
        public int IsEnable { get; set; }//是否启用

        [XmlAttribute]
        public string ServiceAddress { get; set; }//服务地址

        [XmlAttribute]
        public string ServiceMethod { get; set; }//服务方法名

        [XmlAttribute]
        public string Notes { get; set; }//描述

        [XmlAttribute]
        public DateTime EnableDate { get; set; }//生效日期

        [XmlAttribute]
        public DateTime DisableDate { get; set; }//失效日期
    }
}
