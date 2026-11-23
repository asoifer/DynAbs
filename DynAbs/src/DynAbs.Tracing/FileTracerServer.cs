using System;
using System.IO;

namespace DynAbs.Tracing
{
    internal class FileTracerServer : ITracerServer
    {
        public ITraceReceiver Receiver { get; set; }
        public string TraceInput { get; set; }

        public FileTracerServer(ITraceReceiver receiver, string traceInput)
        {
            this.Receiver = receiver;
            if (string.IsNullOrEmpty(traceInput))
                throw new Exception("Trace input file must be defined");

            TraceInput = traceInput;
        }

        public void Initialize()
        {
        }

        public void StartReceivingTrace()
        {
            char separator = ',';
            var lines = File.ReadAllLines(TraceInput);
            foreach (var line in lines)
            {
                var data = line.Split(separator);
                if (data[0][0] == '#')
                    continue;
                var fileId = Convert.ToInt32(data[0]);
                var traceType = (TraceType)Convert.ToInt32(data[1]);
                var spanStart = Convert.ToInt32(data[2]);
                var spanEnd = Convert.ToInt32(data[3]);
                var counter = data.Length > 4 ? Convert.ToInt32(data[4]) : 0;
                TraceInfo info = new TraceInfo() { FileId = fileId, TraceType = traceType, SpanStart = spanStart, SpanEnd = spanEnd, Counter = counter };
                Receiver.TraceReceived(info);
            }
            Receiver.StopReceiving();
        }
    }
}
