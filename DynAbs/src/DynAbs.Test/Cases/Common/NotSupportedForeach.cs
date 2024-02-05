using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynAbs.Test.Cases
{
    class NotSupportedForeach
    {
        static void Main(string[] args)
        {
            IList<A> list = new List<A>();
            A a1 = new A(1);
            A a2 = new A(2);
            A a3 = new A(3);
            list.Add(a1);
            list.Add(a2);
            list.Add(a3);
            int y = 0;
            foreach (A a in list)
            {
                y = y + a.Number;
            }
            int z = y;
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
