using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynAbs.Test.Cases
{
    class ArrayInitialization
    {
        public static void Main(string[] args)
        {
            A a = new A();
            double x = a.arr[0];
            Console.WriteLine(x);
        }
        public class A
        {
            public double[] arr = { .5, 6, .23, 1.2 };
        }
    }
}
