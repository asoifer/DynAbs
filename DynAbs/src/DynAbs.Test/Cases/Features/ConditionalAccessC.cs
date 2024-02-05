using System;

namespace DynAbs.Test.Cases
{
    class ConditionalAccessC
    {
        static void Main()
        {
            Client c = new Client();
            var b = c?.getId();
            var d = b;
            return;
        }

        class Client
        {
            public int getId()
            {
                return 1;
            }
        }
    }
}
