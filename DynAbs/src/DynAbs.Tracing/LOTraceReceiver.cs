using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynAbs.Tracing
{
    public class LOTraceReceiver : ITraceReceiver
    {
        IAliasingSolver solver;
        StackTraceReceiver traceReceiver;
        public List<TraceInfo> Past;
        public IDictionary<TraceInfo, int> LastLoopPosition;
        public TraceInfo tmpNextStmt;

        #region TESTING VALUES
        public long SkippedCounter { get { return LoopsData.Values.Sum(x => x.Skipped); } }
        // For each iteration we look how many times we're skipping and how many times we're not skipping
        // If not, we want to know if the reason was the trace or the convergence of the PTG
        // If yes, we want to know how many iterations and the trace lenght
        public Dictionary<TraceInfo, LoopData> LoopsData = new Dictionary<TraceInfo, LoopData>();

        public class LoopData
        {
            public int YesCounter = 0;
            public int Skipped = 0;

            public int NoCounter { get { return NoForConvergence + NoForDifferentTrace; } }
            public int NotSkipped { get { return NotSkippedForConvergence + NotSkippedForDifferentTrace; } }

            public int NoForConvergence = 0;
            public int NoForDifferentTrace = 0;
            public int NotSkippedForConvergence = 0;
            public int NotSkippedForDifferentTrace = 0;
        }
        #endregion

        public LOTraceReceiver(IAliasingSolver solver, string traceInput)
        {
            this.solver = solver;
            traceReceiver = new StackTraceReceiver(traceInput);
            Past = new List<TraceInfo>();
            LastLoopPosition = new Dictionary<TraceInfo, int>();
        }

        public void StartReceivingTrace()
        {
            traceReceiver.StartReceivingTrace();
        }

        public bool TraceReceived(TraceInfo info)
        {
            return traceReceiver.TraceReceived(info);
        }

        public TraceInfo Next()
        {
            var retValue = ObserveNext();
            traceReceiver.Next();
            tmpNextStmt = null;

            Past.Add(retValue);
            if (retValue != null &&
                retValue.TraceType == TraceType.EnterCondition &&
                ControlManagement.IsLoopStatement(GetStatement(retValue)))
            { 
                LastLoopPosition[retValue] = Past.Count - 1;
                if (!LoopsData.ContainsKey(retValue))
                    LoopsData[retValue] = new LoopData();
            }

            //if (SkippedCounter > 0 && SkippedCounter % 50000 == 0)
            //    Console.WriteLine("Skipped " + SkippedCounter + " lines, " + DateTime.Now.ToString("HH:mm"));

            return retValue;
        }

        public TraceInfo ObserveNext()
        {
            if (tmpNextStmt != null)
                return tmpNextStmt;

            var mainNext = traceReceiver.ObserveNext();
            while (mainNext != null &&
                LastLoopPosition.ContainsKey(mainNext) &&
                mainNext.TraceType == TraceType.EnterCondition && 
                ControlManagement.IsLoopStatement(GetStatement(mainNext)))
            {
                var queue = new Queue<TraceInfo>();
                var methodsBalance = 0;
                var i = LastLoopPosition[mainNext];
                for (; i < Past.Count; i++)
                {
                    var next = traceReceiver.Next();
                    queue.Enqueue(next);
                    if (!next.Equals(Past[i]))
                    {
                        traceReceiver.ReturnToBuffer(queue);
                        break;
                    }

                    if (next.TraceType == TraceType.EnterMethod ||
                        next.TraceType == TraceType.EnterStaticMethod ||
                        next.TraceType == TraceType.EnterConstructor ||
                        next.TraceType == TraceType.EnterStaticConstructor)
                        methodsBalance++;
                    if (next.TraceType == TraceType.EnterFinalFinally)
                        methodsBalance--;
                }
                // Here we know if the next trace is equal or not. We just want to know if this has converged or not.
                var dataInstance = LoopsData[mainNext];

                if (i == Past.Count && solver.Converged() && methodsBalance == 0)
                {
                    dataInstance.YesCounter++;
                    dataInstance.Skipped += queue.Count;

                    Past.AddRange(queue);
                    LastLoopPosition[mainNext] = i;

                    mainNext = traceReceiver.ObserveNext();
                }
                else
                {
                    if (i == Past.Count)
                    {
                        dataInstance.NoForConvergence++;
                        dataInstance.NotSkippedForConvergence += queue.Count;

                        traceReceiver.ReturnToBuffer(queue);
                    }
                    else
                    {
                        dataInstance.NoForDifferentTrace++;
                        dataInstance.NotSkippedForDifferentTrace += queue.Count;
                    }
                    break;
                }
            }
            tmpNextStmt = mainNext;
            return mainNext;
        }

        public void StopReceiving()
        {
            traceReceiver.StopReceiving();
        }

        Stmt GetStatement(TraceInfo traceInfo)
        {
            var ret = new Stmt() { FileId = traceInfo.FileId, SpanStart = traceInfo.SpanStart, SpanEnd = traceInfo.SpanEnd };
            ret.Line = -1;
            ret.TraceType = traceInfo.TraceType;
            return ret;
        }

        public void PrintValues(string writeToFile)
        {
            var sb = new StringBuilder();
            foreach (var ld in LoopsData)
            {
                // FileName - Line - YesCounter - Skipped - No values - Description
                var stmt = GetStatement(ld.Key);
                sb.AppendLine(string.Format("{0},{1},{2},{3},{4},{5},{6},{7}\r\n{8}", 
                    stmt.FileName, stmt.Line, ld.Value.YesCounter, ld.Value.Skipped, 
                    ld.Value.NoForDifferentTrace, ld.Value.NotSkippedForDifferentTrace, 
                    ld.Value.NoForConvergence, ld.Value.NotSkippedForConvergence,
                    stmt.CSharpSyntaxNode.GetText().ToString().Trim()));
            }
            System.IO.File.WriteAllText(writeToFile, sb.ToString());
        }
    }
}
