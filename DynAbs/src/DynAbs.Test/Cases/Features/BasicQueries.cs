using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynAbs.Test.Cases.Features
{
    class BasicQueries
    {
        internal static int contador = 0;

        static void Main()
        {
            var lista = new List<Casa>();
            for (int i = 0; i < 1; i++)
                lista.Add(new Casa());

            var copiaLista = (from unaCasa in lista select unaCasa).ToList();
            var miCasa = copiaLista.First();

        }

        class Casa
        {
            public int cantVentanas { get; set; }
            public Persona propietario { get; set; }
            public Casa()
            {
                cantVentanas = BasicQueries.contador++;
                propietario = new Persona((BasicQueries.contador++).ToString());
            }
        }

        class Persona
        {
            public string nombre {get;set;}
            public Persona(string nombre)
            {
                this.nombre = nombre;
            }
        }
    }
}
