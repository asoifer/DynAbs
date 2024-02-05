using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynAbs.Test.Cases
{
    class LocalEventHandler
    {
        static public event EventHandler LocalHandler;

        public static void Main()
        {
            LocalHandler = new EventHandler((x, y) => { });

            if (LocalHandler != null)
                LocalHandler(null, EventArgs.Empty);

            return;
        }
    }
}
