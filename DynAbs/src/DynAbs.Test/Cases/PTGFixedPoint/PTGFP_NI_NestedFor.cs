using ExternalLibraryExample;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using DynAbs.Test.Util;

namespace DynAbs.Test
{
    class PTGFP_NI_NestedFor
    {
        static void Main()
        {
            var l = new SimpleList<A>();
            var c = 0;
            for (var i = 0; i < 10; i++)
            {
                l.Add(new A());
                for (var j = 0; j < 10; j++)
                {
                    var temp = new B() { a = l.GetAt(i) };
                    c += temp.a.f;
                }
            }
            var t = c;
            return;
        }

        class A
        {
            public int f = 1;
        }

        class B : BasicClass
        {
            public A a = new A();
        }
    }
}
