using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace DynAbs.Test.Cases
{
    class GetConstructor
    {
        public static void Main(string[] args)
        {
            var enterito = Singleton<B>.Instance();
        }
        class B
        {
            int campito;
            public B()
            {
                campito = 10;
            }
        }
        public static class Singleton<T> where T : class
        {
    		private static volatile T _instance;
            public static T Instance()
            {
                if (_instance == null)
                {
                    ConstructorInfo constructor = null;
                    constructor = typeof(T).GetConstructor(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public,
                        null, new Type[0], null);
                    _instance = (T)constructor.Invoke(null);
                }
                return _instance;
            }
        }
    }
}
