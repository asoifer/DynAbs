using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynAbs.Test.Cases.Features
{
    class SpecialRegionA
    {
        static void Main()
        {
            TestMethod(
                "VAR1",
#if PEPITO
                "VAR3,
#endif
                "VAR2"
                );

            return;
        }

        static void TestMethod(string p1, string p2)
        {

        }
    }
}
