using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynAbs.Test.Cases
{
    class PTGFP_REC_WithoutEscapeLevel_C
    {
        static int mainValue = 4;

        static void Main()
        {
            var r = F(mainValue);
            // La idea es que si hace STRONG UPDATE en la línea 36 dejaría de lado la 38 estaría mal
            var z = r.a.i;
            return;
        }

        static A F(int num)
        {
            // Cuando se crea no es alcanzable por ninguno
            var a = new A();
            A r = null;
            // En el último nivel no entrás, en los demás "new A()" no fue alcanzado, se crearon nuevos! 
            // (bajo el criterio anterior)
            if (num > 1)
                r = F(num - 1);

            // Apuntamos al siguiente, normal, se va a terminar apuntando a si mismo porque r = a, pero no múltiple...
            a.a = r;
            
            // Primer nivel de recursión
            if (num == mainValue - 1)
                a.i = 2; // Quedaría este último valor
            else
                a.i = 1; // Los demás niveles de recursión fueron actualizados acá pero debería ser pisado por línea 36

            return a;
        }

        class A
        {
            public A a;
            public A b;
            public int i;
        }
    }
}
