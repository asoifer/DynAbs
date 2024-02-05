using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DynAbs.Tracing
{
    public class StackTraceReceiver : ITraceReceiver
    {
        ITracerServer tracerServer;
        TraceQueue traceBuffer;
        Stack<Queue<TraceInfo>> mainStack;
        public bool ReceivingStoped { get; set; }

        public StackTraceReceiver(string traceInput)
        {
            // Open connection.
            if (traceInput == null)
            {
                //tracerServer = new PipeTracerServer(this);
                throw new NotImplementedException();
            }
            else
            {
                tracerServer = new FileTracerServer(this, traceInput);
            }
            tracerServer.Initialize();
            ReceivingStoped = false;

            // We create the main queue
            var mainQueue = new Queue<TraceInfo>();
            traceBuffer = new TraceQueue(mainQueue);
            mainStack = new Stack<Queue<TraceInfo>>();
            mainStack.Push(mainQueue);
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
            if (mainStack.Count > 1)
            {
                var returnValue = mainStack.Peek().Dequeue();
                if (mainStack.Peek().Count == 0)
                    mainStack.Pop();
                return returnValue;
            }
            else
                return traceBuffer.Take();
        }

        public TraceInfo ObserveNext()
        {
            if (mainStack.Count > 1)
                return mainStack.Peek().Peek();
            else
                return traceBuffer.Peek();
        }

        public void StopReceiving()
        {
            traceBuffer.IsAddingComplete = true;
            ReceivingStoped = true;
        }

        public void ReturnToBuffer(Queue<TraceInfo> queue)
        {
            if (queue.Count > 0)
                mainStack.Push(queue);
        }
    }   
}
