using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynAbs.Test.Cases.Common
{
    class OperatorOverride
    {
        public static void Main()
        {
            OperatorOverride a = new OperatorOverride();
            var b = (-a);
            OperatorOverride c = b;
        }
        
        // unary minus
        public static OperatorOverride operator -(OperatorOverride origin)
        {
            OperatorOverride result = new OperatorOverride();
            return result;
        }
    }
}
