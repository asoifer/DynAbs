using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynAbs.Test.Cases.Common
{
    class NewTypeT
    {
        public static void Main()
        {
            var a = Create<Int32>();
            int b = a;
        }
        static T Create<T>() where T : new()
        {
            return new T(); 
        }
    }
}
