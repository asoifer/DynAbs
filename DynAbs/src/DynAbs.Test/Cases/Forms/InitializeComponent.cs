using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DynAbs.Test.Cases.Forms
{
    class InitializeComponent
    {
        public static void Main()
        {
            var myForm = new Inicial();
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
                var controls = this.Controls;
                controls.Add(this.label1);
                this.Controls.Add(this.button1);
                this.Name = "Inicial";
                this.Text = "Inicial";
                this.ResumeLayout(false);
                this.PerformLayout();
            }
        }
    }
}
