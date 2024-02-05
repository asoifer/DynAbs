using ExternalLibraryExample;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DynAbs.Test.Cases.Forms
{
    public static class ColorBehaviour
    {
        public static void Main()
        {
            var myButton = new Button();
            myButton.BackColor = System.Drawing.SystemColors.ControlDark;
            var binary = new Binary();
            binary.SimpleExternalMethod(myButton);

            var myButton2 = new Button();
            myButton2.BackColor = System.Drawing.SystemColors.ControlDark;
            var binary2 = new Binary();
            binary2.SimpleExternalMethod(myButton2);
        }
    }
}
