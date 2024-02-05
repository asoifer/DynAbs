using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynAbs.Test.Cases
{
    class ContinueInLoopInsideIf
    {
        public static void Main(string[] args)
        {
            int i = 1;
            int x = 0;
            while (i > 0)
            {
                x++;
                i--;
                if (i == 0)
                {
                    continue;
                    // Si no se hace nada con el continue, lo último que queda popeado es la condicion
                    // del if, entonces el proximo stmt va a depender del if.
                }
            }
            x = 2;
            int y = x;
        }
    }
}
