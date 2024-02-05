using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DynAbs.Test.Cases
{
    class PTGFP_ExternalLoopInInternalLoop
    {
        static void Main()
        {
            var l = new List<A>();
            for(var i = 0; i < 3; i++)
                l = Enumerable.Repeat(1, 3).Select(x => new A()).ToList();

            return;
        }

        class A
        {

        }
    }
}
