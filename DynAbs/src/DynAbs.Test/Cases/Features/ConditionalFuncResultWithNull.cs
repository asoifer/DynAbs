using System;
using System.Collections;
using System.Collections.Generic;

namespace DynAbs.Test.Cases.Features
{
    public class ConditionalFuncResultWithNull
    {
        public static void Main()
        {
            var array = new CustomArray<int>(new int[] { 1, 2 });
            Func<int, ConditionalFuncResultWithNull.CustomArray<int>> myFunc = TrySomething<int>;
            var r = array.DoSomethingForAll<int>(myFunc);
            Console.WriteLine(r);
            return;
        }

        public static ConditionalFuncResultWithNull.CustomArray<T> TrySomething<T>(T elem)
        {
            return null;
        }

        public class CustomArray<T> : IEnumerable<T>
        {
            T[] elements;

            public CustomArray(T[] elements)
            {
                this.elements = elements;
            }

            public IEnumerator<T> GetEnumerator()
            {
                foreach (var elem in elements)
                    yield return elem;
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return elements.GetEnumerator();
            }
        }
    }

    public static class MyExtensionsForCustomArray_B
    {
        public static int? DoSomethingForAll<T>(this ConditionalFuncResultWithNull.CustomArray<T> array, Func<T, ConditionalFuncResultWithNull.CustomArray<T>> getNested)
        {
            var total = 0;
            foreach (var item in array)
            {
                var temp = getNested(item)?.DoSomethingForAll(getNested);
                if (temp.HasValue)
                    total += temp.Value;
            }
            return total;
        }
    }
}
