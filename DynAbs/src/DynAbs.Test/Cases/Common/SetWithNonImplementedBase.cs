using ExternalLibraryExample;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynAbs.Test.Cases
{
    class SetWithNonImplementedBase
    {
        public static void Main()
        {
            var aux = new AuxClass();
            var testClass = new TestClass();
            testClass.s = aux;
            var temp = testClass.s;
        }

        class TestClass : SimpleExternalClass
        {
            public override TestInterface s
            {
                get
                {
                    return base.s;
                }
                set
                {
                    base.s = value;
                }
            }
        }

        class AuxClass : TestInterface
        {

        }
    }
}
