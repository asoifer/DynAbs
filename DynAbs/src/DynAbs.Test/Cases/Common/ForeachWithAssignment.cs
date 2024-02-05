using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynAbs.Test.Cases
{
    class ForeachWithAssignment
    {
        static void Main(string[] args)
        {
            IList<A> list = new List<A>();
            A a1 = new A(1);
            list.Add(a1);
            foreach (A a in list)
            {
                a1 = a;
            }
            list.Count();
            var a2 = a1;
        }
        public class A
        {
            public A(int i)
            {
                Number = i;
            }
            public int Number; // { get; set; }
        }
    }
}
