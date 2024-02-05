#define MAKE_IT_RUN

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynAbs.Test.Cases
{
    class PreprocessorDirectiveInstrumentation
    {
        public static void Main(string[] args)
        {
            /* TODO: Hay que verificar bien como esta instrumentando, comento donde vimos 
             * que ponia el Trace */

            // Trace("object objeto = null")
            object objeto = null;
            // Trace("objecto = new Object") --> ERROR: Deberia estar dentro del #if.
            #if MAKE_IT_RUN
                objeto = new object();
                // Trace("Console.WriteLine(objeto)") --> ERROR: Deberia estar fuera del #if.
            #endif
            Console.WriteLine(objeto);
        }
    }
}
