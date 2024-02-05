using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynAbs.Test.Cases.Features
{
    class ReservedKeywords
    {
        static void Main()
        {
            var @int = 1;
            var @test = "Javo";
            Pepe(@int, @test);
            return;
        }

        static void Pepe(int @static, string @namespace)
        {
            if (@static > 0)
                Pepe2(@static: @static, @namespace: @namespace);
        }

        static void Pepe2(string @namespace, int @static)
        {
            Console.WriteLine(@namespace);
        }
    }
}
