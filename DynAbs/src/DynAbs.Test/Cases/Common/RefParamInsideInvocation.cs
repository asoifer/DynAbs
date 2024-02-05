using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynAbs.Test.Cases
{
    class RefParamInsideInvocation
    {
        static void Main(string[] args)
        {
            A a = new A();
            Method2(ref a);
        }

        public static int Method1(ref A a)
        {
            Method2(ref a);
            return 0;
        }

        public static int Method2(ref A a)
        {
            a = new A();
            return 0;
        }

        public class A
        {
        }
    }
}
