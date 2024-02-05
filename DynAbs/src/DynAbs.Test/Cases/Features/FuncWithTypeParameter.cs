using System;
using System.Collections.Generic;
using System.Text;

namespace DynAbs.Test.Cases.Features
{
    public class FuncWithTypeParameter
    {
        static void Main()
        {
            var l1 = new List<int>() { 1 };
            Func<int, IList<string>> myFunc = AuxMethod<int, string>;
            var r = Container<string>.Create<int>(l1, myFunc);
            var a = r.Count;
            return;
        }

        public static IList<V> AuxMethod<T, V> (T a)
        {
            return new List<V>() {  };
        }

        class Container<T>
        {
            public static IList<T> Create<TOrig>(IList<TOrig> collections, Func<TOrig, IList<T>> selector)
            {
                return selector(collections[0]);
            }
        }
	}
}
