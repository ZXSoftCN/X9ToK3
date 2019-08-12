using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Xml.Serialization;

namespace K3ToX9SqlCRL
{
    public class ResultItem
    {        
        [XmlAttribute]
        public string MsgText { get; set; }
        
    }
}
