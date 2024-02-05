namespace DynAbs.Test.Cases
{
    class ImplicitConstructor
    {
        static void Main(string[] args)
        {
            ClassA a = new ClassA();
            ClassA a2 = new ClassA();
            //when the test was performed, access to object (of the form "a.b") did not work,
            //that is why is so simple, but the idea is to test if the implicit constructor is
            //pass through properly
            a = a2;
        }
        public class ClassA
        {
        }
    }
}