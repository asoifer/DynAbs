using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynAbs.Test.Cases.Common
{
    static class ArrayMethodExtension
    {
        public static void Main()
        {
            var arr = new[] { 2, 3, 7, 5, 9 };
            var a = arr.Index(2);
        }
        public static T Index<T>(this T[] array, int i)
        {
            return array[i];
        }
    }
}
