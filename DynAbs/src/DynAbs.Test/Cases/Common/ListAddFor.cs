using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynAbs.Test.Cases
{
    class ListAddFor
    {
        public static void Main()
        {
            var lista = new List<int>();
            for (var i = 0; i < 10; i++)
                lista.Add(1);
            return;
        }
    }
}
