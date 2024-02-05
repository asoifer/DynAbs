using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynAbs.Test.Cases.Common
{
    class ArrayAsParameter
    {
        public static void Main(string[] args)
        {
            A a = new A();
            a.Arr = new int[2] { 7, 42 };
            int b = M(a.Arr);
        }
        static int M(params int[] arr)
        {
            return arr[0] + arr[1];
        }
        class A
        {
            public int[] Arr { get; set; }
        }
    }
}
