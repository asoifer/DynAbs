using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynAbs.Test.Cases
{
    class ConstField
    {
        public static void Main(string[] args)
        {
            int inicial = 5;
            Ejemplo ej = new Ejemplo();
            int fin = Ejemplo.Valor + inicial;
        }

        public class Ejemplo
        {
            public const int Valor = 3;
        }
    }
}
