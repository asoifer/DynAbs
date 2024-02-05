using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynAbs.Test.Cases
{
    public class DynamicSlicingPaperExample1XLess0
    {
        static void Main(string[] args)
        {
            int y;
            int z;
            int x = -1;
            if (x < 0)
            {
                y = x + 2;
                z = x + 3;
            }
            else
            {
                if (x == 0)
                {
                    y = x + 4;
                    z = x + 5;
                }
                else
                {
                    y = x + 6;
                    z = x + 7;
                }
            }
            // y should be 2
            y = y + 1;
            // z should be 3
            z = z + 1;  
            return;
        }

        //[Tests.Util.TestResult(Criteria = 35, Sliced = new[] { 15, 16, 18, 35 })]
        //[Tests.Util.TestResult(Criteria = 37, Sliced = new[] { 15, 16, 19, 37 })]
        public object Result { get; set; }
    }
}
