using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynAbs.Test.Cases
{
    public class ImplicitCastOperator
    {
        static void Main(string[] args)
        {
            Digit dig = new Digit(7);
            //This call invokes the implicit "double" operator
            double num = dig;
            //This call invokes the implicit "Digit" operator
            Digit dig2 = num;
            var num2 = Method(dig2.val);
        }
        private static double Method(Digit d)
        {
            return d.val;
        }
        class Digit
        {
            public Digit(double d) { val = d; }
            public double val;
            public static implicit operator double(Digit d)
            {
                return d.val;
            }
            public static implicit operator Digit(double d)
            {
                return new Digit(d);
            }
        }
    }
}