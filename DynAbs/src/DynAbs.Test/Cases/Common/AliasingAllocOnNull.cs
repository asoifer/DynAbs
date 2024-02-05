using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynAbs.Test.Cases
{
    class AliasingAllocOnNull
    {
        public static void Main(string[] args)
        {
            Nodo nodo = null;
            Nodo nodob = nodo;
        }
        class Nodo
        {

        }
    }
}
