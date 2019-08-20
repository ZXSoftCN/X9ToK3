using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using K3ToX9BillTransfer;
using System.Diagnostics;

namespace TestConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            InterceptHandler handler = new InterceptHandler();
            bool rlt = false;
            string rltMsg = string.Empty;
            string strDestAcctConnect = string.Format("Data Source= {0};Initial Catalog={1};User ID={2};Password={3}", "LEOWORKCENTRE", "U9V30", "sa", "as");

            string strDest = "User ID=sa;Password=as;Data Source=KINGYU-9EB62423;Initial Catalog=AIS20101126160333";
            handler.handle(strDestAcctConnect, 10, 1, 1, 200001, 1, 0, "PR0001", "morningStar", "------fadsfadsf", ref rlt, ref rltMsg);
            //Debug.Assert(false,rlt.ToString());
            Debug.WriteLine(rlt.ToString());
        }
    }
}
