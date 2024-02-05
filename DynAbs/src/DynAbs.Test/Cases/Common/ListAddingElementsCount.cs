using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynAbs.Test.Cases
{
    class ListAddingElementsCount
    {
        static void Main(string[] args)
        {
            IList<int> list = new List<int>();
            list.Add(1);
            list.Add(2);
            list.Add(3);
            list.Count();
        }
    }
}
