using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynAbs.Test.Cases.TryCatch
{
    class AritmeticException
    {
        public static void Main()
        {
            string algo = "No hubo error";
            try
            {
                Dividir(1, 0);
            }
            catch (Exception ex)
            {
                algo = "Hubo error";
            }
            var lineaASlicear = algo;
        }

        public static decimal Dividir(decimal a, decimal b)
        {
            var resultado = a / b;
            return resultado;
        }
    }
}
