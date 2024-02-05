using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynAbs.Test.Cases.Features
{
    class ComplexQuerieWithFromInside
    {
        internal static int contador = 0;
        static void Main()
        {
            var listaCasas = new List<Casa>();
            for (int i = 0; i < 3; i++)
                listaCasas.Add(new Casa());
            var q =
                from Casa c in listaCasas
                select new
                {
                    CantVentanas = c.cantVentanas,
                    Nombre = c.propietario.nombre,
                    Calculo =
                    (
                        from Persona p in c.amigos
                        select Convert.ToInt32(p.nombre)
                    ).Sum()
                };
            var f = q.First();
            var sliceVariable = f.Calculo;
        }

        class Casa
        {
            public int cantVentanas { get; set; }
            public Persona propietario { get; set; }
            public List<Persona> amigos { get; set; }
            public Casa()
            {
                cantVentanas = ComplexQuerieWithFromInside.contador++;
                propietario = new Persona((ComplexQuerieWithFromInside.contador++).ToString());
                var temp = new Persona(cantVentanas.ToString());
                amigos = new List<Persona>() { temp };
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
