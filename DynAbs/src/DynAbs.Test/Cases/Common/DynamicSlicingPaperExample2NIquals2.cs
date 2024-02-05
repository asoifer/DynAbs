using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynAbs.Test.Cases
{
    public class DynamicSlicingPaperExample2NIquals2
    {
        static void Main(string[] args)
        {
            int z = 0;
            int y = 0;
            int i = 1;
            int n = 2;
            while (i <= n)
            {
                z = z + y + 1;
                y = y + 2;
                i = i + 1;
            }
            z = z + 1;
            return;
        }

        //[Tests.Util.TestResult(Criteria = 23, Executed = new[] {12, 14, 15, 16, 17, 19, 20, 21, 17, 19, 20, 21, 17, 23}, Sliced = new[] { 13, 14, 15, 16, 17, 19, 20, 21, 23 })]
        public object Result { get; set; }
    }
}
