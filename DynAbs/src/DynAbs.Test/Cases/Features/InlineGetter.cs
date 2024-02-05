using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynAbs.Test.Cases.Features
{
    class InlineGetter
    {
        static void Main()
        {
            var a = new A();
            var p = a.TestProp;
            Console.WriteLine(p);
            return;
        }

        class A
        {
            public bool TestProp => Pepe();

            private bool Pepe()
            {
                return false;
            }
        }
    }
}
