namespace DynAbs.Test.Cases
{
    class CallWithinAnotherCall
    {
        public static void Main(string[] args)
        {
            int factor = 25;
            int resultado = Suma(factor, 2);
        }

        public static int Suma(int a, int b)
        {
            int parcial = Multiplica(a, b);
            return parcial;
        }

        public static int Multiplica(int s, int t)
        {
            return s * t;
        }
    }
}
