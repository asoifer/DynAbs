using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynAbs.Test.Cases
{
    class CallDestructorFromGarbage
    {
        /* Puede suceder que el destructor ejecute una funcion, que por
         * usos y costumbres suele ser Dispose(). */
        public static void Main(string[] args)
        {
            Func();
            int x = 2;
        }

        private static void Func()
        {
            A a = new A();
        }

        class A : IDisposable
        {
            ~A()
            {
                Dispose();
            }

            public void Dispose()
            {
                Console.WriteLine("Ejecutando Dispose()");
            }
        }
    }
}
