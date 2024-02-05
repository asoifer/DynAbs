using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynAbs.Test.Cases.Features
{
    class PropertyInitializationInline_B
    {
        static void Main()
        {
            var a = new A();
            Console.WriteLine(a.ErrorBackgroundColor);
            return;
        }

        public class A
        {
            public ConsoleColor ErrorForegroundColor { get; set; } = ConsoleColor.Red;
            public ConsoleColor ErrorBackgroundColor { get; set; } = Console.BackgroundColor;
        }
    }
}
