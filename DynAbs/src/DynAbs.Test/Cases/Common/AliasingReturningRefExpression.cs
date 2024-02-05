using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynAbs.Test.Cases
{
    class AliasingReturningRefExpression
    {
        public static void Main(string[] args)
        {
            Prueba pr = new Prueba();
            Prueba p3 = Invocar(pr.P());
            string v = p3.Valor;
        }
        public static Prueba Invocar(Prueba prueba)
        {
            Prueba ret2 = new Prueba();
            ret2.Valor = prueba.Valor;

            return ret2;
        }
        public class Prueba
        {
            public string Valor;

            public Prueba P()
            {
                Prueba ret = new Prueba();
                ret.Valor = "un valor seteado";
                return ret;
            }
        }
    }
}
