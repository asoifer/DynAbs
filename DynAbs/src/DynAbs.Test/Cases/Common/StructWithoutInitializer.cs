using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynAbs.Test.Cases.Common
{
    class StructWithoutInitializer
    {
        static void Main()
        {
            Point myPoint;
            myPoint.X = 1;
            myPoint.Y = (short)(myPoint.X + (short)1);
            var res = myPoint.Y;
            return;
        }

        internal struct Point
        {
            internal short X;

            internal short Y;

            public override string ToString()
            {
                return string.Format("{0},{1}", X, Y);
            }
        }
    }
}
