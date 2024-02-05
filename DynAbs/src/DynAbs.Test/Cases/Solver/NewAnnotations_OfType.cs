using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynAbs.Test.Cases.Solver
{
    class NewAnnotations_OfType
    {
        static void Main()
        {
            var a1 = new A() { i = 1 };
            var b1 = new B() { j = 1 };
            a1.b = b1;

            var l = new List<A>();
            l.Add(a1);
            var retA1 = l[0];
            Console.WriteLine(retA1.i);
        }

        class A
        {
            public int i { get; set; }
            public B b { get; set; }
        }

        class B
        {
            public int j { get; set; }
        }
    }
}
