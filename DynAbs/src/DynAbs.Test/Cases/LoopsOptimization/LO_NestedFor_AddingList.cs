using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynAbs.Test
{
    class LO_NestedFor_AddingList
    {
        static void Main()
        {
            var l = new List<A>();
            for (var i = 0; i < 10; i++)
                for (var j = 0; j < 10; j++)
                    l.Add(new A(i+j));
            var r = l.First().v;
            return;
        }

        class A
        {
            public int v;
            public A(int val)
            {
                v = val;
            }
        }
    }
}
