using System;

namespace DynAbs.Test.Cases
{
    class FuncsAsParameters
    {
        static void Main()
        {
            var a = new A();
            Func<string, bool> f = ((string x) => true);
            var r = a.MainIteration(f);
            Console.WriteLine(r);
        }

        class A
        {
            string name = "PEPE";

            public int MainIteration(Func<string, bool> f)
            {
                Watch watch = new Watch();
                watch.Callback = f;
                if (watch.Callback(this.name))
                    return 1;
                return 0;
            }

            class Watch
            {
                public Func<string, bool> Callback;
            }
        }
    }
}
