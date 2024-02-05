using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynAbs.Test.Cases.Types
{
    class CastingElementsForeach
    {
        public static void Main()
        {
            var myList = new List<TestInterface>();
            myList.Add(new A());
            myList.Add(new B());
            foreach(var l in myList)
            {
                if (l is A)
                    ((A)l).aProperty = 1;
                if (l is B)
                    ((B)l).bProperty = 1;
            }
            return;
        }

        public interface TestInterface { }

        public class A : TestInterface
        {
            public int aProperty { get; set; }
        }
        public class B : TestInterface
        {
            public int bProperty { get; set; }
        }
    }
}
