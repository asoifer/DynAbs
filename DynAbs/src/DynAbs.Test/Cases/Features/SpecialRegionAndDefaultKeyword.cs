using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynAbs.Test.Cases.Features
{
    class SpecialRegionAndDefaultKeyword
    {
        static void Main()
        {
            var t = GetFileShares("", true);
            return;
        }

        internal static List<string> GetFileShares(string machine, bool ignoreHidden)
        {
#if UNIX
            return new List<string>();
#else
            IntPtr shBuf;
            var shares = new List<string>();
            return shares;
#endif
        }
    }
}
