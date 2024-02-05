using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DynAbs.Test.Cases
{
    class UsingUninitializedScalarFields
    {
        public static void Main(string[] args)
        {
            A a = new A();
            var z1 = a.IntProp;
            var z2 = a.BoolProp;
            var z3 = a.StringProp;
            var z4 = a.EnumProp;
            return;
        }
        public class A
        {
            public int IntProp { get; set; }
            public bool BoolProp { get; set; }
            public string StringProp { get; set; }
            public B EnumProp { get; set; }
            public C myCTest { get; set; }
        }
        public enum B
        {
            Valor1=1,
            Valor2
        }
        public struct C
        {
            public int intField { get; set; }
        }
    }
}
