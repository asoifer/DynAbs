using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynAbs.Test.Cases
{
    class CondAttribute
    {
        
        public static void Main(string[] args)
        {
            int var = 0;
            Method1(4);
            var = var + 1;
            return;
        }

        [Conditional("FALSE")]
        public static void Method1(int x)
        {
            var = 10;
            Console.WriteLine("CONDITION1 is defined");
        }

        public static int var { get; set; }
    }
}
