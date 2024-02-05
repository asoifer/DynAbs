using System;

namespace DynAbs.Test.Cases.Common
{
    class SwitchCaseWithPropertiesCallback
    {
        static void Main()
        {
            var a = new A();
            var b = new B1();
            var x = GetVal(a);
            var y = GetVal(b);
            Console.WriteLine(x + y);
            return;
        }

        static int GetVal(TestInterface ti)
        {
            switch (ti)
            {
                case A { MyProp: 2 }:
                    return 1;
                case B1 { b2: { B: false } }:
                    return 2;
                default:
                    return 3;
            }
        }

        interface TestInterface { }

        class A : TestInterface
        {
            public int MyProp
            {
                get
                {
                    return 1;
                }
            }
        }

        class B1 : TestInterface
{
            public B2 b2
            {
                get
                {
                    return new B2();
                }
            }
        }

        class B2
        {
            public bool B
            {
                get
                {
                    return true;
                }
            }
        }
    }
}
