using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DynAbs.Test.Cases
{
    class ComplexBaseConstructor
    {
        static void Main()
        {
            var prueba = new FormExample.MyDataGridViewColumn();
            return;
        }

        class FormExample 
        {
            private MyDataGridViewColumn unaProperty;

            public FormExample() 
            {
                this.unaProperty = new MyDataGridViewColumn();
            }

            public class MyDataGridViewColumn : DataGridViewColumn
            {
                public MyDataGridViewColumn()
                    : base(new MyDataGridViewCell())
                {
                }
            }

            public class MyDataGridViewCell : DataGridViewTextBoxCell
            {
                public MyDataGridViewCell()
                {
                }
            }
        }
    }
}
