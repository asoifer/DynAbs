using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynAbs.Test.Cases
{
    class ScalarDetectCallInCondition
    {
        static void Main(string[] args)
        {
            int x = 5;
            if (booleanCall(x))
            {
                x = x * 5;
            }
            else
            {
                x = 3;
            }
            int y = x;
        }

        private static bool booleanCall(int x)
        {
            if (x == 5)
            {
                return true;
            }
            return false;
        }
    }
}
