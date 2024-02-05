using ExternalLibraryExample;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynAbs.Test.Cases
{
    class Callback0FollowLdOutside
    {
        public static void Main(string[] args)
        {
            CalledChanging called = new CalledChanging();
            called.Val = "valor anterior";
            Binary bin = new Binary(called);
            bin.performCallback0_Once();
            string valor = called.Val;
        }
    }

    public class CalledChanging : IFramework0
    {
        public string Val { get; set; }
        public void Callback()
        {
            this.Val = "valor nuevo";
        }
    }
}
