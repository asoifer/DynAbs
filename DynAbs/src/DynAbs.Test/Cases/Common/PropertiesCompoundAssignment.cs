using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynAbs.Test.Cases.Common
{
    class PropertiesCompoundAssignment
    {
        static void Main()
        {
            var a = new A();
            var a2 = new A();
            a.b = 1;
            a2.SetB(a);
            var c = a2.b;
            return;
        }

        class A
        {
            int _b;
            public int b
            {
                get
                {
                    return _b;
                }
                set
                {
                    _b = value;
                }
            }

            public void SetB(A other)
            {
                this.b += other.b;
            }
        }
    }
}
