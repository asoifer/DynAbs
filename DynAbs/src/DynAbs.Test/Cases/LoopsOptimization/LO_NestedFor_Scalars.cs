using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynAbs.Test
{
    class LO_NestedFor_Scalars
    {
        static void Main()
        {
            var acum = 0;
            for (var i = 0; i < 10; i++)
                for (var j = 0; j < 10; j++)
                    acum += i * j;
            var r = acum;
            return;
        }
    }
}
