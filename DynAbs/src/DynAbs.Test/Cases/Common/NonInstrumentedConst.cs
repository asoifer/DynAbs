using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynAbs.Test.Cases
{
    class NonInstrumentedConst
    {
        static void Main()
        {
            var a = new A();
            return;
        }

        class A
        {
            private const int DATAGRIDVIEWNUMERICUPDOWNCELL_defaultRenderingBitmapWidth = 100;
            private const int DATAGRIDVIEWNUMERICUPDOWNCELL_defaultRenderingBitmapHeight = 22;

            public A()
		    {
                Console.WriteLine(DATAGRIDVIEWNUMERICUPDOWNCELL_defaultRenderingBitmapWidth);
		    }
        }
    }
}
