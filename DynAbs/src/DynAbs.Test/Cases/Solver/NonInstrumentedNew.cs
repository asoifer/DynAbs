using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynAbs.Test.Cases.Solver
{
    class NonInstrumentedNew
    {
        public static void Main(string[] args)
        {
            var a = new List<int>();
            // Poscondición: Una entrada apuntando a un nodo del grafo . 
            // El nodo del grafo debe apuntar a un HUB. 
            // Del hub debe salir un Lambda al nodo, y del nodo debe salir un Sigma al HUB.
            // Del hub debe salir un Sigma a si mismo.
            // Opcional: puede haber otras entradas que apunten a ambos nodos del grafo.
        }
    }
}
