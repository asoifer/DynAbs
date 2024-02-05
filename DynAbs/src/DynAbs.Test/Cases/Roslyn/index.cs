using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynAbs.Test.Cases.Roslyn
{
    class Index
    {
        public static void Main(string[] args)
        {
            int i = 5;
            int j = 1;
            var int_array = new int[] { 0, i, 2 }; 
            Console.WriteLine(int_array[j]);
        }
    }
}
