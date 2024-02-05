using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynAbs.Test.Cases
{
    class FlagsAttributeTest
    {
        // Define an Enum without FlagsAttribute.
        enum SingleHue : short
        {
            None = 0,
            Black = 1,
            Red = 2,
            Green = 4,
            Blue = 8
        };

        // Define an Enum with FlagsAttribute.
        [FlagsAttribute]
        enum MultiHue : short
        {
            None = 0,
            Black = 1,
            Red = 2,
            Green = 4,
            Blue = 8
        };



        public static void Main(string[] args)
        {
            int i = 4;
            Console.WriteLine("{0,3} - {1:G}", i, (SingleHue)i);
            Console.WriteLine("{0,3} - {1:G}", i, (MultiHue)i);
            return;
        }
    }
}
