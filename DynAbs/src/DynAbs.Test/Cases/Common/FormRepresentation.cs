using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynAbs.Test.Cases
{
    class FormRepresentation
    {
        public static void Main(string[] args)
        {
            var form = new List<Aux>();
            var a = new A();
            var b = new B();
            form.Add(a);
            form.Add(b);
            // Hasta ahora a y b están unidos a través de FORM, así sería en un formulario.
            // Ahora bien, lo normal es que durante la ejecución del código hagamos cosas con A que NO impacten ni en FORM ni en B
            // Ejemplo:
            a.PropertyX.texto = "PEPE";
            // Notar que esto hace que todos los last def de los nodos de B y FORM 
            // ahora se actualizan con este statement, cuando no es lo que realmente queremos
        }

        class Aux
        {

        }

        class A : Aux
        {
            public Datos PropertyX { get; set; }
            public A()
            {
                PropertyX = new Datos();
            }
        }

        class B : Aux
        {
            public Datos PropertyY { get; set; }
            public B()
            {
                PropertyY = new Datos();
            }
        }

        class Datos
        {
            public string texto { get; set; }
            public int longitud { get; set; }
        }
    }
}
