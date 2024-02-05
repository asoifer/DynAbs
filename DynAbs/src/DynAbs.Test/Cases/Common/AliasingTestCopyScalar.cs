namespace DynAbs.Test.Cases
{
    class AliasingTestCopyScalar
    {
        public static void Main(string[] args)
        {
            A a = new A();
            B b = new B();
            a.a1 = new C();
            b.b1 = 42;
            a.a1.c1 = b.b1;
        }

        class A
        {
            public C a1;
        }

        class B
        {
            public int b1;
        }

        class C
        {
            public int c1;
        }
    }
}
