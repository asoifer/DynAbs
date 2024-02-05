using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynAbs.Test.Cases
{
    class EmptyMain
    {
        public static void Main(string[] args)
        {

        }

        class A
        {
            public B campoB;
            public A()
            {
                campoB = new B();
                campoB.componente = "valor de componente";
            }

            public string DoSomething()
            {
                return this.campoB.DoIt();
            }
        }
        class B
        {
            public string componente;
            public string DoIt()
            {
                return componente;
            }
        }
    }
}


