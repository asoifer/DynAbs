using System;
using System.Collections.Generic;

namespace DynAbs.Test.Cases.Types
{
    class ForeachWithGenericCollection
    {
        static void Main()
        {
            var collection = new List<int>() { 1, 2, 3 };
            var count = DoNothing(collection);
            Console.WriteLine(count);
            return;
        }

        static int DoNothing<T>(List<T> collection)
        {
            var count = 0;
            foreach (var item in collection)
                count++;
            return count;
        }
    }
}
