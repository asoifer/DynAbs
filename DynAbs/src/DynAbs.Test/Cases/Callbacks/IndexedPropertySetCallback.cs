using ExternalLibraryExample;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynAbs.Test.Cases
{
    class IndexedPropertySetCallback
    {
        static void Main()
        {
            A a = new A();
            B b = new B();
            Binary binary = new Binary(a);
            binary[1] = 5;
            binary["javier"] = 4;
            b[4] = 9;
            int j = binary[1] + binary["javier"] + b[4];
        }

        class A : IFramework1R
        {
            public object Callback(object arg)
            {
                return 3 + (int)arg;
            }
        }

        class B
        {
            public int thisProp = 7;
            public int this[int key]
            {
                get
                {
                    return thisProp + key;
                }
                set
                {
                    thisProp = value + key;
                }
            }
        }
    }
}
