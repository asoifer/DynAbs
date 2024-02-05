using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynAbs
{
    public class CustomStack<T> : IEnumerable
    {
        List<T> l = new List<T>();
        public int Count { get { return l.Count; } }
        public IEnumerator GetEnumerator()
        {
            return l.GetEnumerator();
        }
        public T Peek()
        {
            return l[l.Count - 1];
        }
        public T Pop()
        {
            var r = Peek();
            l.RemoveAt(l.Count - 1);
            return r;
        }
        public void Push(T item)
        {
            l.Add(item);
        }

        public T GetAt(int index)
        {
            return l[index];
        }
    }
}
