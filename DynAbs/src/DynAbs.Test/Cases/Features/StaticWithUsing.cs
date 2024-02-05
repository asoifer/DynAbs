using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynAbs.Test.Cases
{
    using Dbg = StaticWithUsing.Diagnostics;
    public class StaticWithUsing
    {
        
        static void Main()
        {
            var l = new List<string>();
            Dbg.Assert(l != null, "list may not be null");
            var x = l.Count;
            return;
        }

        public class Diagnostics
        {
            internal static void Assert(bool condition, string whyThisShouldNeverHappen)
            {
                Diagnostics.Assert(condition, whyThisShouldNeverHappen, String.Empty);
            }

            internal static void Assert(bool condition, string whyThisShouldNeverHappen, string detailMessage)
            {
                if (condition) 
                    return;

                string assertionMessage = "ASSERT: " + whyThisShouldNeverHappen + "  " + detailMessage + " ";
                throw new Exception(assertionMessage);
            }
        }
    }
}
