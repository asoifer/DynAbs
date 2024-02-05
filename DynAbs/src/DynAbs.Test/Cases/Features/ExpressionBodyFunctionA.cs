using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynAbs.Test.Cases.Features
{
    class ExpressionBodyFunctionA
    {
        public static string TestMethod(string a) => a + " ";

        public static void Main()
        {
            var myString = @"PEPE";
            myString = TestMethod(myString);
            Console.WriteLine(myString);
            return;
        }
    }
}
