using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynAbs.Test.Cases.Features
{
    class PropertyInitializationInline
    {
        static void Main()
        {
            var a = new A();
            Console.WriteLine(a.TestPropB.AnotherProp);
            return;
        }

        public class A
        {
            public Guid TestProp { get; } = new Guid();

            public B TestPropB { get; set; } = TestMethod();

            static B TestMethod()
            {
                return new B();
            }
        }

        public class B
        {
            public int AnotherProp { get; } = 1;
        }
    }
}
