using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynAbs.Test.Cases
{
    class AliasingRecoveringReference
    {
        public static void Main(string[] args)
        {
            Nav nav = new Nav();
            Contenido otro = nav.DameContenido();
            string aa = otro.VAL;
        }

        class Nav
        {
            private Contenido contenido;

            public Nav()
            {
                this.contenido = new Contenido();
                this.contenido.VAL = "algun valor";
            }
            public Contenido DameContenido()
            {
                return this.contenido;
            }
        }

        class Contenido
        {
            public string VAL;
        }
    }
}
