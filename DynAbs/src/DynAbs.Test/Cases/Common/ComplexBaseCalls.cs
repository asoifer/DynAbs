using System;
using System.Collections.Generic;
using System.Text;

namespace DynAbs.Test.Cases.Common
{
    class ComplexBaseCalls
    {
        static void Main()
        {
            var a = new BoundAttribute("hello world", 1, false);
            var b = a != null;
            return;
        }
    }
    
    class BoundAttribute : BoundExpression
    {
        public BoundAttribute(string name, int anotherParam, bool hasErrors = false)
            : base(anotherParam, name, hasErrors || HasErrors(name))
        {

        }

        static bool HasErrors(string something)
        {
            return false;
        }
    }

    class BoundExpression : BoundNode
    {
        public BoundExpression(int kind, string name, bool hasErrors)
            : base(kind, name, hasErrors)
        {

        }
    }

    class BoundNode
    {
        protected BoundNode(int kind, string name)
        {
            
        }

        protected BoundNode(int kind, string name, bool hasErrors)
            : this(kind, name)
        {
            
        }
    }
}
