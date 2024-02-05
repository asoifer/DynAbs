using System;

namespace DynAbs.Test.Cases
{
    class ConditionalAccessH
    {
        static void Main()
        {
            SymbolDeclaredEvent(null);
            return;
        }

        static void SymbolDeclaredEvent(A a)
        {
            var temp = a?.DoSomething(new B(a.GetValue()));
            a?.DoSomethingVoid(new B(a.GetValue()));
        }

        class A
        {
            internal int GetValue()
            {
                return 1;
            }

            internal int DoSomething(B b)
            {
                return 1;
            }

            internal void DoSomethingVoid(B b)
            {
            }
        }

        class B
        {
            public B(int val)
            {

            }
        }
    }
}
