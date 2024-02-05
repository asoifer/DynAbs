using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynAbs.Test.Cases
{
    class SimpleCallNestedConstructor
    {
        static void Main(string[] args)
        {
            int y = 3;
            int x = sum(new A(), y);
            int z = x + y;
        }

        private static int sum(A a, int y)
        {
            int x = a.Some;
            return x + y;
        }

        public class A
        {
            public int Some { get; set; }
            public A()
            {
                Some = 1;
            }
        }
    }
}
