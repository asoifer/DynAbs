using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynAbs.Test.Cases
{
    class InlineNewInstrumented
    {
        public static void Main(string[] args)
        {
            string inicial = "va un inicial";
            Beta bb = Run(new Alpha(inicial));
            string s = bb.Str;
        }

        public static Beta Run(Alpha alpha)
        {
            Beta b2 = new Beta();
            b2.Str = alpha.Guardado;
            return b2;
        }

        public class Alpha { 
            
            public string Guardado;
            public Alpha(string ss)
            {
                Guardado = ss;
            }
        }

        public class Beta { public string Str; }
    }
}
