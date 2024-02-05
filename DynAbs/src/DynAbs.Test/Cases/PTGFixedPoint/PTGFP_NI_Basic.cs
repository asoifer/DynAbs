using ExternalLibraryExample;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DynAbs.Test.Util;

namespace DynAbs.Test
{
    class PTGFP_NI_Basic
    {
        static void Main()
        {
            var l = new SimpleList<BasicClass>();
            for (var i = 0; i < 10; i++)
                l.Add(new BasicClass());
            var t = l.GetAt(0).f;
            return;
        }
    }
}
