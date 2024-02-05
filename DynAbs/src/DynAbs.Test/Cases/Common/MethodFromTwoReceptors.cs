using ExternalLibraryExample;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynAbs.Test.Cases
{
    class MethodFromTwoReceptors
    {
        public static void Main(string[] args)
        {
            var c1 = new Contador();
            var c2 = new Contador();

            c1.Contar();
            c1.Contar();

            c2.Contar();//falla cuando está procesando este

            return;
        }
    }

    public class Contador
    {
        public Contador()
        {
            Count = 0;
        }

        public int Count;

        public void Contar ()
        {
            Count++;
        }
    }
}
