using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynAbs.Test.Cases.Solver
{
    class ListAddWithReadOnlyLambda
    {
        public static void Main(string[] args)
        {
            var a = new A();
            var b = new A();
            var c = new List<A>();
            c.Add(a);
            c.Add(b);

            // Poscondición: 
            // Una entrada A que apunte a un nodo del grafo para a
            // Una entrada B que apunte a un nodo del grafo para b
            // Una entrada C que apunte a un nodo del grafo para c

            // HUB 0: c apunta al HUB
            // HUB 1: el HUB apunta por lambda a C, por readonly lambda a A, C apunta a HUB por Sigma
            // HUB 2: el HUB apunta por lambda a C, por readonly lambda a A y a B, C apunta a HUB por Sigma
        }

        public class A
        {

        }
    }
}
