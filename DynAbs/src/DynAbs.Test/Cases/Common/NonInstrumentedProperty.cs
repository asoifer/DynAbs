using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DynAbs.Test.Cases
{
    class NonInstrumentedProperty
    {
        public static void Main(string[] args)
        {
            CultureInfo ñandu = new CultureInfo("es-AR");
            Thread.CurrentThread.CurrentUICulture = ñandu;
            CultureInfo ñato = Thread.CurrentThread.CurrentUICulture;
        }
    }
}
