using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynAbs.Test.Cases.Common
{
    class ForWithContinue
    {
        public static void Main()
        {
            int x = 0;
            for (int i = 0; i < 10; x++, i++)
            {
                continue;
            }
            int y = x;
        }
    }
}
