using System;

namespace DynAbs.Test.Cases.Features
{
    class LocalFunctions
    {
        static void Main()
        {
            var a = new A();
            a.PublicMethod();
            var z = a.b.x;
            return;
        }

        public class A
        {
            public B b { get; set; }
            public void PublicMethod()
            {
                b = new B();
                Test(b);

                static void Test(B b)
                {
                    b.DoSomething();
                }
            }
        }

        public class B
        {
            public int x { get; set; }

            public void DoSomething()
            {
                x = 1;
            }
        }
    }
}
