using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynAbs.Test.Cases
{
    class InvocationChainWithProperty
    {
        public static void Main(string[] args)
        {
            Alpha alpha = new Alpha();
            string pasadoPorParametro = "esta siendo pasado";
            Gamma gamma = new Gamma(pasadoPorParametro);
            Delta del = new Delta();
            string resultado = alpha.TwoParamsMethod(gamma.AskForEpsilon, del).Prueba;
        }

        class Alpha
        {
            public Beta TwoParamsMethod(Epsilon e, Delta d)
            {
                Beta be = new Beta();
                be.Recibido = e._data;
                be.Prueba = e._data;
                return be;
            }
        }

        class Beta
        {
            public string Recibido { get; set; }
            public string Prueba {get;set;}
        }

        class Gamma
        {
            private string passedStr;

            public Gamma(string str)
            {
                this.passedStr = str;
            }


            public Epsilon AskForEpsilon
            {
                get
                {
                    Epsilon eps = new Epsilon();
                    eps._data = passedStr;
                    return eps;
                }
            }
        }

        class Delta { }

        class Epsilon
        {
            public string _data;
        }
    }
}