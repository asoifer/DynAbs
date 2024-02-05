using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynAbs.Test.Cases.Features
{
    class NestedInitializations
    {
        static void Main()
        {
            var a = true;
            var frame = new Frame
            {
                info =
                {
                    leftIndentation = 0,
                    rightIndentation = 0,
                    firstLine = a ? 0 : 1
                }
            };
            Console.WriteLine(frame.info.firstLine);
        }

        class Frame
        {
            public Info info = new Info();
        }

        class Info
        {
            public int leftIndentation = 0;
            public int rightIndentation = 0;
            public int firstLine = 0;
        }
    }
}
