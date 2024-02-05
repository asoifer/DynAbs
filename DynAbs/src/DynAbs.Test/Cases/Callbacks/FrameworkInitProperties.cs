using ExternalLibraryExample;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynAbs.Test.Cases
{
    class FrameworkInitProperties
    {
        static void Main()
        {
            var valor = 1;
            var instance = new MyClass(valor);
            if (instance.MyProperty == 0)
                throw new Exception("El valor debe ser 1, lo inicializó el framework");
            return;
        }

        class MyClass : InitializePropertiesFramework
        {
            public MyClass(int valor) : base(valor)
            {

            
            }

            public override int MyProperty { get; set; }
        }
    }
}
