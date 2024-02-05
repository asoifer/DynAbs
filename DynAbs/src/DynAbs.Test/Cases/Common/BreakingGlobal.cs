using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynAbs.Test.Cases
{
    public static class Global
    {
        public static bool isFlag = true;
    }

    class BreakingGlobal
    {
        public static void Main(string[] args)
        {
            int j = 3;
            if (Global.isFlag)
            {
                j = j + 1;
            }
            int valor = j;
        }
    }
}
