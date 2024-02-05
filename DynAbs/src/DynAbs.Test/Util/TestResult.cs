using DynAbs;
using System.Collections.Generic;

namespace DynAbs.Test.Util
{
    public class DynAbsResult
    {
        public Stmt Criteria { get; set; }
        public IList<Stmt> Executed { get; set; }
        public IList<Stmt> Sliced { get; set; }
        public IList<Stmt> StaticModeExtraLines { get; set; }
        public IList<Stmt> WithoutSummariesExtraLines { get; set; }

        public DynAbsResult()
        {
            Executed = new List<Stmt>();
            Sliced = new List<Stmt>();
            StaticModeExtraLines = new List<Stmt>();
            WithoutSummariesExtraLines = new List<Stmt>();
        }
    }

    public class DynAbsResultWithSkipFile : DynAbsResult
    {
        public string SkipFile { get; set; }

        public DynAbsResultWithSkipFile(string skipFile) : base()
        {
            SkipFile = skipFile;
        }
    }
}
