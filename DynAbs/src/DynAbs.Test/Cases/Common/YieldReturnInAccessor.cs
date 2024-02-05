using System;
using System.Collections.Generic;
using System.Linq;

namespace DynAbs.Test.Cases
{
    class YieldReturnInAccessor
    {
        static void Main()
        {
            var myElements = GetElements;
            var x = myElements.Count();
            var y = x;
            return;
        }

        public static IEnumerable<object[]> GetElements
        {
            get
            {
                foreach (var level in Enum.GetValues(typeof(TestEnum)))
                {
                    yield return new object[] { level };
                }
            }
        }

        enum TestEnum
        {
            A = 1,
            B = 2
        }
    }
}
