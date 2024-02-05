namespace DynAbs.Test.Cases
{
    class AliasingUsingNestedThis
    {
        public static void Main(string[] args)
        {
            B b = new B();
            int i = 25;
            int j = 42;
            b.Init();
            j = b.HacerAlgo();
            int k = i + j;
        }

        class B
        {
            public int HacerAlgo()
            {
                C c = new C();
                c.Init();
                int x = this.bc1;
                int pepito = c.Sumar(25, x);
                int y = this.bc1;
                return pepito + y;
            }

            internal void Init()
            {
                this.bc1 = 3;
                return;
            }

            public int bc1 { get; set; }
        }

        class C
        {
            public int Sumar(int p, int q)
            {
                int z = this.cc1;
                return p + z + q;
            }

            internal void Init()
            {
                this.cc1 = 7;
                return;
            }

            public int cc1 { get; set; }
        }
    }
}
