using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Xml.Serialization;

namespace K3ToX9BillTransfer
{
    [XmlRoot("ResultInfo", Namespace = "", IsNullable = true)]
    public class ResultInfo
    {
        [XmlAttribute]
        public bool IsSuccess { get; set; }

        [XmlAttribute]
        public bool IsReWrite { get; set; }

        [XmlArrayItem("ResultItem")]
        public List<ResultItem> Results { get; set; }

        [XmlArrayItem("ErrorItem")]
        public List<ErrorItem> Errors { get; set; }
        
        public ResultInfo()
        {
            IsSuccess = true;
            Results = new List<ResultItem>();
            Errors = new List<ErrorItem>();
        }
    }
}
