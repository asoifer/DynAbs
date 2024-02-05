using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DynAbs.Test.Cases
{
    class SingletonA
    {
        public static void Main()
        {
            var a = Generador<A>.Instance;
            a.property = 1;
            var b = Generador<B>.Instance;
            b.property = 1;
            var c = Generador<A>.Instance.property;
            var d = c + 1;
        }

        class A
        {
            public int property { get; set; }
        }

        class B
        {
            public int property { get; set; }
        }

        public static class Generador<T> where T : class
        {
            private static volatile T _instance;
            private static readonly object _lock = new object();
            public static T Instance
            {
                get
                {
                    if (_instance == null)
                    {
                        lock (_lock)
                        {
                            ConstructorInfo constructor = null;
                            constructor = typeof(T).GetConstructor(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public, null, new Type[0], null);
                            
                            _instance = (T)constructor.Invoke(null);
                        }
                    }

                    return _instance;
                }
            }
        }
    }
}
