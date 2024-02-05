using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynAbs.Test.Cases
{
    class InheritanceCallBaseMultipleCtors
    {
        public static void Main(string[] args)
        {
            A val = new A();
        }

        static int F(int x)
        {
            return x * x;
        }

        public class A : B
        {
            public A() : base(3)
            {
                Console.WriteLine("Constructor de A");
                a = 7;
            }

            public A(int x) : base(x)
            {
                Console.WriteLine("Constructor de A");
                a = 7;
            }

            public int a;
        }

        public class B : C
        {
            public B(int x) : base(F(x))
            {
                Console.WriteLine("Constructor de B");
                b = x;
            }

            public int b;
        }

        public class C
        {
            public C(int x)
            {
                Console.WriteLine("Constructor de C");
            }
        }
    }
}
