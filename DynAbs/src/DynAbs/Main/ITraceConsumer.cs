using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DynAbs.Tracing;

namespace DynAbs
{
    public interface ITraceConsumer
    {
        Stmt GetNextStatement(bool consume = true);
        void ReturnStatementsToBuffer(Queue<Stmt> stmts);
        Stmt ObserveNextStatement();
        bool HasNext();
        bool SliceCriteriaReached();
        bool SliceCriteriaReached(Stmt stmt);
        bool RemoveCriteria();
        void AppendToStack(Stmt stmt);

        Stack<Stmt> Stack { get; }
        int TotalTracedLines { get; }

        ITraceReceiver TraceReceiver { get; }
    }
}
