using System;
using System.Collections.Generic;
using System.Text;

namespace DynAbs.Test.Cases.Common
{
    class StructExtension
    {
        static void Main()
        {
            var myStruct = new StructExtension_TestStruct<int>();
            myStruct.Val = 0;
            var slicingVariable = myStruct.RetVal<int>();
            return;
        }        
    }

    public struct StructExtension_TestStruct<T>
    {
        public int Val;
    }

    public static class MyExtensions
    {
        public static int RetVal<T>(this StructExtension_TestStruct<T> testStruct)
        {
            return testStruct.Val;
        }
    }
}
