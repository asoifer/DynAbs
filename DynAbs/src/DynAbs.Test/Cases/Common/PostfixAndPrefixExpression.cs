using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynAbs.Test.Cases
{
    class PostfixAndPrefixExpression
    {
        public static void Main(string[] args)
        {
            A a = new A();
            a.Prop++;
            --a.Prop;
            int z = a.Prop;
        }
        public class A
        {
            public int Prop { get; set; }
        }
    }
}
