using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DynAbs.Test.Cases.Forms
{
    class FormWithForeachOfControls
    {
        public static void Main()
        {
            var myForm = new Inicial();
            RecursiveCustomization(myForm);
            return;
        }

        public static void RecursiveCustomization(Control control)
        {
            foreach(var c in control.Controls)
            {
                if (c is Control) 
                { 
                    RecursiveCustomization((Control)c);
                    Customize((Control)c);
                }
            }
        }

        public static void Customize(Control control)
        {
            if (control is TextBox)
                ((TextBox)control).Text = "myText";
            if (control is Label)
                ((Label)control).Text = "myText";
        }

        class Inicial : Form
        {
            // Controles
            private System.Windows.Forms.Button button1;
            private System.Windows.Forms.Label label1;

            public Inicial()
            {
                InitializeComponent();
            }

            // Método inicializador
            public void InitializeComponent()
            {
                this.button1 = new System.Windows.Forms.Button();
                this.label1 = new System.Windows.Forms.Label();
                
                this.SuspendLayout();
                
                this.Controls.Add(this.label1);
                this.Controls.Add(this.button1);
                
                this.Name = "Inicial";
                this.Text = "Inicial";
                
                this.ResumeLayout(false);
                this.PerformLayout();
            }
        }
    }
}
