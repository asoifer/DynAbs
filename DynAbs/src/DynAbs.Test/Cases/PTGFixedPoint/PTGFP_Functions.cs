using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DynAbs.Test.Util;

namespace DynAbs.Test
{
    class PTGFP_Functions
    {
        static int total = 0;
        static void Main()
        {
            var l = new SimpleList<A>();
            for (var i = 0; i < 4; i++)
            { 
                m(l);
                m(l);
            }
            var pepe = total;
            return;
        }

        static void m(SimpleList<A> l)
        {
            var c = 0;
            for (var i = 0; i < 10; i++)
            {
                l.Add(new A());
                for (var j = 0; j < 10; j++)
                {
                    if (i < j)
                    {
                        var temp = new B() { a = l.GetAt(i) };
                        c += temp.a.f;
                    }
                    else
                        c += new A().f;

                }
            }
            var t = c;
            total += t;
        }

        class A
        {
            public int f = 1;
        }

        class B : A
        {
            public A a = new A();
        }
    }
}
