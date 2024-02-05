using ExternalLibraryExample;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynAbs.Test.Cases
{
    class OverrideToStringTest
    {
        public static void Main(string[] args)
        {
            Cliente c = new Cliente();
            string a = "asda" + c + c.Val;
        }
    }

    public class Cliente
    {
        public string Val { get; set; }
        public override string ToString()
        {
            this.Val = "hola";
            return "mi string";
        }
    }
}
