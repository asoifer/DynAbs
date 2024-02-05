using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynAbs.Test.Cases
{
    class CallWithReferencesParameters
    {
        public static void Main(string[] args)
        {
            A a = new A();
            a.c = new C();
            a.c.d = new D();
            a.c.d.e = new E();
            a.c.d.e.last = 1;
            B b = new B();
            b.y = 2;
            int k = Sum(a.c.d.e.last, b);
            int j = k;
        }

        public static int Sum(int alfa, B beta)
        {
            int l = beta.y + 3;
            return alfa + l;
        }

        public class A
        {
            public C c;
        }

        public class B
        {
            public int y;
        }
        public class C
        {
            public D d;
        }
        public class D
        {
            public E e;
        }
        public class E
        {
            public int last;
        }
    }
}
