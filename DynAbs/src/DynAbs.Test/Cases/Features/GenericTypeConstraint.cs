using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynAbs.Test.Cases
{
    class GenericTypeConstraint
    {
        static void Main(string[] args)
        {
            string s1 = "target";
            string s2 = "target2";
            testMethod<string>(s1,s2);
            System.Console.WriteLine(s1);
            s1 = s1 + "s";
            return;
        }

        public static void testMethod<T>(T s1, T s2) where T : class
        {
            s1 = s2;
        }


    }
}
