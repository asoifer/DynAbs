using System;
using System.Collections.Generic;
using System.Text;

namespace DynAbs.Test.Cases.Features
{
    class RecursivePattern
    {
        static void Main()
        {

            var exp = new A();
            exp.Arguments.Add("");
            var exp2 = new B();
            var exp3 = new A();

            var t1 = TryGetVal(exp);
            var t2 = TryGetVal(exp2);
            var t3 = TryGetVal(exp3);
            var s = t1 + t2 + t3;

            return;
        }

        static int TryGetVal(Int exp)
        {
            switch (exp)
            {
                case A { Arguments: { Count: 1 } args }:
                    return args.Count;

                case B { Conv: { K: Kind.A }, Op: var op }
                        when op.Equals(1):
                    return 3;

                default:
                    return 4;
            }
        }

        interface Int
        {

        }

        class A : Int
        {
            public List<string> Arguments { get; set; } = new List<string>();
        }

        class B : Int
        {
            public C Conv { get; set; } = new C();
            public int Op { get; set; } = 1;
        }

        class C
        {
            public Kind K { get; set; } = Kind.A;
        }

        enum Kind
        {
            A,
            B
        }
    }
}
