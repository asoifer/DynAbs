using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynAbs.Test.Cases.Roslyn
{
    class Cast
    {
        public static void Main(string[] args)
        {
            int intA = 4;
            float floatA= (float)intA;
            Console.WriteLine(floatA);
        }
    }
}
