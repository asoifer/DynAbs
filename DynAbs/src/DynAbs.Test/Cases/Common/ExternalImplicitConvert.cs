using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ExternalLibraryExample;

namespace DynAbs.Test.Cases
{
    class ExternalImplicitConvert
    {
        static void Main()
        {
            var str = "Hello World!";
            var i = TestMethod(str);
            Console.WriteLine(i);
        }

        static int TestMethod(CustomString a)
        {
            return a.Length;
        }
    }
}
