using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynAbs.Test.Cases.Solver
{
    class NonInstrumentedChainObjects
    {
        public static void Main(string[] args)
        {
            var a = new A();
            var b = new List<A>() { a };
            var c = new List<List<A>>() { b };
            // Poscondición: 
            // Una entrada A que apunte a un nodo del grafo para a
            // Una entrada B que apunte a un nodo del grafo para b
            // Una entrada C que apunte a un nodo del grafo para c
            
            // HUB 0: A, B y Hub0 tienen ejes SIGMA a Hub0. Salen Lambdas para A, B y Hub 1
            // HUB 1: B, C, Hub1 tienen ejes SIGMA a Hub1. Hub0 y A tiene Lambda a Hub1. Salen Lambdas para A, B, C y Hub 0
        }

        public class A
        {

        }
    }
}
