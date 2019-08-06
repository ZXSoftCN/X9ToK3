using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace K3ToX9BillTransfer
{
    public class RankException : Exception
    {
        public int Rank { get; set; }

        public RankException() : this(1)
        {
        }

        public RankException(int rank)
            : base()
        {
            Rank = rank;
        }

        public RankException(string msg, Exception innerExp) : this(1, msg, innerExp) 
        {
            if (innerExp.GetType() == typeof(RankException))
            {
                RecurInner(innerExp as RankException);
            }
            
        }

        public RankException(int rank,string msg,Exception innerExp):base(msg,innerExp)
        {
            Rank = rank;
        }
        public RankException(string msg, RankException innerExp) : this(innerExp.Rank,msg,innerExp)
        {
            RecurInner(innerExp);
        }

        /// <summary>
        /// 将内部异常的等级递进。
        /// </summary>
        /// <param name="rankExcep"></param>
        /// <returns><\returns>
        public static RankException RecurInner(RankException rankExcep)
        {
            rankExcep.Rank++;
            if (rankExcep.InnerException.GetType() == typeof(RankException))
            {
                RankException excep = rankExcep.InnerException as RankException;
                return RecurInner(excep);
            }
            else
            {
                return rankExcep;
            }
        }
        
        public string MessageTab
        {
            get
            {
                string strRankTab = "\t";
                for (int i = 0; i < Rank; i++)
                {
                    strRankTab += "\t";
                }
                return string.Format("\n\r{0}{1}", strRankTab, base.Message);
            }
        }

    }
}
