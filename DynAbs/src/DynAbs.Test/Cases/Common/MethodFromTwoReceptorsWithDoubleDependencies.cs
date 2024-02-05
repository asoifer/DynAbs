using ExternalLibraryExample;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynAbs.Test.Cases
{
    class MethodFromTwoReceptorsWithDoubleDependencies
    {
        public static void Main(string[] args)
        {
            var c1 = new ContadorWithDoubleDep(1);
            var c2 = new ContadorWithDoubleDep(2);

            var v = c1.Contar(5);
            v = c1.Contar(v);

            v = c2.Contar(v);
        }
    }

    public class ContadorWithDoubleDep
    {
        public ContadorWithDoubleDep(int inicial)
        {
            Count = inicial;
        }

        public int Count;

        public int Contar (int cuanto)
        {
            Count = Count + cuanto;
            return Count;
        }
    }
}
