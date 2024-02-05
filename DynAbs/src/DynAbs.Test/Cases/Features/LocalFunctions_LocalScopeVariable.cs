using System;

namespace DynAbs.Test.Cases.Features
{
    class LocalFunctions_LocalScopeVariable
    {
        static void Main()
        {
            var tc = new TC();
            var r = tc.C1();
            var x = r;
            return;
        }

        class TC
        {
            public int C1()
            {
                var root = 1;
                var r = L1();
                return r;

                int L1()
                {
                    return root;
                }
            }
        }
    }
}
