using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DynAbs.Tracing;

namespace DynAbs
{
    class TraceBufferedQueue
    {
        public int Count
        {
            get
            {
                return stack.Count;
            }
        }
        private Stack<Queue<Stmt>> stack = new Stack<Queue<Stmt>>();

        public Stmt Dequeue()
        {
            var actualQueue = stack.Peek();
            var stmt = actualQueue.Dequeue();
            if (actualQueue.Count == 0)
            {
                stack.Pop();
            }
            return stmt;
        }

        public Stmt Peek()
        {
            var actualQueue = stack.Peek();
            return actualQueue.Peek();
        }

        public void Enqueue(Queue<Stmt> stmts)
        {
            if (stmts.Count > 0)
            {
                stack.Push(stmts);
            }
        }
    }
}
