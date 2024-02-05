using ExternalLibraryExample;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynAbs.Test.Cases
{
    class Callback1RInstrumentedOutside
    {
        public static void Main(string[] args)
        {
            Called called = new Called();
            Binary bin = new Binary(called);
            object argumento = new object();
            object resultado = InvokeSomething(bin.performCallback1_Once_Returning(argumento));
            called.Val = "Valor";
        }

        public static object InvokeSomething(object paramInstrumented)
        {
            return paramInstrumented;
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
