using System;
using System.Collections.Generic;

namespace DynAbs.Test.Cases.Features
{
    class ComplexTypeParameters
    {
        static void Main()
        {
            var a = new A();
            a.MethodA<C, C>("");
            return;
        }

        class B { }

        class C : B { }

        class D
        {
            public D TestMethod()
            {
                return new D();
            }
        }

        class A
        {
            public void MethodA<TA, TB>(string source)
                where TA : B
                where TB : B
            {
                var (expected, actual) = MethodB<TA, TB>(source);
            }

            private (D outer, D corresponding) MethodB<TA, TB>(string source)
                where TA : B
                where TB : B
            {
                var outer = MethodC<TA>().a;
                var inner = MethodC<TB>().b;
                var correspondingOfInner = inner?.TestMethod();
                return (outer, correspondingOfInner);
            }

            protected static (D a, D b) MethodC<TC>()
                where TC : B
            {
                return (new D(), new D());
            }
        }
    }
}
