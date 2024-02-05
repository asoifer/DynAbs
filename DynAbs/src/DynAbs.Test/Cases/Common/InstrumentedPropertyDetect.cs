using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynAbs.Test.Cases
{
    public class InstrumentedPropertyDetect
    {
        public static void Main(string[] args)
        {
            PruebaInstr p = new PruebaInstr();
            string otro = p.Valor;
        }

        public class PruebaInstr
        {
            public string Valor
            {
                get
                {
                    string s = "determinado valor";
                    return s;
                }
            }
            //public string Valor { get; set; }
        }
    }
}
