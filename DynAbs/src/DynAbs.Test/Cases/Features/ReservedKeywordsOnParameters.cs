using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynAbs.Test.Cases.Features
{
    class ReservedKeywordsOnParameters
    {
        static void Main()
        {
            var temp = new A(true);
            temp = new A(temp);
            return;
        }

        class A
        {
            public A(bool @finally)
            {

            }

            public A(A @finally)
            {
                
            }
        }
    }
}
