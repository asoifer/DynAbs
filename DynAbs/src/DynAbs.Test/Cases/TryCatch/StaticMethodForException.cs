using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynAbs.Test.Cases
{
    class StaticMethodForException
    {
        static void Main()
        {
            string message = "";
            string p = "p";
            try
            {
                throw A.NewArgumentNullException(p);
            }
            catch (Exception e)
            {
                message = e.Message;
            }
            var t = message;
            return;
        }
        public class A
        {
            public static CustomException NewArgumentNullException(string message)
            {
                var e = new CustomException(message);
                return e;
            }
        }

        public class CustomException : Exception
        {
            string message = "";
            public CustomException(string message)
            {
                this.message = message;
            }
            public override string Message => this.message;
        }
    }
}
