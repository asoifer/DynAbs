using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynAbs.Test.Cases
{
    class ComplexForWithFunction
    {
        public static void Main()
        {
            var a = 1;
            for(var i = GetZero(); i < GetOne(); i++)
                a++;
            var j = a;
        }

        static int GetZero()
        {
            return 0;
        }

        static int GetOne()
        {
            return 1;
        }
    }
}
