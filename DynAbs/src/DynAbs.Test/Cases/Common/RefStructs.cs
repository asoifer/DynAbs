using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DynAbs.Test.Cases
{
    class RefStructs
    {
        static void Main()
        {
            var unStruct = new MyStruct() { valor = 1 };
            var clsPrueba = new Prueba();
            clsPrueba.UnEvento(ref unStruct);
            return;
        }

        struct MyStruct
        {
            public int valor { get; set; }
        }

        class Prueba
        {

            public bool UnEvento(ref MyStruct m)
            {
                Metodo(m.valor, m.valor, m.valor, m.valor);
                return true;
            }

            static void Metodo(int param1, int param2, int param3, int param4)
            {
            }
        }
    }
}
