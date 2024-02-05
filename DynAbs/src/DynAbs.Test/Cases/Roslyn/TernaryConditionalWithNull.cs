using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynAbs.Test.Cases.Roslyn
{
    class TernaryConditionalWithNull
    {
        public static void Main(string[] args)
        {
            int input = 5;
            int? b = 2;
            var classify = (input > 0) ? null : b;
        }
    }
}
