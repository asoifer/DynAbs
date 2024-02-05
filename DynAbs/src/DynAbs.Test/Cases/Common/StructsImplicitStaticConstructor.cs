using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynAbs.Test.Cases
{
    class StructsImplicitStaticConstructor
    {
        public static void Main(string[] args)
        {
            int z = MyStruct.Prop1.f;
        }
        public struct MyStruct
        {
            public static A Prop1 = new A();
        }
        public class A
        {
            public A()
            {
                f = 3;
            }
            public int f;
        }
    }
}
