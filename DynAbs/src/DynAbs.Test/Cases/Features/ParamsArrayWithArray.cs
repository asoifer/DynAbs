using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynAbs.Test.Cases.Features
{
    class ParamsArrayWithArray
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
                var temp = new object[] { hijo.valor, hijo.otroValor };
                OtraClaseEstatica.FuncionConParams(temp);
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