using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynAbs.Test.Cases
{
    class InheritanceImplicitCallBase
    {
        public static void Main(string[] args)
        {
            A a = new A(5);
        }

        public class A : B
        {
            int a;
            public A(int x)
            {
                Console.WriteLine("Constructor de A");
            }
        }

        public class B
        {
            int b;
            public B()
            {
                Console.WriteLine("Constructor de B");
                b = 3;
            }
        }
    }
}
