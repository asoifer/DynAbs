using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynAbs.Test.Cases
{
    class ScalarForWithDeclaration
    {
        public static void Main(string[] args)
        {
            int suma = 0;
            for (int i = 0; i < 10; i = i + 1)
            {
                suma = suma + i;
            }
            int valor = suma;
        }
    }
}
