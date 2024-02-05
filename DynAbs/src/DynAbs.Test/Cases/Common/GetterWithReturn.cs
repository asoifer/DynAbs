using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynAbs.Test.Cases
{
    class GetterWithReturn
    {
        public static void Main(string[] args)
        {
            var aux = new Settings();
            aux = Settings.Default;
        }

        internal sealed partial class Settings
        {
            private static Settings defaultInstance = null;

            public static Settings Default
            {
                get
                {
                    return defaultInstance;
                }
            }
        }
    }
}
