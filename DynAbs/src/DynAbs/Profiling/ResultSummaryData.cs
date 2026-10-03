using System;
using System.Collections.Generic;
using DynAbs.Tracing;

namespace DynAbs;

public class ResultSummaryData
{
    #region Properties
    public string FilePath { get; }
    public int Line { get; }

    public int TotalTraceLines { get; }
    public double? TotalSkippedTrace { get; }
    public double? TotalReceivedTrace { get; }
    public int TotalStatements { get; }
    public int DistinctStatements { get; }
    public ISet<Stmt> SlicedStatements { get; set; }

    public TimeSpan ElapsedTime { get; }
    public uint VertexCounter { get; }
    public uint EdgeCounter { get; }
    #endregion

    #region Constructors
    public ResultSummaryData(string filePath, int line, ITraceConsumer traceConsumer, ExecutedStatementsContainer container, TimeSpan elapsedTimeFromTheBeginning, uint vertexCounter, uint edgeCounter)
    {
        FilePath = filePath;
        Line = line;
        TotalTraceLines = traceConsumer.TotalTracedLines;
        TotalSkippedTrace = traceConsumer.TraceReceiver is LOTraceReceiver ? ((LOTraceReceiver)traceConsumer.TraceReceiver).SkippedCounter : (double?)null;
        TotalReceivedTrace = traceConsumer.TraceReceiver is LOTraceReceiver ? ((LOTraceReceiver)traceConsumer.TraceReceiver).Past.Count : (double?)null;
        DistinctStatements = container.DistinctExecutedLines;
        TotalStatements = container.ExecutedStatmentsCounter;
        ElapsedTime = elapsedTimeFromTheBeginning;
        VertexCounter = vertexCounter;
        EdgeCounter = edgeCounter;
    }

    public ResultSummaryData(ITraceConsumer traceConsumer, ExecutedStatementsContainer container, TimeSpan elapsedTimeFromTheBeginning, uint vertexCounter, uint edgeCounter)
    {
        FilePath = null;
        Line = 0;
        SlicedStatements = new HashSet<Stmt>();
        TotalTraceLines = traceConsumer.TotalTracedLines;
        TotalSkippedTrace = traceConsumer.TraceReceiver is LOTraceReceiver ? ((LOTraceReceiver)traceConsumer.TraceReceiver).SkippedCounter : (double?)null;
        TotalReceivedTrace = traceConsumer.TraceReceiver is LOTraceReceiver ? ((LOTraceReceiver)traceConsumer.TraceReceiver).Past.Count : (double?)null;
        DistinctStatements = container.DistinctExecutedLines;
        TotalStatements = container.ExecutedStatmentsCounter;
        ElapsedTime = elapsedTimeFromTheBeginning;
        VertexCounter = vertexCounter;
        EdgeCounter = edgeCounter;
    }
    #endregion
}
