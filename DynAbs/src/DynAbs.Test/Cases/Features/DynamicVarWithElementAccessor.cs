using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynAbs.Test.Cases
{
    class DynamicVarWithElementAccessor
    {
        static void Main()
        {
            dynamic v = new A();
            int intValue = v.intValues[v.indexValue];
            var sliceVariable = intValue;
        }

        class A
        {
            public List<int> intValues = new List<int>() { 0, 1 };
            public int indexValue = 0;
        }
    }
}
