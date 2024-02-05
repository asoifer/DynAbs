
namespace DynAbs.Test.Cases
{
    class PTGFP_REC_WithoutEscapeLevel_B
    {
        static int mainValue = 4;
        static A _tempA, tempB;
        static A tempA
        {
            get
            {
                if (_tempA == null)
                    _tempA = new A();
                return _tempA;
            }
        }

        static void Main()
        {
            var r = F(mainValue);
            // La idea es que si hace STRONG UPDATE en la línea 40 dejaría de lado la 38 estaría mal
            var z = r.b.i;
            return;
        }

        static A F(int num)
        {
            if (num > 1)
            {
                tempB = new A();
                tempB.a = F(num - 1);
                // Acá "new A()" no es alcanzable desde otro nivel entonces si el algoritmo clásico reemplaza el nodo haciendolo STRONG
                tempA.a = new A();
                tempB.a.a = tempA.a; // Se tendrían que alcanzar mediante "a"... 
                tempA.a.b = tempB.a; // Se tendrían que alcanzar mediante "b"... 
                // Estarías en el 1er nivel de recursión
                if (num == 2)
                    tempB.a.i = 2; // Quedaría este último valor
                else
                    tempB.a.i = 1;
                return tempA.a;
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
