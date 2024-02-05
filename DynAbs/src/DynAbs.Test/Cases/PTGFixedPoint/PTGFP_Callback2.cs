using ExternalLibraryExample;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynAbs.Test
{
    class PTGFP_Callback2
    {
        public static void Main(string[] args)
        {
            for (var i = 0; i < 5; i++)
            {
                Called called = new Called();
                Binary bin = new Binary(called);
                bin.performCallback2(called);
                string a = called.Val;
            }
        }

        public class Called : IFramework2
        {
            public string Val { get; set; }
            public object Callback1(object arg)
            {
                this.Val = "constante";
                return new Called();
            }
            public BasicClass Callback2(object arg)
            {
                return new A();
            }
        }

        class A : BasicClass
        {

        }
    }
}
