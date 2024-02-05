using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynAbs.Test.Cases
{
    class YieldWithImplicitTuples
    {
        static void Main()
        {
            var myColection = ForwardIterator(1, 1);
            Console.WriteLine(myColection.First().s);
            return;
        }

        static IEnumerable<(int col, int row, String s)> ForwardIterator(int col, int row)
        {
            if (col < 0 || row < 0)
                yield break;

            while (row < 2)
            {
                for (int c = col; c < 2; c++)
                {
                    yield return (c, row, "HELLO");
                }
                col = 0;
                row++;
            }
        }
    }
}
