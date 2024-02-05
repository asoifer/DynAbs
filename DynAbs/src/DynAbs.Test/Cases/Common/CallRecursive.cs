namespace DynAbs.Test.Cases
{
    class CallRecursive
    {
        public static void Main(string[] args)
        {
            int inicial = 7;
            int resultado = Factorial(inicial);
            int j = resultado;
        }

        public static int Factorial(int inicial)
        {
            if (inicial == 1) return 1;

            return Factorial(inicial - 1) * inicial;
        }
    }
}
