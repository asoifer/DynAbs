namespace DynAbs.Test.Cases
{
    class NonInstrumentedCall
    {
        static void Main(string[] args)
        {
            int x = 0;
            System.Random random = new System.Random();
            x = random.Next(0, 100);
            int y = x + 1;
            int z = y;
        }
    }
}
