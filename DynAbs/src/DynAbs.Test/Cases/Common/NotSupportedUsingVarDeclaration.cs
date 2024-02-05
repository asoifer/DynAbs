using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynAbs.Test.Cases
{
    class NotSupportedUsingVarDeclaration
    {
        static void Main(string[] args)
        {
            var b = new B();
            int x = 0;
            using (var a = new A(1, b))
            {
                x = a.Number;
            }
            int y = b.b + x;
        }
        public class A : IDisposable
        {
            public A(int i, B tmpB)
            {
                Number = i;
                bNumber = tmpB;
            }
            public int Number { get; set; }
            public B bNumber { get; set; }
            public void Dispose()
            {
                bNumber.b = 3;
            }
        }
        public class B
        {
            public int b { get; set; }
            public B()
            {
                b = 1;
            }
        }
    }
}
