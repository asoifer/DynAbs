using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynAbs.Test.Cases
{
    class PTGFP_REC_WithoutEscapeLevel_A
    {
        static int mainValue = 4;

        static void Main()
        {
            var r = F(mainValue);
            // La idea es que si hace STRONG UPDATE en la línea 32 dejaría de lado la 34 estaría mal
            var z = r.b.i;
            return;
        }

        static A F(int num)
        {
            if (num > 1)
            {
                var r = F(num - 1);
                // Acá "new A()" no es alcanzable desde otro nivel entonces si el algoritmo clásico reemplaza el nodo haciendolo STRONG
                var a = new A();
                r.a = a; // Se tendrían que alcanzar mediante "a"... 
                a.b = r; // Se tendrían que alcanzar mediante "b"... 
                // Estarías en el 1er nivel de recursión
                if (num == mainValue)
                    r.i = 2; // Quedaría este último valor
                else
                    r.i = 1;
                return a;
            }
            return new A();
        }

        class A
        {
            public A a;
            public A b;
            public int i;
        }
    }
}
