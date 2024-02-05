using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynAbs.Test.Cases
{
    class DeconstructionAssignment
    {
        static void Main()
        {
            var i = 1;
            var j = 2;
            (var a, var b) = TestMethod(i, j);
            Console.WriteLine(a);
            return;
        }

        static (int A, int B) TestMethod(int i, int j)
        {
            var t = i;
            var s = j;
            return (t, s);
        }
    }
}
