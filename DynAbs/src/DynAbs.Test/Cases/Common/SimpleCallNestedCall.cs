namespace DynAbs.Test.Cases
{
    class SimpleCallNestedCall
    {
        static void Main(string[] args)
        {
            int valor = 52;
            int valor2 = 39;
            int valor3 = 101;
            int resultado = f(g(valor), h(t(valor2)));
            int j = resultado;
            //return;
        }

        static int t(int a)
        {
            return a + 1;
        }

        static int g(int c)
        {
            return c * 3;
        }

        static int h(int p)
        {
            return p - (14 * (25 + p));
        }

        static int f(int r, int s)
        {
            return r + s * s;
        }

    }
}



