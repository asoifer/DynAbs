using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynAbs.Test.Cases
{
    class FieldInitializationAtDeclaration
    {
        static void Main(string[] args)
        {
            A a = new A();
            int x = a.c.field;
        }
        public class A
        {
            private static B func1()
            {
                return new B();
            }
            public A()
            {
                Console.WriteLine("Constructor de A");
            }
            public C c = new C();
            public B b = func1();
        }
        public class B
        {
            public B()
            {
                Console.WriteLine("Constructor de B");
            }
        }
        public class C
        {
            public int field;
            public C()
            {
                field = 3;
                Console.WriteLine("Constructor de C");
            }
        }
    }
}
