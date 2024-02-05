using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynAbs.Test.Cases.Roslyn
{
    class CallWithNamedArgument
    {
        public static void Main(string[] args)
        {
            int valuei = 1;
            int valuej = 2;
            Method(i:valuei, j:valuej);
            // value is now 44
        }


        static void Method(int i, int j)
        {
            j = i;
            Console.WriteLine(j);
        }
    }
}
