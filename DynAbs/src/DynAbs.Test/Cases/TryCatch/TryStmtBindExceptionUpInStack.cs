using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynAbs.Test.Cases
{
    class TryStmtBindExceptionUpInStack
    {
        public static void Main(string[] args)
        {
            string s = "msj: ";
            try
            {
                Func1();
            }
            catch (Exception e)
            {
                s = s + e.ToString();
            }
            string x = s;
        }

        private static void Func1()
        {
            Func2();
        }

        private static void Func2()
        {
            throw new Exception("Implementame amigo!");
        }
    }
}
