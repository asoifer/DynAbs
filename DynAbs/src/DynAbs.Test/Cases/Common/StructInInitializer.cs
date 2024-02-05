using System;

namespace DynAbs.Test.Cases.Common
{
    class StructInInitializer
    {
        static void Main()
        {
            var b = new B();
            var a = new A() { b = b };
            var c = a.b.x;
            return;
        }

        class A
        {
            public B b { get; set; }
        }

        struct B
        {
            public int x { get; set; }
        }
    }
}
