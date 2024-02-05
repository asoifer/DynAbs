using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynAbs.Test.Cases
{
    class SimpleBaseWithoutConstructors
    {
        public static void Main()
        {
            var n = new NorthWest();
            return;
        }

        public abstract class Quadrant
        {

        }

        public class NorthWest : Quadrant
        {

        }
    }
}
