using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynAbs.Test.Cases.Features
{
    class YieldWithoutElements
    {
        static void Main()
        {
            IEnumerable<A> validations = ValidateInd();
            foreach (var val in validations)
                Console.WriteLine("Error");
            var c = validations.Count();
            return;
        }

        static IEnumerable<A> ValidateInd()
        {
            return Validate();
        }

        static IEnumerable<A> Validate()
        {
            var enterIf = false;

            if (enterIf)
                yield return new A();
        }

        class A
        {

        }
    }
}
