using Antlr4.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace DynAbs.Summaries
{
    public class InterpretedAnnotation
    {
        public string Annotation;
        public RVType RV;
        public List<ETType> RVTypes = new List<ETType>();
        public string RVKind = "";
        public List<ET> RVMET = new List<ET>();
        public List<RO> RO = new List<RO>();

        public List<ETType> ToMatch = new List<ETType>();
        public ISet<string> Parameters = new HashSet<string>();

        public List<ETF> R;
        public List<ETF> W;

        public List<Connect> CN;

        public static InterpretedAnnotation Parse(string s)
        {
            var inputStream = new AntlrInputStream(s);
            var summariesLexerLexer = new SummariesLexer(inputStream);
            var commonTokenStream = new CommonTokenStream(summariesLexerLexer);
            var summariesParser = new SummariesParser(commonTokenStream);
            var sContext = summariesParser.s();
            var visitor = new SummariesVisitor();
            visitor.Visit(sContext);
            return visitor.annotation;
        }
    }

    public enum RVType
    {
        Null,
        Fresh,
        IsIn
    }

    public enum ROType
    {
        Single,
        Many
    }

    public enum BaseET
    {
        R,
        P,
        RV,
        RO,
        G
    }

    public enum ETTypeKind
    {
        Default,
        Element,
        Parametric
    }

    public class ETType
    {
        public ETTypeKind Kind;
        public string Name;
        public BaseET? @base;
        public int? ParamIndex;

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;
            var cObj = (ETType)obj;
            return cObj.Kind == this.Kind && cObj.Name == this.Name && cObj.@base == this.@base && cObj.ParamIndex == this.ParamIndex;
        }
    }

    public class ET
    {
        public ET()
        {
            chainOfFields = new List<string>();
            untilType = new List<ETType>();
            ofType = new List<ETType>();
            ofKind = new List<string>();
            ParamIndex = null;
        }

        public BaseET @base;
        public int? ParamIndex;
        public List<string> chainOfFields;
        public List<ETType> untilType;
        public List<ETType> ofType;
        public List<string> ofKind;
    }

    public class ETF
    {
        public ET et;
        public string field; // "", ?, @param or another string
    }

    public class Connect
    {
        public List<ET> met1;
        public List<ET> met2;
        public string field;
    }

    public class RO
    {
        public ROType Type;
        public List<ETType> ROTypes = new List<ETType>();
        public string Kind = "";
    }
}
