using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynAbs.Test.Cases
{
    class RegionsTrivia
    {
        #region Properties
        static int t = 1;
        #endregion

        static void Main()
        {
            #region Internal region
            var a = 1;
            #endregion

            return;
        }

        #region Aux
        void Pepe()
        {
            #region NestedRegion
            var b = 1;
            #endregion
        }
        #endregion
    }
}
