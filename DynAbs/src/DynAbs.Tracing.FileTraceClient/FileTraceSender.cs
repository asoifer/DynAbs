using DynAbs.Tracing;
using System;
using Tracer;

namespace DynAbs.Tracing
{
    public static partial class TraceSender
    {
        static TraceSender()
        {
            TracerClient = new FileTracerClient();
            TracerClient.Initialize();
        }
    }
}