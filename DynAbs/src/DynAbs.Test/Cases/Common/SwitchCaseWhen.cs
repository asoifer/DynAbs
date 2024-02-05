using System;
using System.Collections.Generic;
using System.Text;

namespace DynAbs.Test.Cases
{
    class SwitchCaseWhen
    {
        static void Main()
        {
            int a = 1;
            var ret = 0;
            switch (a)
            {
                case 1 when f() > 0:
                    ret = 1;
                    break;
                case 1:
                    ret = 2;
                    break;
                default:
                    ret = 3;
                    break;
            }
            var toSlice = ret;
            return;
        }

        static int f()
        {
            return 1;
        }
    }
}
