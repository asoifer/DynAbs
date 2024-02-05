using ExternalLibraryExample;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynAbs.Test.Cases.Features
{
    class NonInstrumentedNonScalarOutParameter
    {
        static void Main()
        {
            BasicClass local;
            BasicClass.Initialize(out local);
            var g = local.child.f;
        }
    }
}
