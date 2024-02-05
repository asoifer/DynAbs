using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynAbs.Test.Cases
{
    class AliasingAssigningSimpleRef
    {
        public static void Main(string[] args)
        {
            Pepe p = new Pepe();
            p.Valor = "algun valor";
            Pepe q = p;
            string jj = q.Valor;
        }
        class Pepe
        {
            public string Valor;
        }
    }
}
