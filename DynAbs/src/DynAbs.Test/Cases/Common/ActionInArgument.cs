using System;

namespace DynAbs.Test.Cases.Common
{
    class ActionInArgument
    {
        static void Main()
        {
            var f = new Action<A>(a => { });
            var x = Test(f);
            Console.WriteLine(x);
            return;
        }

        public class A { }

        public static int Test(Action<A> a)
        {
            Action<A> translate(Action<A> action)
            {
                if (action != null)
                {
                    return (m) => action(m);
                }
                else
                {
                    return null;
                }
            }

            return TestB(translate(a));
        }

        public static int TestB(Action<A> a)
        {
            return a != null ? 0 : 1;
        }
    }
}
