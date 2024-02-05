using ExternalLibraryExample;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynAbs.Test.Cases
{
    class Callback0
    {
        public static void Main(string[] args)
        {
            Called called = new Called();
            Binary bin = new Binary(called);
            bin.performCallback0_Once();


            called.Val = "Valor";
        }
    }

    public class Called : IFramework0
    {
        public string Val { get; set; }
        public void Callback()
        {
            object a = new object();
            int valor = 22;
        }
    }
}
