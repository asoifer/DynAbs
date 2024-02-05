using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynAbs.Test.Cases.Roslyn
{
    class NewWithNoArgumentsAndInitializer
    {
        public static void Main(string[] args)
        {
            Casa casa = new Casa {duenho = "Christian" };
            Console.WriteLine(casa.duenho);
        }

        class Casa
        {
            public string duenho = "valor";
        }
    }
}
