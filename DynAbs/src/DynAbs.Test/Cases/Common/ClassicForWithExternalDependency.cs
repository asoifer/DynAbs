using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace DynAbs.Test.Cases
{
    class ClassicForWithExternalDependency
    {
        public static void Main()
        {
            int j = 1;
            int i = 0;
            for (i = 0; i < j; i++)
            {
                Console.WriteLine("");
            }
            var slicingVariable = i;
            return;
        }
    }
}
