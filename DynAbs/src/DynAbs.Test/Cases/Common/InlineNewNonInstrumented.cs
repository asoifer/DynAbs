using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynAbs.Test.Cases
{
    class InlineNewNonInstrumented
    {
        public static void Main(string[] args)
        {
            Beta bb = Run(new Alpha());
        }

        public static Beta Run(Alpha alpha)
        {
            Beta b2 = new Beta();
            return b2;
        }

        public class Alpha { }

        public class Beta { public string Str; }
    }
}
