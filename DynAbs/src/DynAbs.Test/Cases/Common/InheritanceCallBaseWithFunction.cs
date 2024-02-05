using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynAbs.Test.Cases
{
    class InheritanceCallBaseWithFunction
    {
        public static void Main(string[] args)
        {
            A a = new A(5);
        }

        static int F(int x)
        {
            Console.WriteLine("Funcion F");
            return x * x;
        }

        public class A : B
        {
            int a;
            public A(int x) : base(F(x))
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
