using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynAbs.Test.Cases.PTGFixedPoint
{
    class PTGFP_Clear2ndLevel
    {
        static void Main()
        {
            for (var i = 0; i < 2; i++)
            {
                var t = new A();
                for(var j = 0; j < 2; j++)
                {
                    t.b = new B();
                }
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
