using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynAbs.Test.Cases
{
    class NonInstrumentedCallWithStringParam
    {
        public static void Main(string[] args)
        {
            IList<string> list = new List<string>();
            int n = 3;
            int i = 0;
            string aux = "aux";
            while (i <= n)
            {
                list.Add(aux);
                i++;
            }
            string y = list[0];
        }
    }
}
