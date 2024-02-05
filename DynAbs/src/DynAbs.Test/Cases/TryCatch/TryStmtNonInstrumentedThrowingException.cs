using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynAbs.Test.Cases
{
    class TryStmtNonInstrumentedThrowingException
    {
        public static void Main(string[] args)
        {
            string s1 = "file: ";
            try
            {
                s1 = Func();
            }
            catch
            {
                s1 = s1 + "NO_FILE";
            }
            string s2 = s1;
        }

        private static string Func()
        {
            var file = File.OpenRead(@"C:/temp/non_existing_file.txt");
            var a = file.ToString();
            return a;
        }
    }
}
