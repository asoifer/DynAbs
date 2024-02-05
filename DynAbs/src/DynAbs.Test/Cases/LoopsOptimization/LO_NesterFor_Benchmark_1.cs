using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace DynAbs.Test
{
    class LO_NesterFor_Benchmark_1
    {
        static void Main()
        {
            var p = new A(); // For static constructor of A
            for(var i = 0; i < 10; i++)
            {
                var temp = new A();
                for(var j = 0; j < 5; j++)
                {
                    temp.aField++;
                }
            }
            return;
        }

        class A
        {
            public int aField = 0;
        }
    }
}
