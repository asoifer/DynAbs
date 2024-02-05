using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ExternalLibraryExample;

namespace DynAbs.Test.Cases
{
    class CallbackFromNonInstrumentedDestructor
    {
        public static void Main(string[] args)
        {
            Func();
            System.GC.Collect();
            int x = 0;
        }

        private static void Func()
        {
            A a = new A();
        }

        class A : Binary
        {
            protected override void Dispose(bool disposing)
            {
                Console.WriteLine("Ejecutando Dispose()");
            }
        }
    }
}
