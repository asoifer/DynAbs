using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynAbs.Test.Cases.Common
{
    class SimplePrintWithCallControlDependency
    {
        static void Main()
        {
            var rv = Print("Hello World");
            return;
        }

        static int Print(string msj)
        {
            // No lee el parámetro
            Console.Write("Pepe");
            return 0;
        }
    }
}
