using System;

namespace DynAbs.Test.Cases.Common
{
    class NestedCallBaseConstructor
    {
        static void Main()
        {
            var a = new A("", false, "B", "C", "D", "E");
            Console.WriteLine(a.prop);
            return;
        }

        class A
        {
            public bool prop { get; set; }

            internal A
            (
                string name,
                bool isDefault,
                string applicationBase
            )
            {
                prop = isDefault;
            }

            internal A
            (
                string name,
                bool isDefault,
                string applicationBase,
                string assemblyName
            ) :  this(name, isDefault, applicationBase)
            {

            }

            public A
            (
                string name,
                bool isDefault,
                string applicationBase,
                string assemblyName,
                string moduleName,
                string description
            ) : this(name, isDefault, applicationBase, assemblyName)
            {
                
            }
        }
    }
}
