using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DynAbs.Tracing
{
    interface ITracerServer
    {
        ITraceReceiver Receiver {get;set;}
        void Initialize();
        void StartReceivingTrace();
    }
}
