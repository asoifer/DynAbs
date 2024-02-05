using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynAbs.Test.Cases
{
    class ActionOnFieldNeedingThisPrefix
    {
        public static void Main(string[] args)
        {
            A a = new A();
            string resultado = a.DoSomething();
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
                /// FIXME: se debe probar tambien con return campoB.DoIt();
                /// FIXME: se debe probar tambien con this.campoB.DoIt();
                string cc = campoB.DoIt();
                return cc;
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
