using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynAbs.Test.Cases
{
    public class VerySimpleSlicing
    {
        static void Main()
        {
            int x = 0;
            int y = 0;
            int z = 0;

            // Now x = 1.
            x = x + 1;
            
            // Now y = 10.            
            int i = 0;
            while (i <= 10)
            {
                y = y + 1;
                i++;
            }

            // Now x = 2.
            x = x + 1;

            // Now y = 11
            y = y + 1;

            // Now z = 12.
            z = y + x;
            return;
        }

        //[Tests.Util.TestResult(Criteria = 29, Sliced = new[] { 13, 18, 29 })]
        //[Tests.Util.TestResult(Criteria = 32, Sliced = new[] { 14, 21, 22, 24, 25, 32 })]
        //[Tests.Util.TestResult(Criteria = 35, Sliced = new[] { 13, 14, 18, 21, 22, 24, 25, 29, 32, 35 })]
        public object Result { get; set; }
    }
}