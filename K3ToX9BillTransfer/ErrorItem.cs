using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Xml.Serialization;

namespace K3ToX9BillTransfer
{
    public class ErrorItem
    {        
        [XmlAttribute]
        public string ErrorText { get; set; }


        
    }
}
