using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynAbs.Test.Cases
{
    class MemberAccessChainCase
    {
        public static void Main(string[] args)
        {
            string s = CreateAlice().Valor;
        }
        public static Alice CreateAlice()
        {
            Alice ali = new Alice();
            ali.Valor = "Cierto valor";
            return ali;
        }
    }

    class Alice
    {
        public string Valor { get; set; }
    }
}
