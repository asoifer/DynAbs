using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynAbs.Test.Cases
{
    class NullCoalescingOperator
    {
        public static void Main(string[] args)
        {
            int? x = 2;
            int p = 2;
            //Setea y con el valor de x si es distinto de null, sino con p
            int y = x ?? p;
            y = y + 1;
        }
    }
}
