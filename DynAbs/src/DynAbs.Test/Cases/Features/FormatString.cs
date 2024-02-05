using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynAbs.Test.Cases
{
    public class FormatString
    {
        static void Main(string[] args)
        {
            decimal variable = 11;
            String s = String.Format("The current price is {0} per ounce.", variable);
            string a = "first" + s;
            return;
        }

    }
}