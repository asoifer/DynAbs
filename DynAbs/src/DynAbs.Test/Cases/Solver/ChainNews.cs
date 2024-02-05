using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynAbs.Test.Cases.Solver
{
    class ChainNews
    {
        public static void Main(string[] args)
        {
           int CANT_NEWS = 50;
            
           var v1 = new B();
           for (int i = 0; i < CANT_NEWS; i++)
           {
               v1 = new B(v1);
           }
           return;
        }

        class B
        {
            public B miProperty;

            public B()
            {

            }

            public B(B _b)
            {
                this.miProperty = _b;
            }
        }
        
    }
}
