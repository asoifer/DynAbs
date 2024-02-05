using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynAbs.Test.Cases
{
    class InlineMethodsWithNativeMethods
    {
        static void Main()
        {
            var c = Curses.isendwin();
            return;
        }

        public partial class Curses
        {
            static NativeMethods methods = new NativeMethods();
            static public bool isendwin() => methods.isendwin();
        }

        class NativeMethods
        {
            public readonly Delegates.isendwin isendwin;

            public NativeMethods()
            {
                isendwin = () => true; 
            }
        }

        internal class Delegates
        {
            public delegate bool isendwin();
        }
    }
}
