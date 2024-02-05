using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynAbs.Test.Cases.Features
{
    class DefaultKeywordWithTrivia
    {
        static void Main()
        {
            var temp = default(
#if PEPE
                A 
#else
                B
#endif
                );
            Console.WriteLine(temp == null);
            return;
        }

        class A
        {
            public int test_field = 1;
        }

        class B : A
        {

        }
    }
}
