using ExternalLibraryExample;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynAbs.Test.Cases
{
    class IndexedPropertyCallback
    {
        static void Main()
        {
            A a = new A();
            var i = a.Method();
            var j = i + 4;
        }

        class A : IFramework1R
        {
            public int Method()
            {
                int[] arr = new int[1] { 1 };
                Binary b = new Binary(this);
                return b[1] + arr[0];
            }

            public object Callback(object arg)
            {
                return 3 + (int)arg;
            }
        }
    }
}
