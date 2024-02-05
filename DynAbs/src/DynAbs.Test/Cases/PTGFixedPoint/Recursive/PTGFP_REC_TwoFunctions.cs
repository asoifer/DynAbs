using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynAbs.Test.Cases
{
    class PTGFP_REC_TwoFunctions
    {
        static void Main()
        {
            var a = new A();
            a.i = 8;
            BuildA(a, a.i - 1);
            var c = a.a.a.a.a.i;
            return;
        }

        static void BuildA(A a, int num)
        {
            a.a = new A();
            a.a.i = num;
            if (num > 0)
                BuildB(a.a, num - 1);
        }

        static void BuildB(A a, int num)
        {
            a.a = new A();
            a.a.i = num;
            if (num > 0)
                BuildA(a.a, num - 1);
        }

        class A
        {
            public A a;
            public int i;
        }
    }
}
