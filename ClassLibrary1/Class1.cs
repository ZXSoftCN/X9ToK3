using System;
using System.Collections.Generic;
using System.Text;
using System.Data.SqlClient;
using System.Data;
using System.Windows.Forms;

namespace ClassLibrary1
{
    public class Class1
    {
        public int func(int _i)
        {
            string sqlConn = "User ID=sa;Password=as;Data Source=KINGYU-9EB62423;Initial Catalog=AIS20101126160333";
            string strViewXml = string.Empty;
            using (SqlConnection sqlconn = new SqlConnection(sqlConn))
            {
                sqlconn.Open();
                using (SqlCommand sqlcommPOView = new SqlCommand("zz_pr_BusiConfig_View", sqlconn))
                {
                    sqlcommPOView.CommandType = CommandType.StoredProcedure;

                    SqlParameter sqlparaResult = new SqlParameter("@Infos", SqlDbType.Xml, 5000);
                    sqlparaResult.Direction = ParameterDirection.Output;
                    sqlcommPOView.Parameters.Add(sqlparaResult);

                    sqlcommPOView.ExecuteNonQuery();
                    strViewXml = sqlparaResult.Value.ToString();
                    //Debug.WriteLine(lst3.Count());
                }
            }
            
            return 1;
        }

        //进行regasm注册时，必须至少有一个公共类的缺省构造函数可见（不能被私有化屏蔽）.
        //private Class1() { }
    }
}
