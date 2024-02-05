using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynAbs.Test.Cases
{
    class NewArrayInArgumenter
    {
        public static void Main(string[] args)
        {
            A a = new A();
            object o1 = new object();
            object o2 = new object();
            int x = a.DoSomething(new object[] {o1, o2});
            int y = x;
        }
        public class A
        {
            public int DoSomething(object[] arrType)
            {
                int x = 0;
                if (arrType.Count() == 0)
                {
                    x = 3;
                }
                return x + arrType[1].GetHashCode();
            }
        }
    }
}
