using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynAbs.Test.Cases
{
    class AliasingGlobalWithConstructor
    {
        static AA GlobalVariable = new AA();
        public static void Main(string[] args)
        {
            int x = 3;
            AliasingGlobalWithConstructor.GlobalVariable.GV = x;
            int y = GlobalVariable.GV;
        }
        public class AA
        {
            public int GV;
            public AA()
            {
                Console.WriteLine("Constructor de AA");
            }
        }
    }
}
