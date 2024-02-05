using System;

namespace DynAbs.Test.Cases
{
    class ConditionalAccessA
    {
        static void Main()
        {
            var c = new Client();
            c.id = 1;
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
