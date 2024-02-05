using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynAbs.Test.Cases.Roslyn
{
    class NewWithNoArgumentsAndInitializers
    {
        public static void Main(string[] args)
        {
            Casa casa = new Casa { duenho = "Christian" , direccion = "unaDireccion"};
            string v = casa.duenho + casa.direccion;
            Console.WriteLine(v);
        }

        class Casa
        {
            public string duenho = "valor";
            public string direccion = "valor";
        }
    }
}
