using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynAbs.Test.Cases.Roslyn
{
    class CallWithRef
    {
        public static void Main(string[] args)
        {
            int val = 1;
            Method(ref val);
            Console.WriteLine(val);
            //output = 45
        }


        static void Method(ref int i)
        {
            i = i + 44;
        }
    }
}
