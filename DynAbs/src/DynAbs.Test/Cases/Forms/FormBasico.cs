using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DynAbs.Test.Cases.Forms
{
    class FormBasico
    {
        public static void Main()
        {
            var myForm = new MyForm();
        }

        class MyForm : Form
        {
            private System.Windows.Forms.Button button1;

            public MyForm()
            {
                this.button1 = new System.Windows.Forms.Button();
                this.SuspendLayout();
                this.Controls.Add(button1);
                this.ResumeLayout(false);
            }
        }

    }
}
