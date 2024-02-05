using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynAbs.Test.Cases.Roslyn
{
    class CallWithObjectRef
    {
        public static void Main(string[] args)
        {
            A val = new A(4);
            M(ref val);
            var x = val.a;
            Console.WriteLine(x);
        }

        static void M(ref A i)
        {
            i = new A(5);
        }

        public class A
        {
            public A(int x)
            {
                a = x;
            }
            public int a;
        }
    }
}
