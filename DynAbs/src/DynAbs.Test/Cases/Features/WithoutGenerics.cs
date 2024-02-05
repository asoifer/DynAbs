using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynAbs.Test.Cases
{
    class WithoutGenerics
    {
        public static void Main(string[] args)
        {
            ArrayList list = new ArrayList();

            list.Add(1);
            list.Add("foo");
            return;
        }
    }
}
