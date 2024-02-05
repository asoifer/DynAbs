using System;
using System.Collections.Generic;
using System.Text;

namespace DynAbs.Test.Cases.Common
{
    class SwitchWithDeconstructor
    {
        static void Main()
        {
            var a1 = new A.A1B();
            var a2 = new A.A2();
            var a3 = new A.A3(new B());

            var i1 = magicMethod(a1);
            var i2 = magicMethod(a2);
            var i3 = magicMethod(a3);
            var x = i1 + i2 + i3;
            return;
        }

        static int magicMethod(A a)
        {
            switch (a)
            {
                case A.A1 a1:
                    return 1;
                case A.A2 _:
                    return 2;
                case A.A3(B b):
                    return 3;
                default:
                    throw new Exception();
            }
        }

        public class A
        {
            public abstract class A1 : A
            {
            }

            public class A1B : A1
            {

            }

            public class A2 : A
            {
            }

            public sealed class A3 : A
            {
                public B B;
                public A3(B b) => this.B = b;
                public void Deconstruct(out B b) => b = new B();
            }
        }


        public class B
        {

        }
    }
}
