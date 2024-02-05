using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynAbs.Test.Cases
{
    class SimpleDecimalConstant
    {
        //const Decimal valorPruebaPura = 0;
        const Decimal valorPrueba = Decimal.Zero;

        static void Main()
        {
            var myDecimal = funcion(Decimal.Zero);
            //var _myDecimal = Wrapper(() => funcion(Wrapper(() => Decimal.Zero)));

            return;
        }

        static T Wrapper<T>(Func<T> function)
        {
            return function.Invoke();
        }

        static int funcion(decimal unDecimal)
        {
            return 1;
        }

        class A
        {

        }
    }
}
