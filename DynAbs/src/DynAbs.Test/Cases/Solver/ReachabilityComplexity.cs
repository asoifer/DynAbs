using ExternalLibraryExample;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynAbs.Test.Cases.Solver
{
    class ReachabilityComplexity
    {
        public static void Main(string[] args)
        {
            int CANT_NEWS = 10;

            Persona miPersona = new Persona();
            List<TimeSpan> evolution = new List<TimeSpan>();
            
            for (int i = 0; i < CANT_NEWS; i++)
                Binary.Test(miPersona, new Casa());
        }

        class Casa
        {
            public Ventana laVentana = new Ventana();
        }

        class Ventana
        {

        }

        class Persona
        {

        }
    }


}
