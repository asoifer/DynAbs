using System;
using System.Collections.Generic;
using System.Text;

namespace DynAbs.Test.Cases.Common
{
    class ComplexThisConstructorInitializer
    {
        static void Main()
        {
            var d = new D();
            d.internalB = new B();
            d.valD = 1;
            var c = new C();
            var a = new A(d, c);
            var x = a.v;
            return;
        }

        class A
        {
            public int v { get; internal set; }
            private A(int valA, int valB)
            {
                v = valA + valB;
            }

            private A(B b, int p, C c)
            : this(b == null ? -1 : c.MethodC(b), p)
            {
            }

            public A(D d, C c)
                : this(d.internalB, d.valD, c)
            {
            }
        }

        class B
        {

        }

        class C
        {
            public int MethodC(B b)
            {
                return 0;
            }
        }

        class D
        {
            public B internalB { get; set; }
            public int valD { get; set; }
        }
    }
}
