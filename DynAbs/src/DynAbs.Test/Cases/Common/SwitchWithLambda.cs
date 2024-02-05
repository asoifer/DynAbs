using System;
using System.Collections.Generic;

namespace DynAbs.Test.Cases
{
    class SwitchWithLambda
    {
        static void Main()
        {
            var someValue = TestEnum.A;
            var otherValue = TestEnum.C;
            var a = GetSomething(someValue);
            var a2 = GetSomething(someValue);
            var b = GetSomething(otherValue);
            var c = a + a2 + b;
            return;
        }

        enum TestEnum
        {
            A = 1,
            B = 2,
            C = 3
        }

        static int F()
        {
            return 1;
        }

        static int G()
        {
            return 2;
        }

        static int H()
        {
            return 3;
        }

        static bool temp = true;
        static bool B()
        {
            if (temp)
            { 
                temp = false;
                return true;
            }
            return false;
        }

        static int GetSomething(TestEnum testEnum) => testEnum switch
        {
            TestEnum.A when B() 
                    => F(),
            TestEnum.A 
                    => F(),
            TestEnum.B 
                    => G(),
            _ => H(),
        };
    }
}
