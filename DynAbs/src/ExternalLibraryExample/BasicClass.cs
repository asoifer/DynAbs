using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExternalLibraryExample
{
    public class BasicClass
    {
        public int f = 1;
        public BasicClass child = null;

        public static void Initialize(out BasicClass elem)
        {
            elem = new BasicClass();
            elem.child = new BasicClass();
            elem.child.f = 1;
        }
    }

    public class CustomString
    {
        public int Length { get; set; }
        public static implicit operator CustomString(string str)
        {
            var a = new CustomString();
            a.Length = str.Length;
            return a;
        }
    }
}
