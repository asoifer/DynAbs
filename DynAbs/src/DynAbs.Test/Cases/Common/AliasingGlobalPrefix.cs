using System;
namespace DynAbs.Test.Cases
{
    class AliasingGlobalPrefix
    {
        static AA GlobalVariable = new AA();
        public static void Main(string[] args)
        {
            int x = 3;
            AliasingGlobalPrefix.GlobalVariable.GV = x;
            int y = GlobalVariable.GV;
        }
        public class AA
        {
            public int GV;
        }
    }
}
