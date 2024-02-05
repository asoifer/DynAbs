using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynAbs.Test.Cases
{
    class LiteralString
    {
        static void Main(string[] args)
        {
            string a = "hello, world";                  // hello, world
            string b = @"hello, world";               // hello, world
            string c = "hello \t world";               // hello     world
            string d = @"hello \t world";               // hello \t world
            string e = "Joe said \"Hello\" to me";      // Joe said "Hello" to me
            string f = @"Joe said ""Hello"" to me";   // Joe said "Hello" to me
            string g = "\\\\server\\share\\file.txt";   // \\server\share\file.txt
            string h = @"\\server\share\file.txt";      // \\server\share\file.txt
            string i = "one\r\ntwo\r\nthree";

            string result = a + b;
            result = result + c;
            result = result + d;
            result = result + e;
            result = result + f;
            result = result + g;
            result = result + h;
            result = result + i;
            Console.WriteLine(result);
            return;
        }
    }
}
