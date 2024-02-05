using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynAbs.Test.Cases
{
    class LiteralToObjectBinding
    {
        public static void Main(string[] args)
        {
            A a = new A();
            int x = 4;
            a.func(x);
        }
        public class A 
        {
            object field = null;
            public void func(object obj)
            {
                field = obj;
            }
        }
    }
}
