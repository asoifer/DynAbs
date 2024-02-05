using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynAbs.Test.Cases
{
    class StructsExplicitStaticConstructor
    {
        public static void Main(string[] args)
        {
            int z = MyStruct.Prop1;
        }
        public struct MyStruct
        {
            static MyStruct()
            {
                Prop1 = 3;
            }
            public static int Prop1;
        }
    }
}
