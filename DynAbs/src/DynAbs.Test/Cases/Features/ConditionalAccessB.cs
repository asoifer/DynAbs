using System;

namespace DynAbs.Test.Cases
{
    class ConditionalAccessB
    {
        static void Main()
        {
            Client c = null;
            var b = c?.id;
            var d = b;
            return;
        }

        class Client
        {
            public int id { get; set; }
        }
    }
}
