using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynAbs.Test.Cases
{
    class InheritanceCallBase
    {
        public static void Main(string[] args)
        {
            A a = new A(5);
        }

        public class A : B
        {
            int a;
            public A(int x) : base(x)
            {
                Console.WriteLine("Constructor de A");
            }
        }

        public class B
        {
            int b;
            public B(int x)
            {
                Console.WriteLine("Constructor de B");
                b = x;
            }
        }
    }
}
