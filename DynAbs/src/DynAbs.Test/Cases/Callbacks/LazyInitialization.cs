using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynAbs.Test.Cases
{
    class LazyInitialization
    {
        static Lazy<A> aInstance = new Lazy<A>(() => new A());

        static void Main()
        {
            var t = aInstance.Value;
            return;
        }

        class A
        {

        }
    }
}
