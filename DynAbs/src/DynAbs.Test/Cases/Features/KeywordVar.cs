using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynAbs.Test.Cases
{
    class KeywordVar
    {
        public static void Main(string[] args)
        {
        
            var int_var = 6;
            var int_var2 = 4;
            var string_var = "Mony";         
            var int_array = new[] { 0, 1, 2 };    
            var string_array = new[] { "hello", null, "world" };

            var result = int_var + int_var2;

            foreach (var item in string_array)
            {
                Console.WriteLine(int_array);
            }

        }
    }
}

