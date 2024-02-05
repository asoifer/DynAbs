using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynAbs.Test.Cases
{
    class NotSupportedForeachWithCall
    {
        static void Main(string[] args)
        {
            IList<A> list = new List<A>();
            A a1 = new A(1);
            list.Add(a1);
            int res = 0;
            foreach (A a in list)
            {
                res = res + a.getValue();//esto es visitado por el walker, y la instrumentacion del bloque foreach (antes del foreach) trata de acceder a la variable, lo cual no compila
            }
            int z = res;
        }
        public class A
        {
            public int getValue()
            {
                return this.Number;
            }

            public A(int i)
            {
                Number = i;
            }
            public int Number { get; set; }
        }
    }
}
