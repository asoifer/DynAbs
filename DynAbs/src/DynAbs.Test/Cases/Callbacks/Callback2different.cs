using ExternalLibraryExample;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynAbs.Test.Cases
{
    class Callback2different
    {
        public static void Main(string[] args)
        {
            Calls2different called = new Calls2different();
            Binary bin = new Binary(called);
            bin.performCallback0_Twice();
        }
    }

    public class Calls2different : IFramework0
    {
        private int entramos = 0;
        private string cadenatotal = "INICIAL";
        private void click1()
        {
            string leido = "primerclick";
            if (entramos % 2 == 0)
                cadenatotal = concatenar(cadenatotal, leido, entramos);
            else
                cadenatotal = "BORROINICIAL";
            if (entramos >= 2) {// if (entramos >= 1) es hack para test mas corto, criteria 32
                string a = cadenatotal;}
            entramos++;
        }

        private void click2()
        {
            string leido = "segundoclick";
            if (entramos % 2 == 0)
                cadenatotal = concatenar(cadenatotal, leido, entramos);
            if (entramos >= 2){
                string a = cadenatotal;}
        }

        public void Callback()
        {
            if (entramos == 0)
                entramos = 1;
            if (entramos == 1)
                click1();
            else
                click2();

            //int i;
            //i = entramos;
            //i = 4;
            //entramos++;
            //string leido = textBox1.Text;
            //if (entramos % 2 == 0)
            //    cadenatotal = concatenar(cadenatotal, leido, entramos);
            //if (entramos >= i)
            //    Console.WriteLine(cadenatotal);
        }

        private string concatenar(string primera, string segunda, int cantidad)
        {
            string res;
            if (cantidad == 1)
                //if (cantidad % 4 == 0)
                res = primera + segunda;
            else
                res = primera + "no_concateno";
            return res;
        }
    }
}
