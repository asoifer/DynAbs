using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynAbs.Test.Cases
{
    class ComplexFieldsInitializacionWithMoreConstructors
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
            Dictionary<string, bool> myDictionary = GetUniqueDictionary();
            public TestClass()
            {

            }

            public TestClass(string firstKey, bool firstValue) : this()
            {
                myDictionary.Add(firstKey, firstValue);
            }

            public string GetFirstKey()
            {
                return myDictionary.Keys.First();
            }
        }

        public static bool FirstCall = false;
        public static Dictionary<string, bool> GetUniqueDictionary()
        {
            if (!FirstCall)
            {
                FirstCall = true;
                return new Dictionary<string, bool>();
            }
            throw new Exception("Si el instrumentador funciona bien no se debe entrar acá en este caso");
        }
    }
}
