using System;
using System.Collections.Generic;
using System.Text;

namespace DynAbs.Test.Cases.Common
{
    class SwitchWithVariableDeclaration
    {
        static void Main()
        {
            object Value = new List<int>() { };
            var c = ReturnSomething(Value);
            return;
        }

        static int ReturnSomething(object Value)
        {
            switch (Value)
            {
                case string source:
                    return 1;
                case string[] sources:
                    return 2;
                case List<int> list:
                    return list.Count;
                case null:
                    return -1;
                default:
                    throw new Exception($"Unexpected value: {Value}");
            }
        }
    }
}
