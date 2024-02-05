using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynAbs.Test.Cases
{
    class UsingWithIDisposableStruct
    {
        static void Main()
        {
            Console.WriteLine(MyIDisposableStruct.GetTheValue());
        }

        struct MyIDisposableStruct : IDisposable
        {
            string MyProperty { get; set; }

            MyIDisposableStruct(string s)
            {
                MyProperty = s;
            }

            public static string GetTheValue()
            {
                using (var writer = new MyIDisposableStruct("HELLO WORLD"))
                {
                    writer.GetTheValue("!");
                    return writer.MyProperty;
                }
            }

            string GetTheValue(string m)
            {
                MyProperty += m;
                return MyProperty;
            }

            public void Dispose()
            {
                Console.WriteLine("I'm being destroying");
            }
        }
    }
}
