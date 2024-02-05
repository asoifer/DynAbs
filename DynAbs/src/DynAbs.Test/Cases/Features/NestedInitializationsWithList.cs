using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynAbs.Test.Cases
{
    class NestedInitializationsWithList
    {
        static readonly A MyInstance = new A
        {
            List = {
                GetString("HELLO "),
                GetString("BEAUTIFUL ")
            }
        };

        static void Main()
        {
            Console.WriteLine(MyInstance.List[0]);
            return;
        }

        static string GetString(string p)
        {
            return p + "WORLD";
        }

        class A
        {
            public List<string> List { get; set; }
        }
    }
}
