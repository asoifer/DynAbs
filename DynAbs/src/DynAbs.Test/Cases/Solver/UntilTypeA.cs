using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynAbs.Test.Cases.Solver
{
    public class UntilTypeA
    {
        static void Main()
        {
            var a = new A();
            a.b = new B();
            a.b.c = new C();

            a.x = 1;
            a.b.y = 1;
            a.b.c.z = 1;

            UntilTypeA_ExternalClass.UpdateFieldYOfTypeB(a);
            var slicingVariable1 = a.x;
            var slicingVariable2 = a.b.y;
            var slicingVariable3 = a.b.c.z;
            return;
        }

        public class A
        {
            public B b { get; set; }
            public int x { get; set; }
        }

        public class B
        {
            public C c { get; set; }
            public int y { get; set; }
        }

        public class C
        {
            public int z { get; set; }
        }
    }

    class UntilTypeA_ExternalClass
    {
        public static void UpdateFieldYOfTypeB(UntilTypeA.A a)
        {
            a.b.y = 1;
        }
    }
}
