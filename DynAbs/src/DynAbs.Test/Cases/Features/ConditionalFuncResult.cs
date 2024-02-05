using System;
using System.Collections;
using System.Collections.Generic;

namespace DynAbs.Test.Cases.Features
{
    public class ConditionalFuncResult
    {
        public static void Main()
        {
            var array = new CustomArray<int>(new int[] { 1, 2 });
            Func<int, ConditionalFuncResult.CustomArray<int>> myFunc = TrySomething<int>;
            var r = array.DoSomethingForAll<int>(myFunc);
            Console.WriteLine(r);
            return;
        }

        public static ConditionalFuncResult.CustomArray<T> TrySomething<T>(T elem)
        {
            return new CustomArray<T>(new T[] { });
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

    public static class MyExtensionsForCustomArray
    {
        public static int? DoSomethingForAll<T>(this ConditionalFuncResult.CustomArray<T> array, Func<T, ConditionalFuncResult.CustomArray<T>> getNested)
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
