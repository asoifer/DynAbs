using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynAbs.Test.Cases.Solver
{
    class ReachableSimple
    {
        public static void Main(string[] args)
        {
            int CANT = 100;

            var v1 = new B();
            var v2 = v1;
            for (int i = 0; i < CANT; i++)
            {
                v1 = new B(v1);
            }

            for (int i = 0; i < CANT; i++)
            {
                Console.WriteLine(v2);
            }
            return;
        }
       
        class B
        {
            public B miProperty;

            public B(){}

            public B(B _b)
            {
                this.miProperty = _b;
            }
        }

    }
}
