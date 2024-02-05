using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynAbs.Test.Cases
{
    class ScalarDoubleUse
    {
        public static void Main(string[] args)
        {
            int y = 42;
            int x = y + y + y;
            int z = x;
        }
    }
}
