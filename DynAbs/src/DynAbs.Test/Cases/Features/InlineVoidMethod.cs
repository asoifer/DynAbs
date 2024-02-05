using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynAbs.Test.Cases
{
    class InlineVoidMethod
    {
        static void Main()
        {
            TestMethod(1);
            return;
        }

        static void TestMethod(int i) => Console.WriteLine(i);
    }
}
