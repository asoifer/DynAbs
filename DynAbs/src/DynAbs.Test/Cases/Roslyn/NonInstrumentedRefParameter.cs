using ExternalLibraryExample;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynAbs.Test.Cases
{
    class NonInstrumentedRefParameter
    {
        public static void Main(string[] args)
        {
            var a = new object();
            Binary b = new Binary();
            b.Test(ref a);
            object d = a;
        }
    }
}
