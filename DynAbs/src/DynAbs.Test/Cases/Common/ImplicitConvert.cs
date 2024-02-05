using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynAbs.Test.Cases
{
    class ImplicitConvert
    {
        static void Main()
        {
            var str = "Hello World!";
            var i = TestMethod(str);
            Console.WriteLine(i);
        }

        static int TestMethod(A a)
        {
            return a.Length;
        }

        class A
        {
            public int Length { get; set; }
            public static implicit operator A(string str)
            {
                var a = new A();
                a.Length = str.Length;
                return a;
            }
        }
    }
}
