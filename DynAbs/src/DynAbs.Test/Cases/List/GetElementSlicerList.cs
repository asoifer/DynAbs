using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DynAbs.Test.Util;

namespace DynAbs.Test.Cases.List
{
    class GetElementSlicerList
    {
        public static void Main()
        {
            var sl = new SlicerList<int>();
            sl.Add(1);
            sl.Add(2);
            var j = sl[0];
        }
    }
}
