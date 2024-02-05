using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynAbs.Test.Cases
{
    class NullableTypes
    {
        public static void Main(string[] args)
        {
            int? num = 1;
            if (num != null)
            {
                int a = (int) num;
            }
            return;
        }
    }
}
