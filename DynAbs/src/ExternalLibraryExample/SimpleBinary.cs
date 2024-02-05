using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExternalLibraryExample
{
    public class SimpleBinary
    {
        public TestInterface s { get; set; }
    }

    public class SimpleExternalClass : SimpleVirtualExternalClass
    {
        public override TestInterface s { get; set; }
    }

    public class SimpleVirtualExternalClass
    {
        public virtual TestInterface s { get; set; }
    }
}
