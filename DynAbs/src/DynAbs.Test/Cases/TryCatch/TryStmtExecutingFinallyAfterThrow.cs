using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynAbs.Test.Cases
{
    class TryStmtExecutingFinallyAfterThrow
    {
        public static void Main(string[] args)
        {
            var a = new A();
            try
            {
                Func(a);
            }
            catch (Exception e)
            {
                a.s = a.s + e.ToString();
            }
            string y = a.s;
        }

        private static void Func(A a)
        {
            try
            {
                throw new Exception(" <= Exception! => ");
            }
            catch (Exception e)
            {
                a.s = e.Message;
                throw e;
            }
            finally
            {
                a.s = a.s + " --- Finally --- ";
            }
        }

        public class A
        {
            public string s = null;
        }
    }
}
