using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynAbs
{
    public class SlicerException : Exception
    {
        public SlicerException(string message) : base(message)
        {
            
        }
    }

    public class UnexpectedTrace : SlicerException
    {
        public UnexpectedTrace() : base("Tipo de traza no esperada")
        {

        }

        public UnexpectedTrace(ITraceConsumer traceConsumer) : base("Tipo de traza no esperada:\r\n" + GetText(traceConsumer))
        {

        }

        static string GetText(ITraceConsumer traceConsumer)
        {
            var list = traceConsumer.Stack.ToList();
            var take = Math.Min(10, list.Count);
            var sublist = list.Take(take);
            var sb = new StringBuilder();
            foreach (var s in sublist)
            {
                var code = s.CSharpSyntaxNode.ToString().Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries).First().Trim();
                var text = $"{s.FileName}-{s.FileId}-{s.Line}-{s.SpanStart}-{s.SpanEnd}-{s.TraceType}-{code}";
                sb.AppendLine(text);
            }
            return sb.ToString();
        }
    }

    public class UninitializedTerm : SlicerException
    {
        Term _term;
        public Term Term { get { return _term; } }

        public UninitializedTerm(Term term)
            : base("Término no inicializado en el PTG")
        {
            _term = term;
        }
    }

    public class NonGlobalUninitializedTerm : SlicerException
    {
        Term _term;
        public Term Term { get { return _term; } }

        public NonGlobalUninitializedTerm(Term term)
            : base("Non global term: {term}")
        {
            _term = term;
        }
    }

    public class ProgramException : SlicerException
    {
        public ProgramException()
            : base("Excepcion generada en programa, representada en backend")
        {

        }
    }

    public class CatchedProgramException : SlicerException
    {
        public CatchedProgramException()
            : base("Excepcion generada en programa catcheada por el usuario representada en backend")
        {

        }
    }

    public class SliceCriteriaReachedException : SlicerException
    {
        public SliceCriteriaReachedException()
            : base("Se alcanzó el criterio de slice")
        {

        }
    }

    public class GoToStatementException : SlicerException
    {
        public GoToStatementException() : base("We don't support GoTo expressions") { }
    }

    public class EnterSwitchException : SlicerException
    {
        public EnterSwitchException() : base("Entering to switch expression") { }
    }
}
