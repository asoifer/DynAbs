using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace DynAbs.Test.Cases.Common
{
    public class StructsWithArrayAccess
    {
        static void Main()
        {
            var a1 = new ImmutableArray<int>();
            var a2 = new ImmutableArray<int>();

            var b1 = new int[] { 1, 2, 3 };
            var b2 = new int[] { 1, 2, 3 };

            a1.array = b1;
            a2.array = b2;
            var v = a1.SetEquals<int>(a2);
            var x = v;
            return;
        }

        public struct ImmutableArray<T> : IEnumerable<T>
        {
            public T[] array { get; set; }

            public T this[int index] 
            { 
                get 
                { 
                    return array[index]; 
                } 
            }
            public int Length => array.Length;
            public bool IsDefault => false;

            public IEnumerator<T> GetEnumerator()
            {
                foreach (var a in array)
                    yield return a;
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return array.GetEnumerator();
            }
        }
    }

    public static class StrucWithArrayAccessExtensions
    {
        public static bool SetEquals<T>(this StructsWithArrayAccess.ImmutableArray<T> array1, StructsWithArrayAccess.ImmutableArray<T> array2)
        {
            if (array1.IsDefault)
            {
                return array2.IsDefault;
            }
            else if (array2.IsDefault)
            {
                return false;
            }

            var count1 = array1.Length;
            var count2 = array2.Length;

            if (count1 == 0)
            {
                return count2 == 0;
            }
            else if (count2 == 0)
            {
                return false;
            }
            else if (count1 == 1 && count2 == 1)
            {
                var item1 = array1[0];
                var item2 = array2[0];

                return item1.Equals(item2);
            }

            var set1 = new HashSet<T>(array1);
            var set2 = new HashSet<T>(array2);

            return set1.SetEquals(set2);
        }
    }
}
