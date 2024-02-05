using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using DynAbs.Tracing;

namespace DynAbs.Tracing
{
    class TraceQueue
    {
        bool IsEmpty = true;
        Queue<TraceInfo> queue;

        public bool IsAddingComplete { get; set; }

        public TraceQueue(Queue<TraceInfo> queue)
        {
            this.queue = queue ?? new Queue<TraceInfo>();
        }

        internal TraceInfo Take()
        {
            TraceInfo info = null;
            lock (this)
            {
                while (!IsAddingComplete && IsEmpty)
                    Monitor.Wait(this, 1000);
                if (IsEmpty && IsAddingComplete) return null;
                info = queue.Dequeue();
                if (queue.Count == 0)
                {
                    IsEmpty = true;
                }
            }
            if (info == null)
            {
                throw new Exception("Info cannot be null");
            }
            return info;
        }

        internal void Add(TraceInfo trace)
        {
            lock (this)
            {
                queue.Enqueue(trace);
                if (queue.Count == 1)
                {
                    IsEmpty = false;
                    Monitor.Pulse(this);
                }
            }
        }

        internal TraceInfo Peek()
        {
            TraceInfo info = null;
            lock (this)
            {
                while (!IsAddingComplete && IsEmpty)
                    Monitor.Wait(this, 1000);
                if (IsEmpty && IsAddingComplete) return null;
                info = queue.Peek();
            }
            return info;
        }
    }
}
