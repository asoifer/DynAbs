using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynAbs.Test.Cases
{
    class ParamsBinaryExpression
    {
        public static void Main(string[] args)
        {
            Casa casa = new Casa { duenho = "Christian", direccion = "unaDireccion" };
            Console.WriteLine(casa.duenho + casa.direccion);
        }

        class Casa
        {
            public string duenho = "valor";
            public string direccion = "valor";
        }
    }
}
