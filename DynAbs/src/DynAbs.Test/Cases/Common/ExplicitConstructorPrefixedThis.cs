using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynAbs.Test.Cases
{
    class ExplicitConstructorPrefixedThis
    {
        static void Main(string[] args)
        {
            ClassA a = new ClassA();
            ClassA a2 = new ClassA();
            a.Value1 = a2.Value1 + a2.Value2;
        }
        public class ClassA
        {
            public int Value1 { get; set; }
            public int Value2 { get; set; }
            public ClassA()
            {
                this.Value1 = 3;
                this.Value2 = 4;
            }

        }
    }
}
