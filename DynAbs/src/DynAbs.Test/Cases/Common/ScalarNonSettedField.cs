using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynAbs.Test.Cases
{
    class ScalarNonSettedField
    {
        public static void Main(string[] args)
        {
            A a = new A();
            int x = a.field;
        }

        public class A
        {
            public int field = 3;
        }
    }
}
