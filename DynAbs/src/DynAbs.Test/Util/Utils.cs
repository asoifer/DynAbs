using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynAbs.Test
{
    static class Utils
    {
        public static int[] GetRange(int a, int b)
        {
            var tmp = new List<int>();
            for (var i = a; i <= b; i++)
                tmp.Add(i);
            return tmp.ToArray();
        }
    }
}
