using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynAbs.Test.Cases
{
    class NotSupportedUsing
    {
        static void Main(string[] args)
        {
            A a = new A(1);
            int x = 0;
            using (a)
            {
                a.Number = x;
            }
            int y = a.Number;
        }
        public class A : IDisposable
        {
            public A(int i)
            {
                Number = i;
            }
            public int Number { get; set; }
            public void Dispose()
            {
                Number = 3;
            }
        }
    }
}
