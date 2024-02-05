using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynAbs.Test.Cases.Features
{
    class NewWithNoArgumentsAndInitializersCollection
    {
        public static void Main(string[] args)
        {
            string element1 = "Christian";
            string element2 = "Alexis";
            var list = new List<string> {element1, element2 };
        }
    }
}
