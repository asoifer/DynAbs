using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynAbs.Test.Cases
{
    public class GlobalTwiceEntry
    {
        static int GlobalVariable = 11;
        static int GV2;
        public static void Main(string[] args)
        {
            int x = 3;
            //GlobalTwiceEntry.GlobalVariable = x;
            GlobalTwiceEntry.GV2 = 25;
            int y = GlobalVariable;
            int j = GlobalTwiceEntry.GV2;
        }
        public class Prueba2
        {
            static int Cuenta = 0;
        }
    }
}
