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
    public uint DDGVerticesCount { get; }
    public uint DDGEdgesCount { get; }
    public uint MemoryModelVerticesCount { get; }
    public uint MemoryModelRegionsCount { get; }
    public uint MemoryModelEdgesCount { get; }
    #endregion

    #region Constructors
    public ResultSummaryData(string filePath,
        int line,
        ITraceConsumer traceConsumer,
        ExecutedStatementsContainer container,
        TimeSpan elapsedTimeFromTheBeginning) : this((string?)filePath, (int?)line, null, traceConsumer, container, null, null, elapsedTimeFromTheBeginning)
    {

    }

    public ResultSummaryData(string filePath, 
        int line, 
        UserSliceConfiguration userConfiguration, 
        ITraceConsumer traceConsumer, 
        ExecutedStatementsContainer container,
        IDependencyGraph dependencyGraph,
        IAliasingSolver aliasingSolver,
        TimeSpan elapsedTimeFromTheBeginning) : this((string?)filePath, (int?)line, userConfiguration, traceConsumer, container, dependencyGraph, aliasingSolver, elapsedTimeFromTheBeginning)
    {
        
    }

    public ResultSummaryData(UserSliceConfiguration userConfiguration, 
        ITraceConsumer traceConsumer, 
        ExecutedStatementsContainer container,
        IDependencyGraph dependencyGraph,
        IAliasingSolver aliasingSolver,
        TimeSpan elapsedTimeFromTheBeginning) : this((string?)null, (int?)0, userConfiguration, traceConsumer, container, dependencyGraph, aliasingSolver, elapsedTimeFromTheBeginning)
    {
        
    }

    ResultSummaryData(string? filePath,
        int? line,
        UserSliceConfiguration userConfiguration,
        ITraceConsumer traceConsumer,
        ExecutedStatementsContainer container,
        IDependencyGraph dependencyGraph,
        IAliasingSolver aliasingSolver,
        TimeSpan elapsedTimeFromTheBeginning)
    {
        FilePath = filePath;
        Line = line ?? 0;
        TotalTraceLines = traceConsumer.TotalTracedLines;
        TotalSkippedTrace = traceConsumer.TraceReceiver is LOTraceReceiver ? ((LOTraceReceiver)traceConsumer.TraceReceiver).SkippedCounter : (double?)null;
        TotalReceivedTrace = traceConsumer.TraceReceiver is LOTraceReceiver ? ((LOTraceReceiver)traceConsumer.TraceReceiver).Past.Count : (double?)null;
        DistinctStatements = container.DistinctExecutedLines;
        TotalStatements = container.ExecutedStatmentsCounter;
        ElapsedTime = elapsedTimeFromTheBeginning;

        DDGVerticesCount = dependencyGraph?.VertexCount ?? 0;
        DDGEdgesCount = dependencyGraph?.EdgeCount ?? 0;

        MemoryModelVerticesCount = 0;
        MemoryModelRegionsCount = 0;
        MemoryModelEdgesCount = 0;
        if (userConfiguration?.ComputeMemoryModelSize == true && aliasingSolver != null)
        {
            aliasingSolver.MeasureMemoryModelSize(out var mmTotalNodes, out var mmTotalRegions, out var mmTotalEdges);
            MemoryModelVerticesCount = mmTotalNodes;
            MemoryModelRegionsCount = mmTotalRegions;
            MemoryModelEdgesCount = mmTotalEdges;
        }
    }
    #endregion
}
