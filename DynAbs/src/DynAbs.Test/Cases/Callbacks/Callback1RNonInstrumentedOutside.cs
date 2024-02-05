using ExternalLibraryExample;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynAbs.Test.Cases
{
    class Callback1RNonInstrumentedOutside
    {
        public static void Main(string[] args)
        {
            Called called = new Called();
            Binary bin = new Binary(called);
            object argumento = new object();
            object resultado = Binary.Test(bin.performCallback1_Once_Returning(argumento));
            called.Val = "Valor";
        }

        public class Called : IFramework1R
        {
            public string Val { get; set; }
            public object Callback(object arg)
            {
                return arg;
            }
        }
    }
}
