using System;

namespace DynAbs.Test.Cases
{
    class ConditionalAccessD
    {
        static void Main()
        {
            Client c = null;
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
