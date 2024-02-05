using System;

namespace DynAbs.Test.Cases.Features
{
    class LocalFunctions_RepeatedNames
    {
        static void Main()
        {
            var c1 = C1();
            var c2 = C2();
            var r = c1 + c2;
            return;
        }

        static int C1()
        {
            var r = L1();
            return r;

            static int L1()
            {
                return 0;
            }
        }

        static int C2()
        {
            var r = L1();
            return r;

            static int L1()
            {
                return 0;
            }
        }
    }
}
