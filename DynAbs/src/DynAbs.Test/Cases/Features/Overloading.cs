using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DynAbs.Test.Suites.Slicing
{
    class Overloading
    {
        public static void Main(string[] args)
        {
            Casa casa = new Casa();
            Casa casa2 = new Casa();
           // Console.WriteLine((casa + casa2).duenho);
            casa = casa + casa2;
            Console.WriteLine(casa.duenho);
        }

        class Casa
        {
            public string duenho = "valor";

            public static Casa operator +(Casa c1, Casa c2)
            {
                c1.duenho = "pepe";
                return c1;
            }
        }
    }
}
