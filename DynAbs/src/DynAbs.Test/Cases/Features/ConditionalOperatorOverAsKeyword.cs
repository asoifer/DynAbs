using System;

namespace DynAbs.Test.Cases.Features
{
    class ConditionalOperatorOverAsKeyword
    {
        static void Main()
        {
            var a = new A();
            var b = (a as B)?.GetVal() as int?;
            Console.WriteLine(b.Value);
            return;
        }

        class A : B
        {

        }

        class B
        {
            public int? Val;
            public int? GetVal()
            {
                return Val ?? 0;
            }
        }
    }
}
