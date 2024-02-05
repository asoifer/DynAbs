using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynAbs.Test.Cases
{
    class SpecialParametricType
    {
        static void Main()
        {
            var list = new InternalCollection<A>();
            list.Add(new A() { Name = "Pedro" });
            list.Add(new A() { Name = "Turkish" });
            var list_b = Match<A>(list, "Turkish");
            Console.WriteLine(list_b.Count);
            return;
        }

        static InternalCollection<T> Match<T>(InternalCollection<T> memberList, string name) where T : A
        {
            InternalCollection<T> returnValue = new InternalCollection<T>();
            foreach (T member in memberList)
            {
                if (name == member.Name)
                {
                    returnValue.Add(member);
                }
            }
            return returnValue;
        }

        class A
        {
            public string Name { get; set; }
        }

        class InternalCollection<T> : ICollection<T>, IEnumerable<T> where T : A
        {
            List<T> internalList = new List<T>();
            public int Count => internalList.Count;

            public bool IsReadOnly => false;

            public void Add(T item)
            {
                internalList.Add(item);
            }

            public void Clear()
            {
                internalList.Clear();
            }

            public bool Contains(T item)
            {
                return internalList.Contains(item);
            }

            public void CopyTo(T[] array, int arrayIndex)
            {
                throw new NotImplementedException();
            }

            public IEnumerator<T> GetEnumerator()
            {
                return internalList.GetEnumerator();
            }

            public bool Remove(T item)
            {
                return internalList.Remove(item);
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return internalList.GetEnumerator();
            }
        }

    }
}
