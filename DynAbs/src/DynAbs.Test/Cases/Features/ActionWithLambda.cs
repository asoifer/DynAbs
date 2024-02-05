using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynAbs.Test.Cases
{
    class ActionWithLambda
    {
        public static void Main()
        {
            var action = new Action<int>(x => Console.WriteLine(x));
            action.Invoke(1);
            var intValue = 1;
            var func = new Func<int, string>(x => x.ToString());
            var s = func.Invoke(intValue);
        }
    }
}
