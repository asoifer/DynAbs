using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynAbs.Test.Cases.Features
{
    class KeywordYieldBreak
    {
        public static void Main(string[] args)
        {
            var a = 0;
            foreach (int i in Integers())
            {
                a += i;
            }
            Console.WriteLine(a);
            return;
        }
        public static IEnumerable<int> Integers()
        {
            yield return 1;
            yield return 2;
            yield return 4;
            yield return 8;
            yield return 16;
            yield break;
        }
    }
}
