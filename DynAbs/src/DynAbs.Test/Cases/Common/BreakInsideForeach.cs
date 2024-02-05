using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynAbs.Test.Cases
{
    class BreakInsideForeach
    {
        public static void Main(string[] args)
        {
            var list = new List<int>() { 1, 2, 3, 4 };
            int sum = 0;
            if (true)
            {
                foreach (var num in list)
                {
                    sum = sum + num;
                    if (sum > 3)
                    {
                        break;
                    }
                }
                // Al momento del test, el break hace más pop de los que debe porque no encuentra
                // el Foreach, esto causa que en este lugar, al hacer el Exit del if(true) explote.
            }
            int y = 2;
        }
    }
}
