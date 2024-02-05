using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynAbs.Test.Cases
{
    class InlineOperator
    {
        public static void Main()
        {
            Attribute a = 2;
            Console.WriteLine(a.value);
            return;
        }

        public struct Attribute
        {
            internal int value;
            public Attribute(int value)
            {
                this.value = value;
            }

            public static implicit operator int(Attribute c) => c.value;
            public static implicit operator Attribute(int v) => new Attribute(v);
        }
    }
}
