using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynAbs.Test.Cases
{
    class TryStmtExceptionInProperty
    {
        public static void Main(string[] args)
        {
            A a = new A();
            string s = null;
            try
            {
                s = a.Prop;
            }
            catch(Exception e)
            {
                s = e.ToString();
            }
            string x = s;
        }
        class A
        {
            public string Prop
            {
                get
                {
                    throw new Exception("Property con excepcion!");
                }
            }
        }
    }
}
