using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DynAbs.Test.Cases.Forms
{
    class InitializeComponentV2
    {
        public static void Main()
        {
            var myForm = new Inicial();
        }

        class Inicial : Form
        {
            // Controles
            //private System.Windows.Forms.Button button1;
            //private System.Windows.Forms.Label label1;
            private System.Windows.Forms.MenuStrip menu1;
            private System.Windows.Forms.ToolStripMenuItem tsmi1;
            private System.Windows.Forms.ToolStripMenuItem tsmi2;
            //private System.Windows.Forms.ToolStripMenuItem tsmi3;
            //private System.Windows.Forms.ToolStripMenuItem tsmi4;
            //private System.Windows.Forms.ToolStripMenuItem tsmi5;
            //private System.Windows.Forms.ToolStripMenuItem tsmi6;

            public Inicial()
            {
                InitializeComponent();
            }

            // Método inicializador
            public void InitializeComponent()
            {
                //this.button1 = new System.Windows.Forms.Button();
                //this.label1 = new System.Windows.Forms.Label();
                this.menu1 = new System.Windows.Forms.MenuStrip();
                this.tsmi1 = new System.Windows.Forms.ToolStripMenuItem();
                this.tsmi2 = new System.Windows.Forms.ToolStripMenuItem();
                //this.tsmi3 = new System.Windows.Forms.ToolStripMenuItem();
                //this.tsmi4 = new System.Windows.Forms.ToolStripMenuItem();
                //this.tsmi5 = new System.Windows.Forms.ToolStripMenuItem();
                //this.tsmi6 = new System.Windows.Forms.ToolStripMenuItem();

                //this.SuspendLayout();
                //var controls = this.Controls;
                //controls.Add(this.label1);
                //this.Controls.Add(this.button1);
                //this.Name = "Inicial";
                //this.Text = "Inicial";

                var temp = new System.Windows.Forms.ToolStripItem[] {
                    this.tsmi1,
                    this.tsmi2
                    //,this.tsmi3,
                    //this.tsmi4,
                    //this.tsmi5,
                    //this.tsmi6
                };
                this.menu1.Items.AddRange(temp);

                //this.Controls.Add(this.menu1);
                //this.menu1.ResumeLayout(false);
                //this.menu1.PerformLayout();

                //this.ResumeLayout(false);
                //this.PerformLayout();
            }
        }
    }
}
