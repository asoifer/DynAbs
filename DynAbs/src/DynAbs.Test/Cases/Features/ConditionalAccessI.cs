using System;
using System.Collections.Generic;
using System.Text;

namespace DynAbs.Test.Cases
{
    class ConditionalAccessI
    {
        static void Main()
        {
            var a = new A();
            var c1 = new C();
            c1.d = new D();
            var c2 = new C();
            var t1 = a.TestMethod(c1);
            var t2 = a.TestMethod(c2);
            var x = t1 + t2;
            return;
        }

        class A
        {
            public int TestMethod(C receiver)
            {
                if (receiver.d?.BoolTestMethod() != true)
                {
                    return 0;
                }
                return 1;
            }
        }

        class C
        {
            public D d { get; set; }
        }

        class D
        {
            public bool BoolTestMethod()
            {
                return false;
            }
        }
    }
}
