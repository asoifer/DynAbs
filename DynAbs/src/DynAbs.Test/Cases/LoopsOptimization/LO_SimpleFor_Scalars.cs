using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynAbs.Test
{
    public class LO_SimpleFor_Scalars
    {
        static void Main()
        {
            var i = 0;
            while (i < 50000)
                i++;
            var c = i;
            return;
        }
    }
}
