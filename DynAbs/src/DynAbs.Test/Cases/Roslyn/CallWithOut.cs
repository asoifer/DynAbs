using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynAbs.Test.Cases.Roslyn
{
    class CallWithOut
    {
        public static void Main(string[] args)
        {
            int value = 2;
            Method(out value);
            Console.WriteLine(value);
            // value is now 44
        }

        static void Method(out int i)
        {
            i = 44;
        }
    }
}
