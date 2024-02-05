using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynAbs.Test.Cases
{


    class ObsoleteAttributeTest
    {

        [ObsoleteAttribute("This property is obsolete. Use NewProperty instead.", false)]
        
        public static string OldProperty = "Test";
        public static void Main(string[] args)
        {

            Console.WriteLine(OldProperty);
            return;
        }


    }
}
