using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynAbs.Test.Cases.TryCatch
{
    class NullPointerException
    {
        public static void Main()
        {
            string algo = "No hubo error";
            try 
            { 
                A myList = null;
                var contador = myList.myField;
            }
            catch(Exception ex)
            {
                algo = "Hubo error";
            }
            var lineaASlicear = algo;
        }

        public class A
        {
            public int myField;
        }
    }
}
