using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynAbs.Test.Cases.Features
{
    class PlusEqualsOperatorWithProperties
    {
        static A myAInstance = new A();

        static void Main()
        {
            myAInstance.myProperty = new B();
            myAInstance.myProperty.anotherProperty = 0;
            GetAInstance().myProperty.anotherProperty += GetValor();
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
            public B myProperty { get; set; }

            public A()
            {

            }
        }

        class B
        {
            public int anotherProperty { get; set; }
        }
    }
}
