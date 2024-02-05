using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynAbs.Test.Cases
{
    public class LegatingByControlStmt
    {
        static void Main(string[] args)
        {
            int n = 10;
            
            int y = 0;
            
            int i = y;

            int x = 0;
            while (i <= n)
            {
                x = x + 1;
            }

            // Now x = 11;
            x = x + 1;
            return;
        }

        //[Tests.Util.TestResult(Criteria = 19, Sliced = new[] { 19 })]
        //[Tests.Util.TestResult(Criteria = 26, Sliced = new[] { 13, 15, 17, 19, 20, 22, 26 })]
        public object Result { get; set; }
    }
}