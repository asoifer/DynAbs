using System;
using System.Collections.Generic;
using DynAbs.Tracing;
using System.Linq;
using System.Threading;

namespace DynAbs.Tracing
{
    public class TraceReceiver : ITraceReceiver
    {
        UserSliceConfiguration configurartion;
        private ITracerServer tracerServer;
        private TraceQueue traceBuffer = new TraceQueue(null);
        public bool ReceivingStoped { get; set; }

        public TraceReceiver(UserSliceConfiguration userSliceConfiguration, string traceInput)
        {
            configurartion = userSliceConfiguration;

            // Open connection.
            if (traceInput == null)
            {
                tracerServer = new PipeTracerServer(this);
            }
            else
            {
                tracerServer = new FileTracerServer(this, traceInput);
            }
            tracerServer.Initialize();
            ReceivingStoped = false;
        }

        public void StartReceivingTrace()
        {
            tracerServer.StartReceivingTrace();
        }

        public bool TraceReceived(TraceInfo info)
        {
            if (!ReceivingStoped)
            {
                traceBuffer.Add(info);
                return true;
            }

            return false;
        }

        public TraceInfo Next()
        {
            TraceInfo temp;
            if (Globals.skip_trace_enabled)
            {
                do
                {
                    temp = traceBuffer.Take();
                } while (configurartion.FilesToSkip?.Contains(temp.FileId) == true);
            }
            else
            {
                temp = traceBuffer.Take();
            }

            return temp;
        }

        public TraceInfo ObserveNext()
        {
            TraceInfo temp = traceBuffer.Peek();

            if (Globals.skip_trace_enabled)
            {
                while (temp != null && configurartion.FilesToSkip?.Contains(temp.FileId) == true)
                {
                    traceBuffer.Take();
                    temp = traceBuffer.Peek();
                }
            }

            return temp;
        }

        public void StopReceiving()
        {
            traceBuffer.IsAddingComplete = true;
            ReceivingStoped = true;
        }
    }
}