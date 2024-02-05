using ExternalLibraryExample;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynAbs.Test.Cases.Solver
{
    public class SolverCallback0
    {
        static void Main(string[] args)
        {
            Called called = new Called();
            Binary bin = new Binary(called);
            bin.performCallback0_Once();

            // Poscondición:
            // El called es un nodo del grafo común.
            // El bin es un nodo del grafo que apunta a un HUB, el HUB apunta por lambda a todos. 
            // Todos apuntan al HUB por Sigma, inclusive si mismo
            // La llamada al método crea un HUB (hub 1). Este HUB que hace? 
            // Apunta por lambda a todos. Todos lo apuntan por Sigma salvo el hub anterior que es por lambda
            // CALLBACK
            // Genera un HUB 2:
            // Apunta por Lambda a todos lados
            // Todos lo apuntan por Lambda
            // Todos lo apuntan por Sigma, inclusive si mismo, es decir: 
            // Esto ocurre porque cualquiera es argumento del Havoc
        }

        public class Called : IFramework0
        {
            public void Callback()
            {

            }
        }
    }
}
