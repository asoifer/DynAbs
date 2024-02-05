using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynAbs.Test.Cases
{
    class DictionariesInicialization
    {
        public enum MyEnum
        {
            ValorA,
            ValorB
        }

        public class ClaseA
        {

        }

        private static readonly Dictionary<MyEnum, object> _cosas = 
            new Dictionary<MyEnum, object>
		        {
		            {
		                MyEnum.ValorA,
		                new ClaseA()
		                },
		            {
		                MyEnum.ValorB,
		                new ClaseA()
		            }
		        };

        static void Main()
        {
            var tmp = _cosas.Keys;
            return;
        }
    }
}
