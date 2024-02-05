using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynAbs.Test.Cases
{
    class NewArray
    {
        static void Main()
        {
            var a = new A();
            return;
        }

        public class A
        {
            public static int[] myList = { 1, 2, 3, 4 };

            public A()
            {

            }
        }
    }
}
