namespace DynAbs.Test.Cases
{
    class AliasingTestCopyRef
    {
        public static void Main(string[] args)
        {
            A a = new A();
            B b = new B();
            a.a1 = new C();
            char[] arr = new char[] { 'a' };
            object obj = new string(arr);
            b.b1 = obj;
            a.a1.c1 = b.b1;
        }

        class A
        {
            public C a1;
        }

        class B
        {
            public object b1;
        }

        class C
        {
            public object c1;
        }
    }
}
