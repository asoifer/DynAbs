namespace DynAbs.Test.Cases
{
    class AliasingPassingReference
    {
        public static void Main(string[] args)
        {
            A a = new A();
            B b = new B();
            a.Init(b);
            string str = a.a1.b1;
        }

        public class A
        {
            public B a1;

            public void Init(B __b)
            {
                this.a1 = __b;
                this.a1.b1 = "Prueba";
                return;
            }
        }

        public class B
        {
            public string b1 = "";
        }
    }
}
