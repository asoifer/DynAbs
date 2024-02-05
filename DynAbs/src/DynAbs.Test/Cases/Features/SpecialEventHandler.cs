using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynAbs.Test.Cases
{
    class SpecialEventHandler
    {
        static void Main()
        {
            var a = new A();
            return;
        }

        class A
        {
            BackgroundWorker backgroundWorker = new System.ComponentModel.BackgroundWorker();

            public A()
            {
                backgroundWorker.DoWork += new DoWorkEventHandler(doWork);
            }

            void doWork(object sender, DoWorkEventArgs e)
            {
                
            }
        }
    }
}
