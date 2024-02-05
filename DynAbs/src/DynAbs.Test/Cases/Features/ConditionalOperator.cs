using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynAbs.Test.Cases
{
    class ConditionalOperator
    {
        public static void Main(string[] args)
        {
            int classify;
            int input = 10;
            int x = 1;
            int y = 2;
            classify = (input > 0) ? x : y;
            return;
        }
    }
}
