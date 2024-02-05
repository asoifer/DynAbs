using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynAbs.Test.Cases.Solver
{
    class HubCompression
    {
        /// <summary>
        /// Queremos evaluar que tanto afecta la compresión a la performance
        /// Para probarla a full hay que descomentar la sección de list en summaries 
        /// En 5000 elementos antes habría 5000 hubs doblemente enlazados por lambda
        /// </summary>
        public static void Main(string[] args)
        {
            var lista = new List<A>();
            for(var i = 0; i < 20; i++)
            {
                var tmp = new A();
                lista.Add(tmp);
            }
            Console.WriteLine("Termino");
        }

        class A
        {
        }
    }
}
