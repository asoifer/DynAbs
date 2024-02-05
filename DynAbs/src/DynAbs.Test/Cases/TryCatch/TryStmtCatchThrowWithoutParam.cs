using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynAbs.Test.Cases
{
    class TryStmtCatchThrowWithoutParam
    {
        public static void Main(string[] args)
        {
            string s = "Todo bien!";
            try
            {
                Func();
            }
            catch (Exception e)
            {
                s = e.ToString();
            }
            string x = s;
        }

        private static void Func()
        {
            try
            {
                Func2();
            }
            catch
            {
                throw;
            }
        }

        private static void Func2()
        {
            throw new Exception("Todo mal!");
        }
    }
}
