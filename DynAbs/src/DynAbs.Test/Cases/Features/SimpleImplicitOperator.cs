using System;
using System.Collections.Generic;
using System.Text;

namespace DynAbs.Test.Cases.Features
{
    class SimpleImplicitOperator
    {
        static void Main()
        {
            SimpleImplicitOperator a = new string[] { "a" };
            var b = a.v;
            return;            
        }
        public string v { get; set; }
        public static implicit operator SimpleImplicitOperator(string[] source)
        {
            return new SimpleImplicitOperator() { v = source[0] };
        }
    }
}
