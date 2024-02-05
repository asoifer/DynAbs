using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynAbs.Test.Cases.Features
{
    class ComplexPlusEqualsOperator
    {
        static A myAInstance = new A();

        static void Main()
        {
            myAInstance.myProperty = 0;
            GetAInstance().myProperty += GetValor();
            var dummy = myAInstance.myProperty;
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
            int _myField;
            public int myProperty
            {
                get
                {
                    Console.WriteLine("Entrando al get");
                    return _myField;
                }
                set
                {
                    Console.WriteLine("Entrando al set");
                    _myField = value;
                }
            }

            public A()
            {

            }
        }
    }
}
