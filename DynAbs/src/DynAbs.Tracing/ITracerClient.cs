using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DynAbs.Tracing
{
    public interface ITracerClient
    {
        void Initialize();
        void Trace(int fileId, int traceType, int spanStart, int spanEnd);
    }
}
