using ExternalLibraryExample;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynAbs.Test.Cases
{
    class SimpleNonInstrumentedString
    {
        public static void Main(string[] args)
        {
            Prueba p1 = new Prueba();
            p1.Dato = "dato1";
            Prueba p2 = new Prueba();
            p2.Dato = "dato2";
            object obj = Binary.Test(p1, p2);
            string valor = p1.Dato;
        }
        public class Prueba
        {
            public string Dato { get; set; }
        }
    }
}
