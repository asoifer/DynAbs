using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynAbs.Test.Cases
{
    class PTGFP_SimpleTest
    {
        static void Main()
        {
            A a = null;
            for(var i = 0; i < 2; i++)
            {
                if (i == 0)
                    a = new A();
                a.b = new B();
            }
            return;
        }

        class A
        {
            public B b;
        }

        class B
        {

        }
    }
}
