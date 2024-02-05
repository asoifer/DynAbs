using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynAbs.Test.Cases.Solver
{
    class InstrumentedNew
    {
        public static void Main(string[] args)
        {
            var a = new A();
            // Poscondición: El grafo de la memoria tiene que tener 1 entrada apuntando a un nodo del grafo
            // Opcional: puede haber otra entrada apuntando al mismo nodo
        }

        class A
        {

        }
    }
}
