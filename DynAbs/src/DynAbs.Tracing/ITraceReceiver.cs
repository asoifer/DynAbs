using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DynAbs.Tracing
{
    public interface ITraceReceiver
    {
        void StartReceivingTrace();
        bool TraceReceived(TraceInfo info);
        TraceInfo Next();
        TraceInfo ObserveNext();
        void StopReceiving();
    }
}
