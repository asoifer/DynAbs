using System;

namespace DynAbs.Test.Cases.Features
{
    class AsWithNullableExpression
    {
        static void Main()
        {
            bool simpleBoolean = true;
            SimpleClass sc;
            SetSimpleClass(out sc);
            ParentClass pc;
            SetParentOrChildClass(out pc);

            var i = 0;
            if (simpleBoolean && sc != null && (pc as ChildClass)?.K.Kind == CustomKind.A)
            {
                i++;
            }

            Console.WriteLine(i);
            return;
        }

        static void SetSimpleClass(out SimpleClass cs)
        {
            cs = new SimpleClass();
        }

        static void SetParentOrChildClass(out ParentClass pc)
        {
            pc = new ChildClass();
        }

        class SimpleClass
        {

        }

        class ParentClass { }
        class ChildClass : ParentClass 
        {
            AuxClass k = new AuxClass();
            public AuxClass K => this.k;
        }

        class AuxClass
        {
            public CustomKind Kind = CustomKind.A;
        }

        public enum CustomKind : ushort
        {
            A = 1,
            B = 2
        }
    }
}
