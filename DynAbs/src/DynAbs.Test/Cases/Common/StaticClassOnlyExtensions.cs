// Initial comment and license

#nullable disable

using System.Collections.Generic;
using System.Linq;

namespace DynAbs.Test.Cases.Common
{
    class StaticClassOnlyExtensions
    {
        static void Main()
        {
            var l = new List<int>() { 1, 2, 3 };
            var c = l.ExtensionMethod1<int>();
            var s = l.ExtensionMethod2<int>();
            var d = s + c.ToString();
            return;
        }
    }

    public static class StaticClassOnlyExtensions_B
    {
        #region Region myRegion

        // A classical comment.

        public static int ExtensionMethod1<T>(this List<T> myList)
            => myList.Count;

        public static IEnumerable<string> ExtensionMethod2<T>(this List<T> myList)
            => myList == null ? null : myList.Select(s => $"C={s.ToString()}");
                
        #endregion
    }
}
