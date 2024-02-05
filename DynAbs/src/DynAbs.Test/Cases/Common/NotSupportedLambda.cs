using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynAbs.Test.Cases
{
    class NotSupportedLambda
    {
        static void Main(string[] args)
        {
            var list = new List<string>{};
            var newList = list.Where(a => a.Contains('a'));//cuando instrumenta esto trata de acceder a la variable libre a antes de su scope
        }
    }
}
