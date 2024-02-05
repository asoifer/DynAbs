using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynAbs.Test.Cases
{
    class ScalarSimpleFor
    {
        public static void Main(string[] args)
        {
            int suma = 0;
            int i = 0;
            for (; i < 1; i = i + 1)
            {
                suma = suma + i;
            }
            int valor = suma;
        }
    }
}
