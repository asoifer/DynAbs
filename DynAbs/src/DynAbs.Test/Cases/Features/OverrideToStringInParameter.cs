using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynAbs.Test.Cases
{
    class OverrideToStringInParameter
    {
        public static void Main(string[] args)
        {
            var c = new Cliente<string>("hola");
            Console.WriteLine(c);
        }

        public class Cliente<T>
        {
            T _valor;

            public Cliente(T valor)
            {
                _valor = valor;
            }

            public override string ToString()
            {
                var temp = new Cliente<T>(_valor);
                return _valor.ToString();
            }
        }
    }
}
