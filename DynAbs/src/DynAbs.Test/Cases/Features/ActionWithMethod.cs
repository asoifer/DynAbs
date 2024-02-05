using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynAbs.Test.Cases
{
    class ActionWithMethod
    {
        public static void Main()
        {
            var aux = new AuxClass();
            return;
        }

        class AuxClass
        {
            public AuxClass()
            {
                var action = new Action<int>(this.TestMethod);
                action.Invoke(1);
            }

            public void TestMethod(int i)
            {

            }
        }
    }
}
