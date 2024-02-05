using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynAbs.Test.Cases
{
    class OrExpressionWithTrue
    {
        public static void Main(string[] args)
        {
            var tmp = true;
            var a = 1;
            if (tmp || Test())
                a = 2;
            Console.WriteLine(a);
            return;
        }

        public static bool Test()
        {
            return false;
        }
    }
}
