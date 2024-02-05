using System;
using System.Collections.Generic;
using System.Text;

namespace DynAbs.Test.Cases.Common
{
    class NewUsingThisWithArguments
    {
        static void Main()
        {
            var a1 = new A(1);
            var a2 = a1.WithVal(2);
            var c = a2.Val;
        }

        class A
        {
            public int Val { get; set; }

            public A(int myVal)
            {
                Val = myVal;
            }

            public A(A other) : this(myVal: other.Val)
            {

            }

            public A WithVal(int val)
            {
                if (val == this.Val)
                    return this;

                return new A(this);
            }
        }
    }
}
