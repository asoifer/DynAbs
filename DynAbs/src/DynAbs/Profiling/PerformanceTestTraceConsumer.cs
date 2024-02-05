using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DynAbs.Tracing;

namespace DynAbs
{
    public class PerformanceTestTraceConsumer : ITraceConsumer
    {
        ITraceConsumer _traceConsumer;

        public Stack<Stmt> Stack { get { return _traceConsumer.Stack; } }
        public int TotalTracedLines { get { return _traceConsumer.TotalTracedLines; } }

        public PerformanceTestTraceConsumer(ITraceConsumer traceConsumer)
        {
            _traceConsumer = traceConsumer;
        }

        public Stmt GetNextStatement(bool consume = true)
        {
            GlobalPerformanceValues.TraceConsumerValues.Counters.GetNextStatement++;
            var start = DateTime.Now;
            var result = _traceConsumer.GetNextStatement(consume);
            var diff = DateTime.Now.Subtract(start);
            GlobalPerformanceValues.TraceConsumerValues.Times.GetNextStatement =
                GlobalPerformanceValues.TraceConsumerValues.Times.GetNextStatement.Add(diff);
            return result;
        }

        public void ReturnStatementsToBuffer(Queue<Stmt> stmts)
        {
            GlobalPerformanceValues.TraceConsumerValues.Counters.GetNextStatement++;
            var start = DateTime.Now;
            _traceConsumer.ReturnStatementsToBuffer(stmts);
            var diff = DateTime.Now.Subtract(start);
            GlobalPerformanceValues.TraceConsumerValues.Times.GetNextStatement =
                GlobalPerformanceValues.TraceConsumerValues.Times.GetNextStatement.Add(diff);
        }

        public Stmt ObserveNextStatement()
        {
            GlobalPerformanceValues.TraceConsumerValues.Counters.ObserveNextStatement++;
            var start = DateTime.Now;
            var result = _traceConsumer.ObserveNextStatement();
            var diff = DateTime.Now.Subtract(start);
            GlobalPerformanceValues.TraceConsumerValues.Times.ObserveNextStatement = 
                GlobalPerformanceValues.TraceConsumerValues.Times.ObserveNextStatement.Add(diff);
            return result;
        }

        public bool HasNext()
        {
            GlobalPerformanceValues.TraceConsumerValues.Counters.HasNext++;
            var start = DateTime.Now;
            var result = _traceConsumer.HasNext();
            var diff = DateTime.Now.Subtract(start);
            GlobalPerformanceValues.TraceConsumerValues.Times.HasNext =
                GlobalPerformanceValues.TraceConsumerValues.Times.HasNext.Add(diff);
            return result;
        }

        public bool SliceCriteriaReached()
        {
            GlobalPerformanceValues.TraceConsumerValues.Counters.SliceCriteriaReached++;
            var start = DateTime.Now;
            var result = _traceConsumer.SliceCriteriaReached();
            var diff = DateTime.Now.Subtract(start);
            GlobalPerformanceValues.TraceConsumerValues.Times.SliceCriteriaReached =
                GlobalPerformanceValues.TraceConsumerValues.Times.SliceCriteriaReached.Add(diff);
            return result;
        }

        public bool SliceCriteriaReached(Stmt stmt)
        {
            GlobalPerformanceValues.TraceConsumerValues.Counters.SliceCriteriaReached++;
            var start = DateTime.Now;
            var result = _traceConsumer.SliceCriteriaReached(stmt);
            var diff = DateTime.Now.Subtract(start);
            GlobalPerformanceValues.TraceConsumerValues.Times.SliceCriteriaReached =
                GlobalPerformanceValues.TraceConsumerValues.Times.SliceCriteriaReached.Add(diff);
            return result;
        }

        public bool RemoveCriteria()
        {
            return _traceConsumer.RemoveCriteria();
        }

        public void AppendToStack(Stmt stmt)
        {
            _traceConsumer.AppendToStack(stmt);
        }

        public ITraceReceiver TraceReceiver { get { return _traceConsumer.TraceReceiver; } }
    }
}
