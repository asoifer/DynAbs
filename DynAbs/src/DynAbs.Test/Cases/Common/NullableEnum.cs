using System;

namespace DynAbs.Test.Cases.Common
{
    class NullableEnum
    {
        static void Main()
        {
            MyEnum? t = null;
            var i = TryToDoSomething(t);
            var j = i;
            return;
        }

        static int TryToDoSomething(MyEnum? @enum)
        {
            if (!@enum.HasValue)
                return 1;
            return 0;
        }

        enum MyEnum
        {
            A = 1,
            B = 2
        }
    }
}
