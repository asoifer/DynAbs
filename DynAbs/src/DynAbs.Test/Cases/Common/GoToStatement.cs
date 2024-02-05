using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynAbs.Test.Cases
{
    class GoToStatement
    {
        public static void Main(string[] args)
        {
            var t = M();
        }

        static int M()
        {
            int dummy = 0;
            goto Outer;
            Outer:
                dummy = 1;
            return dummy;
        }
    }
}
