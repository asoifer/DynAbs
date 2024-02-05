using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynAbs.Test.Cases
{
    class StructsModifytInsideInvocation
    {
        public static void Main(string[] args)
        {
            StructA structA = new StructA();
            structA.structBField = new StructB();
            structA.structBField.intField = 2;
            structA.structBField.classAField = new ClassA();
            structA.structBField.classAField.intField = 13;
            Func(structA);
            int x = structA.structBField.intField + structA.structBField.classAField.intField;
        }

        private static void Func(StructA myStruct)
        {
            myStruct.structBField.intField = 3;
            myStruct.structBField.classAField = new ClassA();
            myStruct.structBField.classAField.intField = 17;
        }

        public struct StructA
        {
            public StructB structBField;
        }

        public struct StructB
        {
            public int intField;
            public ClassA classAField;
        }

        public class ClassA
        {
            public int intField;
        }
    }
}
