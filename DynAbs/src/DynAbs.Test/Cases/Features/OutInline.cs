using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynAbs.Test.Cases.Features
{
    class OutInline
    {
        static void Main()
        {
            Aux(out string s);
            Console.WriteLine(s);
            return;
        }

        static void Aux(out string a)
        {
            a = "";
            return;
        }
    }
}
