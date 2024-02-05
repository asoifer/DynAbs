using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynAbs.Test.Cases
{
    class LinqQueryToListWithCallBack
    {
        static void Main(string[] args)
        {
            IList<int> list = new List<int>();
            list.Add(1);
            Modifier y = new Modifier();
            Modifier m = new Modifier();
            IList<int> mapApplied = list.Select(x => m.modify(x,y)).ToList();
            int z = mapApplied.First();
        }
        public class Modifier
        {
            public int f = 1;
            public int modify(int x, Modifier y)
            {
                return x + y.f;
            }
        }
    }
}
