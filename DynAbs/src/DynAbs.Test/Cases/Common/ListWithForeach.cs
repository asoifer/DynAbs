using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynAbs.Test.Cases
{
    class ListWithForeach
    {
        static void Main(string[] args)
        {
            IList<int> list = new List<int>();
            list.Add(1);
            foreach (var a in list)
                ;
            list.Count();
        }
    }
}
