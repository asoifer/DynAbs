using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynAbs.Test.Cases
{
    class TryStmtBindNonInsThrowedExcep
    {
        public static void Main(string[] args)
        {
            string s = "exp: ";
            try
            {
                s = Fun1();
            }
            catch(Exception e)
            {
                s = e.Message.ToString();
            }
            string z = s;
        }

        private static string Fun1()
        {
            return Func();
        }

        private static string Func()
        {
            var file = File.OpenRead(@"C:/temp/non_existing_file.txt");
            var a = file.ToString();
            return a;
        }
    }
}
