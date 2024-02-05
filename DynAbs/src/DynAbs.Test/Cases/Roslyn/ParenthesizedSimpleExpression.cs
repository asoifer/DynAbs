using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynAbs.Test.Cases
{
    class ParenthesizedSimpleExpression
    {
        static void Main(string[] args)
        {
            int j = 1;
            var g = (j);
            var h = g + j;
            Console.WriteLine(h);
        }
    }
}
