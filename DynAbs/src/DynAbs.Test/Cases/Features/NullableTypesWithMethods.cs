using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynAbs.Test.Cases
{
    class NullableTypesWithMethods
    {
        public static void Main(string[] args)
        {
            int? num = 5;
            if (num.HasValue)
            {
               System.Console.WriteLine("num = " + num.Value);
            }
            return;
        }
    }
}
