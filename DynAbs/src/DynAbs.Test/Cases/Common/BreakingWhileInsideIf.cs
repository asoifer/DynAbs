namespace DynAbs.Test.Cases
{
    class BreakingWhileInsideIf
    {
        static void Main(string[] args)
        {
            int x = 1;
            int z = 1;
            while (true)
            {
                z = x + x;
                if (x == 1) break;
                x = x + 1;
            }
            int y = x + z;
        }
    }
}
