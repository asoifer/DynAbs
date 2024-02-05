using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynAbs.Test.Cases
{
    class NestedClasses
    {
        static void Main(string[] args)
        {
            var c = new A.B.C();
            Console.WriteLine("Hello World");
        }

        class A
        {
            int field1 = 1;

            public class B
            {
                int field2 = 2;

                public class C
                {
                    int field3 = 3;
                }
            }
        }

    }
}
