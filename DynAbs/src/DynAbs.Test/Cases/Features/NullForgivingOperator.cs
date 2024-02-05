using System;
using System.Collections.Generic;
using System.Linq;

namespace DynAbs.Test.Cases.Features
{
    class NullForgivingOperator
    {
        static void Main()
        {
            List<int>? list = new List<int>() { 1, 2, 3 };
            var x = list!.Count;
            var y = list!.Count();
            var z = x + y;
        }
    }
}
