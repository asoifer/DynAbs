using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynAbs.Test.Cases
{
    class ParamsArray
    {
        static void Main()
        {
            var a = new A();
            a.UnaFuncion();
            return;
        }

        public class A
        {
            public virtual B hijo { get; set; }

            public A()
            {
                hijo = new B();
            }

            public void UnaFuncion()
            {
                OtraClaseEstatica.FuncionConParams(hijo.valor, hijo.otroValor);
            }
        }

        public class B
        {
            public int valor { get; set; }
            public int otroValor { get; set; }
            public B()
            {
                valor = 1;
            }
        }


        public static class OtraClaseEstatica
        {
            public static void FuncionConParams(params object[] keys)
            {

            }
        }

    }
}
