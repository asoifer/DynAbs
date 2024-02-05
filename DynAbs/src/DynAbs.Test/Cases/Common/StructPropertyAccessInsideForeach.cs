using System;
using System.Collections.Generic;

namespace DynAbs.Test.Cases.Common
{
    class StructPropertyAccessInsideForeach
    {
        public static void Main()
        {
            var l = new List<MyCustomStruct>();
            l.Add(new MyCustomStruct() { Key = 1 });
            l.Add(new MyCustomStruct() { Key = 2 });
            l.Add(new MyCustomStruct() { Key = 3 });
            var acum = 0;
            foreach (var c in l)
            {
                ExternalLibraryExample.Binary.Test(c);
                ExternalLibraryExample.Binary.Test(c.Key);
                ExternalLibraryExample.Binary.Test(c.Key.ToString());
                acum += c.Key;
            }
            Console.WriteLine(acum);
            return;
        }

        struct MyCustomStruct
        {
            public int Key { get; set; }
        }
    }
}
