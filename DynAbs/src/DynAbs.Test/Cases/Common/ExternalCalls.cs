using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace DynAbs.Test.Cases
{
    class ExternalCalls
    {
        static void Main()
        {
            var i = IntPtr.Zero;
            IntPtr pyname = Runtime.PyObject_Unicode(i);
            return;
        }

        class Runtime
        {
            [DllImport(_PythonDll, CallingConvention = CallingConvention.Cdecl)]
            internal static extern IntPtr PyObject_Unicode(IntPtr pointer);

            internal const string dllBase = "python" + _pyver;
            internal const string dllWithPyMalloc = "";
            internal const string dllWithPyDebug = "";
            internal const string _pyver = "27";
            internal const string _PythonDll = dllBase + dllWithPyDebug + dllWithPyMalloc;
        }
    }
}
