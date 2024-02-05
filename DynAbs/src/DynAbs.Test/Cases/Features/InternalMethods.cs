using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynAbs.Test.Cases
{
    class InternalMethods
    {
        static void Main()
        {
            var t = Test();
            Console.WriteLine(t.value);
            return;
        }

        public struct Attribute
        {
            public int value;
        }

        public static Attribute Test()
        {
            Attribute currentAttribute = new Attribute();
            currentAttribute.value = 0;

            void SetAttribute(Attribute attribute)
            {
                if (currentAttribute.value != attribute.value)
                {
                    currentAttribute = attribute;
                }
            }

            Attribute testAttrA = new Attribute();
            testAttrA.value = 1;
            SetAttribute(testAttrA);
            return currentAttribute;
        }
    }
}
