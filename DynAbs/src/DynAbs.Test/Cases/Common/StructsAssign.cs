using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynAbs.Test.Cases
{
    class StructsAssign
    {
        public static void Main()
        {
            var a = new A();
            a.myProperty = 1;

            var b = a;
            b.myProperty = 2;

            var c = a.myProperty;
        }

        struct A
        {
            public int myProperty;
        }
    }
}
