using ExternalLibraryExample;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynAbs.Test.Cases
{
    class SimpleNonInstrumentedObject
    {
        public static void Main(string[] args)
        {
            Prueba p1 = new Prueba();
            p1.Obj = new object();
            Prueba p2 = new Prueba();
            p2.Obj = new object();
            Binary.Test(p1, p2);
            object valor = p1.Obj;
        }
        public class Prueba
        {
            public object Obj { get; set; }
        }
    }
}
