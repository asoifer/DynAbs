using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynAbs.Test.Cases.Roslyn
{
    class CallWithOutNotInitialized
    {
        public static void Main(string[] args)
        {
            int value; //Notar que no se inicializa, esto da error en el Wrapper.
            Method(out value);
            Console.WriteLine(value);
            // value is now 44
        }

        static void Method(out int i)
        {
            i = 44;
        }
    }
}
