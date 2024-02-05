using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynAbs.Test.Cases.Solver
{
    class NonInstrumentedNewWithArguments
    {
        public static void Main(string[] args)
        {
            var a = new A();
            a.x = 1;
            var b = new List<A>() { a };
            return;
            // Poscondición: 
            // Una entrada A que apunte a un nodo del grafo para a
            // Una entrada B que apunte a un nodo del grafo para b
            // A y B apuntan a un HUB por Sigma. Hay otro sigma para si mismo. 
            // Del Hub salen lambdas para B y A.
        }

        public class A
        {
            public int x { get; set; }
        }
    }
}
