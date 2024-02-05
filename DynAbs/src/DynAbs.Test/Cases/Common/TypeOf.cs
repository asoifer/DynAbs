using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynAbs.Test.Cases
{
    class TypeOf
    {
        public static void Main(string[] args)
        {
            var aux = typeof(bool);
            bar();
            var a = 2;
            if (foo(aux))
            {
                a = 5;
            }
            int b = a;
        }

        public static bool foo(Type t)
        {
            if (t == typeof(bool))
            {
                return true;
            }
            return false;
        }
        public static bool bar()
        {
            if (true)
            {
                return true;
            }
            return false;
        }
    }
}
