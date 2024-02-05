using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DynAbs.Tracing;

namespace DynAbs
{
    public class ResultSummaryData
    {
        public string FilePath { get; set; }
        public int Line { get; set; }

        public int TotalTraceLines { get; set; }
        public double? TotalSkippedTrace { get; set; }
        public double? TotalReceivedTrace { get; set; }
        public int TotalStatements { get; set; }
        public int DistinctStatements { get; set; }
        public ISet<Stmt> SlicedStatements { get; set; }

        public TimeSpan ElapsedTime { get; set; }

        public ResultSummaryData(string filePath, int line, ITraceConsumer traceConsumer, ExecutedStatementsContainer container, TimeSpan elapsedTimeFromTheBeginning)
        {
            this.FilePath = filePath;
            this.Line = line;
            this.TotalTraceLines = traceConsumer.TotalTracedLines;
            this.TotalSkippedTrace = traceConsumer.TraceReceiver is LOTraceReceiver ? ((LOTraceReceiver)traceConsumer.TraceReceiver).SkippedCounter : (double?)null;
            this.TotalReceivedTrace = traceConsumer.TraceReceiver is LOTraceReceiver ? ((LOTraceReceiver)traceConsumer.TraceReceiver).Past.Count : (double?)null;
            this.DistinctStatements = container.DistinctExecutedLines;
            this.TotalStatements = container.ExecutedStatmentsCounter;
            this.ElapsedTime = elapsedTimeFromTheBeginning;
        }

        public ResultSummaryData(ITraceConsumer traceConsumer, ExecutedStatementsContainer container, TimeSpan elapsedTimeFromTheBeginning)
        {
            this.FilePath = null;
            this.Line = 0;
            this.TotalTraceLines = traceConsumer.TotalTracedLines;
            this.TotalSkippedTrace = traceConsumer.TraceReceiver is LOTraceReceiver ? ((LOTraceReceiver)traceConsumer.TraceReceiver).SkippedCounter : (double?)null;
            this.TotalReceivedTrace = traceConsumer.TraceReceiver is LOTraceReceiver ? ((LOTraceReceiver)traceConsumer.TraceReceiver).Past.Count : (double?)null;
            this.DistinctStatements = container.DistinctExecutedLines;
            this.TotalStatements = container.ExecutedStatmentsCounter;
            this.ElapsedTime = elapsedTimeFromTheBeginning;
        }
    }
}
