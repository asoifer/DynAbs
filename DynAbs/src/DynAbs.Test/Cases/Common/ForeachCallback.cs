using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynAbs.Test.Cases
{
    class ForeachCallback
    {
        static void Main()
        {
            var acum = 0;
            var listaNumeros = new int[] { 1, 2, 3 };
            var myEnumerator = listaNumeros.Select(x => new A(x));
            foreach (var unElemento in myEnumerator)
                acum += unElemento.myProperty;
            var aux = acum;
            return;
        }

        class A
        {
            public int myProperty;

            public A(int valor)
            {
                myProperty = valor;
            }
        }

        //var temp = myEnumerator.GetEnumerator();
        //while (temp.MoveNext())
        //    acum += temp.Current.myProperty;
    }
}
