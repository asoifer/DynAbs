using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynAbs.Test.Cases
{
    class ThrowInsideTry
    {
        public static void Main(string[] args)
        {
            string s = null; // For demonstration purposes.
            try
            {
                throw DameExcepcion("Me pinta");
            }
            catch (Exception e)
            {
                s = e.Message;
            }
            finally
            {
                s = s + " algo ";
            }
            var t = s;
        }

        public static Exception DameExcepcion(string message)
        {
            string s = "Exepcion: " + message;
            return new Exception(s);
        }
    }
}
