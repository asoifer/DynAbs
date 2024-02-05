using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynAbs.Test.Cases
{
    class NullParameter
    {
        public static void Main()
        {
            A myANullInstance = null;
            var resultInstance = TestMethod(myANullInstance);
            var temp = resultInstance;
            return;
        }
        static A TestMethod(A myArg)
        {
            var temp = myArg;
            return temp;
        }
        class A
        {

        }
    }
}
