using ExternalLibraryExample;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynAbs.Test.Cases
{
    class TryStmtExceptionInNonInsProperty
    {
        public static void Main(string[] args)
        {
            Binary a = new Binary();
            string s = null;
            try
            {
                s = a.PropertyWithThrow;
            }
            catch(Exception e)
            {
                s = e.ToString();
            }
            string x = s;
        }
    }
}
