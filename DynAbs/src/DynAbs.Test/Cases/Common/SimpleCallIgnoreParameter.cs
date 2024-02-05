namespace DynAbs.Test.Cases
{
    class SimpleCallIgnoreParameter
    {
        static void Main(string[] args)
        {
            int valor = 52; // LastDef[valor] = s7.1
            int valor2 = 39;// LastDef[valor2] = s7.1
            int resultado = Suma(valor, valor2);
            int j = resultado;
            return;
        }

        static int Suma(int a, object b)
        {
            return a;
        }
    }
}



