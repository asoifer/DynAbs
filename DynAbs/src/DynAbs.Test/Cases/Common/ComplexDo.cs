using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynAbs.Test.Cases.Common
{
    // Este test está dedicado al Instrumentador, que fallaba en un caso similar.
    class ComplexDo
    {
        public static void Main()
        {
            string Search = "dsda";
            int Result = 7;
            do
            {
                Result--;

                if (Result < 0) Search = null;

                else if (Result > 0) Search = "sdasd";

                else break;

            } while (Search != null);
            return;
        }
    }
}
