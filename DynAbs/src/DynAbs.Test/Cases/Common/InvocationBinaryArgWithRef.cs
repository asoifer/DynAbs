using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynAbs.Test.Cases
{
    class InvocationBinaryArgWithRef
    {
        public static void Main(string[] args)
        {
            int x = 3;
            int y = 4;
            int z = Func(ref x, y);
        }
        public static int Func(ref int x, int y)
        {
            int z = Func2(x + y);
            return z;
        }
        public static int Func2(int x)
        {
            return x;
        }
    }
}
