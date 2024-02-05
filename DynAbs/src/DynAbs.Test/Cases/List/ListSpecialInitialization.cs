using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynAbs.Test.Cases.List
{
    class ListSpecialInitialization
    {
        public static void Main()
        {
            var memos = new List<M> {
                new M { Titulo = "Título 1", Descripcion = "Bella magia", Fecha = new DateTime(2007, 03, 12) },
                new M { Titulo = "Otro título", Descripcion = "Ser o no ser", Fecha = DateTime.Now }
            };
            var a = memos[1].Fecha;
        }

        class M
        {
            public string Titulo { get; set; }
            string _descripcion;
            public string Descripcion
            {
                get
                {
                    return _descripcion;
                }
                set
                {
                    _descripcion = value;
                }
            }
            public DateTime Fecha { get; set; }
        }
    }
}
