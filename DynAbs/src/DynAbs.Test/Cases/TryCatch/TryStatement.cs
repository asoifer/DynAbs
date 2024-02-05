using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynAbs.Test.Cases
{
    class TryStatement
    {
        static void Main()
        {
            string s = null; // For demonstration purposes.
            try
            {
                string p = ProcessString(s);
                s = p;
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

        static String ProcessString(string s)
        {
            if (s == null)
            {
                throw new ArgumentNullException(); //Estoy ligando este objeto al s! MAL MAL!
            }
            else
            {
                return s + "hola";
            }

        }
    }
}
