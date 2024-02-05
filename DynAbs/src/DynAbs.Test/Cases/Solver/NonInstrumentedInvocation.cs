using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynAbs.Test.Cases.Solver
{
    class NonInstrumentedInvocation
    {
        public static void Main(string[] args)
        {
            var a = new A();
            var b = new List<A>();
            b.Add(a);

            // Poscondición: 
            // Una entrada A que apunte a un nodo del grafo para a
            // Una entrada B que apunte a un nodo del grafo para b
            // Un HUB 0 con SIGMA a si mismo, LAMBDA a B. B lo apunta con SIGMA a.
            // UN HUB 1 con: Lambda a A, B y HUB0. A, B y Hub 1 lo apuntan con SIGMA
            // HUB 0 apunta a HUB 1 con Lambda
        }

        public class A
        {

        }
    }
}
