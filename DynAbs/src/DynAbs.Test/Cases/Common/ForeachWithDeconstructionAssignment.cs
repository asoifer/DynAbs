using System;
using System.Collections.Generic;
using System.Text;

namespace DynAbs.Test.Cases.Common
{
    class ForeachWithDeconstructionAssignment
    {
        static void Main()
        {
            var dictionary = new Dictionary<A, B>();
            dictionary.Add(new A() { i = 1 }, new B() { j = 2 });
            var total = 0;
            foreach (var (k, v) in dictionary)
            {
                total += k.i;
            }
            var res = total;
            return;
        }

        class A
        {
            public int i { get; set; }
        }

        class B
        {
            public int j { get; set; }
        }
    }
}
