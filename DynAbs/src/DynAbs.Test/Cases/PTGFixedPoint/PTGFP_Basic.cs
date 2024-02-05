using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DynAbs.Test.Util;

namespace DynAbs.Test
{
    class PTGFP_Basic
    {
        static void Main()
        {
            var l = new SimpleList<A>();
            for (var i = 0; i < 10; i++)
                l.Add(new DynAbs.Test.PTGFP_Basic.A());
            var t = l.GetAt(0).f;
            return;
        }

        class A
        {
            public int f = 1;
        }
    }
}
