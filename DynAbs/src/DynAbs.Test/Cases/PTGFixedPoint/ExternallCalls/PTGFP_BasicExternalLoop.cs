using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynAbs.Test.Cases
{
    class PTGFP_BasicExternalLoop
    {
        static void Main()
        {
            var enumResult = Enumerable.Repeat(1, 10).Select(x => new A()).ToList();
            return;
        }

        class A
        {

        }
    }
}
