using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynAbs.Test.Cases
{
    class RefSimpleInvocation
    {
        static void Main()
        {
            var a = new A();
            TestMethod(ref a.propB);
            var t = a.propB.myField;
        }

        class A
        {
            public B propB = new B();
        }

        class B
        {
            public int myField = 0;
            public B()
            {

            }
            public B(int initialValue)
            {
                myField = initialValue;
            }
        }

        static void TestMethod(ref B reference)
        {
            reference = new B(1);
        }
    }
}
