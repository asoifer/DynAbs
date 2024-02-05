using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynAbs.Test.Cases
{
    class InlineAccessor
    {
        public static void Main()
        {
            var a = new A();
            a.MyProp = 1;
            Console.WriteLine(a.MyProp);
            return;
        }

        class A
        {

            internal int _prop = 0;

            public int MyProp
            {
                get => _prop + 1;
                set
                {
                        _prop = value;
                }
            }
        }
    }
}
