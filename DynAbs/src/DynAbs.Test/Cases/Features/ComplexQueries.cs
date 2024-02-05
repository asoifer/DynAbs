using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynAbs.Test.Cases.Features
{
    class ComplexQueries
    {
        internal static int contador = 0;
        const int variableAuxiliar = 1;
        const int variableAuxiliar2 = 2;
        const int variableAuxiliar3 = 100;

        static void Main()
        {
            var lista = new List<Casa>();
            for (int i = 0; i < 3; i++)
                lista.Add(new Casa());
            var lista2 = new List<Casa>();
            for (int i = 0; i < 3; i++)
                lista2.Add(new Casa());

            var copiaLista = (from unaCasa in lista
                              join otraCasa in lista2 
                              on unaCasa.cantVentanas equals otraCasa.cantVentanas - variableAuxiliar2
                              where otraCasa.cantVentanas < variableAuxiliar3
                              select new { unaCasa.cantVentanas, unaCasa.propietario.nombre, variableAuxiliar }).ToList();

            var miCantidadVentanas = copiaLista.First().cantVentanas;
        }

        class Casa
        {
            public int cantVentanas { get; set; }
            public Persona propietario { get; set; }
            public Casa()
            {
                cantVentanas = ComplexQueries.contador++;
                propietario = new Persona((ComplexQueries.contador++).ToString());
            }
        }

        class Persona
        {
            public string nombre { get; set; }
            public Persona(string nombre)
            {
                this.nombre = nombre;
            }
        }
    }
}
