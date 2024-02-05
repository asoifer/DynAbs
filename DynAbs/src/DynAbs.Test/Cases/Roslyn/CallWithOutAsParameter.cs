using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynAbs.Test.Cases.Roslyn
{
    class CallWithOutAsParameter
    {
        public static void Main(string[] args)
        {
            int value = 2;
            MyMethod(out value);
            Console.WriteLine(value);
            // value is now 44
        }

        static void MyMethod(out int i)
        {
            MyMethod2(out i);
        }

        static void MyMethod2(out int i)
        {
            i = 33;
        }
    }
}
