using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynAbs.Test.Cases
{
    class PTGFP_Basic_ExternalList
    {
        static void Main()
        {
            var l = new List<A>();
            for (var i = 0; i < 4; i++)
                l.Add(new A());
            var t = l[0].f;
            return;
        }

        class A
        {
            public int f = 1;
        }
    }
}
