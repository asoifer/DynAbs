using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynAbs.Test.Cases
{
    class SimpleDo
    {
        public static void Main()
        {
            var a = 1;
            do
            {
                int b = 3; a += b;
            } while (a < 10);
            var c = a;
            return;
        }
    }
}
