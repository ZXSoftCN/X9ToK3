﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Xml.Serialization;

namespace SqlCLRTest
{
    public class ErrorItem
    {        
        [XmlAttribute]
        public string ErrorText { get; set; }


        
    }
}
