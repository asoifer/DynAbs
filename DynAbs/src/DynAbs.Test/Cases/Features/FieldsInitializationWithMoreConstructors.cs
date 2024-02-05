using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynAbs.Test.Cases
{
    class FieldsInitializationWithMoreConstructors
    {
        public static void Main()
        {
            var k = "Javito";
            var v = true;
            var a = new TestClass(k, v);
            var sliceVariable = a.GetFirstKey();
        }

        class TestClass
        {
            Dictionary<string, bool> myDictionary = new Dictionary<string, bool>();
            public TestClass() : this ("Billy", false)
            {

            }

            public TestClass(string firstKey, bool firstValue)
            {
                myDictionary.Add(firstKey, firstValue);
            }

            public string GetFirstKey()
            {
                return myDictionary.Keys.First();
            }
        }
    }
}
