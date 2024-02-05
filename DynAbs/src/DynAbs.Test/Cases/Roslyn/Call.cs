using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynAbs.Test.Cases.Roslyn
{
    class Call
    {
        public static void Main(string[] args)
        {
            int numA = 4;
            int productA = Square(numA);
        }
        private static int Square( int i ) 
        {
            int input = i;
            return input * input;
        }
    }
}
