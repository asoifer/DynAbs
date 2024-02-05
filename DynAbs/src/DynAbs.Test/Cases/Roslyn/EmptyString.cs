using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynAbs.Test.Cases
{
    class EmptyString
    {
        public static void Main(string[] args)
        {
            string svacio = string.Empty;
            string slleno = "string";
            slleno = slleno + svacio;
        }
    }
}
