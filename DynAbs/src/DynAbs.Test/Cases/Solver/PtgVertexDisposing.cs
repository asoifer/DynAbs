using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynAbs.Test.Cases.Solver
{
    class PtgVertexDisposing
    {
        public static void Main(string[] args)
        {
            Test();
            Console.WriteLine("Hola");
        }

        static void Test()
        {
            Test2();
        }

        static void Test2()
        {
            var a1 = new A();
            var a2 = new A();
        }

        class A
        {
            B b;
            public A()
            {
                b = new B();
            }
        }

        class B
        {

        }
    }
}
