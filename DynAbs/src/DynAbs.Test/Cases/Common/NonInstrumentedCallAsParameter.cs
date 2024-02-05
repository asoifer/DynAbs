namespace DynAbs.Test.Cases
{
    class NonInstrumentedCallAsParameter
    {
        static void Main(string[] args)
        {
            A a = new A();
            System.Random random = new System.Random();
            int y = 5;
            int x = a.f(a.g(y), a.h(random.Next()));
            int z = x;
        }
        public class A
        {
            public int f(int x, int y)
            {
                return x + y;
            }
            public int g(int z)
            {
                return z;
            }
            public int h(int t)
            {
                return t;
            }
        }
    }
}