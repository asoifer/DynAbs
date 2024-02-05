using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynAbs.Test.Cases.Common
{
    class EmptyStruct
    {
        static void Main()
        {
            var a = new A();
            var b = false;
            if (a.M())
                b = true;
            Console.WriteLine(b);
            return;
        }

        struct Rect
        {
            public int X;
            public int Y;
            public int Width;
            public int Height;
            public static readonly Rect Empty;

            public Rect(int x, int y, int width, int height)
            {
                this.X = x;
                this.Y = y;
                this.Width = width;
                this.Height = height;
            }

            public bool IsEmpty
            {
                get
                {
                    return ((X == 0) && (Y == 0) && (Width == 0) && (Height == 0));
                }
            }
        }

        class A
        {
            internal Rect emptyRect { get; private set; } = Rect.Empty;

            public bool M()
            {
                return emptyRect.IsEmpty;
            }
        }
    }
}
