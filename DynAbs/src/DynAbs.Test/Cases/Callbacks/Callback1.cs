using ExternalLibraryExample;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynAbs.Test.Cases
{
    class Callback1
    {
        public static void Main(string[] args)
        {
            Called called = new Called();
            Binary bin = new Binary(called);
            bin.performCallback1_Once_Returning();
            string a = called.Val;
        }

        public class Called : IFramework1R
        {
            public string Val { get; set; }
            public object Callback(object arg)
            {
                this.Val = "constante";
                return new Called();
            }
        }
    }
}
