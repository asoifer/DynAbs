using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynAbs.Test.Cases.TryCatch
{
    class OutOfRangeException
    {
        public static void Main()
        {
            string algo = "No hubo error";
            try
            {
                List<int> myList = new List<int>();
                var contador = myList[0];
            }
            catch (Exception ex)
            {
                algo = "Hubo error";
            }
            var lineaASlicear = algo;
        }
    }
}
