using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynAbs.Test.Cases
{
    class ArrayMultipleLastDef
    {
        public static void Main(string[] args)
        {
            int[] varios = new int[10];
            varios[0] = 55;
            varios[1] = 21;
            varios[7] = 99;
            varios[8] = 140;
            int j = varios[6];
        }
    }
}
