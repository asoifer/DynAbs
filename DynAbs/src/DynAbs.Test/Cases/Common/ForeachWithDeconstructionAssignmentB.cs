using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;

namespace DynAbs.Test.Cases.Common
{
    class ForeachWithDeconstructionAssignmentB
    {
        static void Main()
        {
            var dict = ImmutableDictionary<string, MyCustomStruct>.Empty;
            dict.Add("a", new MyCustomStruct(new AnotherClass()));
            var a = new Aux(dict);
            a.DoSomething();
            return;
        }

        class Aux
        {
            public readonly ImmutableDictionary<string, MyCustomStruct> InternalDict;

            public Aux(ImmutableDictionary<string, MyCustomStruct> internalDict)
            {
                InternalDict = internalDict;
            }

            public void DoSomething()
            {
                foreach (var (_, myVar) in InternalDict)
                {
                    myVar.A.DoSomethingMore();
                }
            }
        }

        struct MyCustomStruct
        {
            public readonly AnotherClass A;

            public MyCustomStruct(AnotherClass a)
            {
                A = a;
            }
        }

        class AnotherClass
        {
            public int prop { get; set; } = 1;

            public void DoSomethingMore()
            {
                var b = this.prop;
                var c = Math.Max(1, b);
            }
        }
    }
}
