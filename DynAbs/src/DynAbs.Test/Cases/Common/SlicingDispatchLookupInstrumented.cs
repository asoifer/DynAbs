namespace DynAbs.Test.Cases
{
    class SlicingDispatchLookupInstrumented
    {
        public static void Main(string[] args)
        {
            A a = new A();
            System.Console.WriteLine(a.ToString());
        }

        class A
        {
            public override string ToString()
            {
                return "Mi propio ToString()";
            }
        }
    }
}
