using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynAbs.Test.Cases
{
    class NotSupportedOutInCondition
    {
        static void Main(string[] args)
        {
            int a;
            if (foo(out a)) ;//el error de compilacion ya esta resuelto, ahora hay que tratar invocaciones en la condicion del if //aca hay un error al compilar el instrumentado, porque por el lazy evaluation (aka. cortocircuito) no necesariamente se ejecutará foo (a pesar que TraceSimpleStatement siempre devuelve true, parece que el compilador no se mete a chequear eso)
            int b = a;
        }

        public static bool foo(out int param)
        {
            param = 0;
            return true;
        }
    }
}
