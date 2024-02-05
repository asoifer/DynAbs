using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynAbs.Test.Cases
{
    class TryStmtReturnWithFinallyInCatch
    {
        public static void Main(string[] args)
        {
            Error err = new Error();
            string s = Func(err);
            string x = s + err.errmsg;
        }

        private static string Func(Error err)
        {
            string s = "";
            try
            {
                throw new Exception("Excepcion en el try!");
            }
            catch(Exception e)
            {
                s = s + "Hola mundo! " + e.ToString();
                return s;
            }
            finally
            {
                err.errmsg = " -- Llego el finally! --";
            }
        }

        public class Error
        {
            public string errmsg = "";
        }
    }
}
