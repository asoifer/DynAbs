using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynAbs.Test.Cases.Features
{
    class PlusEqualsOperator
    {
        static A myAInstance = new A();

        static void Main()
        {
            myAInstance.myProperty = 0;
            GetAInstance().myProperty += GetValor();
            return;
        }

        static int GetValor()
        {
            Console.WriteLine("Entré a GetValor");
            return 1;
        }

        static A GetAInstance()
        {
            Console.WriteLine("Entré a GetAInstance");
            return myAInstance;
        }

        class A
        {
            public int myProperty { get; set; }

            public A()
            {

            }
        }
    }
}
