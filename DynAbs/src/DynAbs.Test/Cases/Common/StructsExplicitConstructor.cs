using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynAbs.Test.Cases
{
    class StructsExplicitConstructor
    {
        public static void Main(string[] args)
        {
            int x = 1;
            MyStruct myStruct = new MyStruct(x, 4);
            int z = myStruct.Prop1;
        }
    }
    public struct MyStruct
    {
        public MyStruct(int x, int y)
        {
            Prop1 = x + y;
        }
        public int Prop1;
    }
}
