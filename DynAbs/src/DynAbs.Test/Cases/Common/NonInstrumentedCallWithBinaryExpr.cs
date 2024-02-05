using ExternalLibraryExample;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynAbs.Test.Cases
{
    class NonInstrumentedCallWithBinaryExpr
    {
        public static void Main(string[] args)
        {
            IList<int> list = new List<int>();
            int b = 4;
            list.Add(b + 3);
            int y = list[0];
        }
    }
}
