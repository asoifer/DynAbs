using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynAbs.Test.Cases.Common
{
    class GenericClasses
    {
        public static void Main()
        {
            A<int> a = new A<int>(5);
            var b = a.Method();
            var c = b + 4;
        }

        public class A<T> where T : new()
        {
            T current;
            public A(T _current)
            {
                current = _current;
            }
            public T Method()
            {
                return current;
            }
        }
    }
}
