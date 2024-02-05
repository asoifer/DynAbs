namespace DynAbs.Test.Cases
{
    class AliasingThisLikeParameter
    {
        public static void Main(string[] args)
        {
            var a = new A();
            a.Click(a);
            var x = a.field;
        }
        public class A
        {
            public int field = 0;
            public void Click(A p)
            {
                if (this != null)
                {
                    this.field = 3;
                }
            }
        }
    }
}
