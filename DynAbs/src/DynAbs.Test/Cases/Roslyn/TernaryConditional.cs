using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynAbs.Test.Cases.Roslyn
{
    class TernaryConditional
    {
        public static void Main(string[] args)
        {
            int input = 5;
            int a = 1;
            int b = 2;
            int classify = (input > 0) ? a : b;
        }
    }
}
