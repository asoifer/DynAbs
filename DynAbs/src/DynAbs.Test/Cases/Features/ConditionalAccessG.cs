using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynAbs.Test.Cases
{
    class ConditionalAccessG
    {
        static void Main()
        {
            var t = new A();
            return;
        }

        class A
        {
            View previousFocused;

            void Pepe()
            {
                var temp = previousFocused?.SuperView?.IntVal;
                previousFocused?.SuperView?.SetFocus(previousFocused);
            }
        }

        class View
        {
            View container = null;
            int intVal = 0;

            public View SuperView => container;
            public int IntVal => intVal;

            public void SetFocus(View view)
            {
            }
        }
    }
}
