using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynAbs.Test.Cases
{
    class SlicingSimpleCallFirstStmtIf
    {
        static void Main(string[] args)
        {
            int valor = 52;
            int valor2 = 39;
            int resultado = Suma(valor, valor2);
            int j = resultado;
            return;
        }

        static int Suma(int a, object b)
        {
            if (a == 0)
            {
                return a;
            }
            return 5;
        }
    }
}
