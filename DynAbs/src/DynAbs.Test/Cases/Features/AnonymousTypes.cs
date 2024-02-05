using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynAbs.Test.Cases
{
    class AnonymousTypes
    {
        public static void Main(string[] args)
        {
            int num = 42;
            var v = new { Amount = num, Message = "Hello" };
            Console.WriteLine(v.Amount + v.Message);
            return;
        }
    }
}
