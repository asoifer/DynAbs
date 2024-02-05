using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynAbs.Test.Cases.Features
{
    class StringInObjectParameter
    {
        public static void Main(string[] args)
        {
            var a = new A();
            a.funcion("hola");
        }

        public class A
        {
            object value = null;
            public void funcion(object obj)
            {
                value = obj;
            }
        }
    }
}
