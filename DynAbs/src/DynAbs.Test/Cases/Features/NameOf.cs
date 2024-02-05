using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynAbs.Test.Cases.Features
{
    class NameOf
    {
        string Pedro = "";

        static void Main()
        {
            var pepe = nameof(NameOf.Pedro);
            Console.WriteLine(pepe);
            return;
        }
    }
}
