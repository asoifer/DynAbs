using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynAbs.Test.Cases
{
    class PTGFP_MatrixMultiplication
    {
        static void Main()
        {
            var rnd = new Random();

            int x = 2;
            int o, i, j, k;

            int[][] m1 = new int[x][];
            for (i = 0; i < x; i++)
            {
                m1[i] = new int[x];
            }
            for (o = 0; o < x; o++)
                for (i = 0; i < x; i++)
                    m1[o][i] = rnd.Next(1, 100);

            int[][] m2 = new int[x][];
            for (i = 0; i < x; i++)
            {
                m2[i] = new int[x];
            }
            for (o = 0; o < x; o++)
                for (i = 0; i < x; i++)
                    m2[o][i] = rnd.Next(1, 100);

            int[][] ans = new int[x][];
            for (i = 0; i < x; i++)
            {
                ans[i] = new int[x];
            }

            int n = m1.Length;
            for (i = 0; i < n; i++)
                for (j = 0; j < n; j++)
                    for (k = 0; k < n; k++)
                        ans[i][j] += m1[i][k] * m2[k][j];

            int y = ans[1][1];
        }
    }
}
