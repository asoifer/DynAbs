using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynAbs.Test.Cases
{
    class AssignOrder
    {
        public static void Main(string[] args)
        {
            var a = new A();
            // Qué entrará primero? El GetInstance o el GetInt?
            GetInstance(a).i = GetInt();
            // Qué entrará primero? al EnterStatic o al GetInt?
            B.j = GetInt();
            var testArray = new double[] { 0.1, 0.2 };
            // Qué entrará primero? a GetInt o a GetDouble?
            testArray[GetInt()] = GetDouble();
            return;
        }

        static A GetInstance(A a)
        {
            return a;
        }

        static int GetInt()
        {
            return 0;
        }

        static double GetDouble()
        {
            return (double)0.5;
        }

        class A
        {
            public int i { get; set; }
        }

        class B
        {
            public static int j { get; set; }
        }
    }
}
