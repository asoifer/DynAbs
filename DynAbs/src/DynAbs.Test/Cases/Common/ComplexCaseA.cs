using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynAbs.Test.Cases
{
    class ComplexCaseA
    {
        static void Main()
        {
            var t = new T();
            var a = t.m();
            Console.WriteLine(a);
        }

        public static uint modInv(uint num)
        {
            return (uint)num;
        }

        class T
        {
            public T()
            {
                keyAttrs = new Tuple<string, Func<int, int>>[]
                {
                    new Tuple<string, Func<int, int>>("HELLO", x => x)
                };
            }

            Tuple<string, Func<int, int>>[] keyAttrs;

            public int m()
            {
                Dictionary<string, Func<int, int>> keyFuncs = keyAttrs
                    .Where(entry => entry != null)
                    .ToDictionary(entry => entry.Item1, entry => entry.Item2);

                var k = "HELLO";
                var u = (uint)1;
                int encodedKey = keyFuncs[k]((int)modInv(u));
                return encodedKey;
            }
        }
    }
}
