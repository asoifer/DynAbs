using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using DynAbs.Tracing;

namespace DynAbs
{
    public class Stmt
    {
        public int FileId { get; set; }
        private string fileName;
        public string FileName {
            get
            {
                if (fileName == null) 
                {
                    var path = Globals.InstrumentationResult.IdToFileDictionary[FileId];
                    fileName = System.IO.Path.GetFileName(path);
                }
                return fileName;
            }
            set
            {
                fileName = value;
            }
        }
        public int SpanStart { get; set; }
        public int SpanEnd { get; set; }
        private int line;
        public int Line
        {
            get
            {
                if (line == -1)
                {
                    InitializeNode();
                }
                return line;
            }
            set
            {
                line = value;
            }
        }

        private CSharpSyntaxNode syntaxNode;
        public CSharpSyntaxNode CSharpSyntaxNode
        {
            get
            {
                if (syntaxNode == null)
                {
                    InitializeNode();
                }
                return syntaxNode;
            }
            set
            {
                syntaxNode = value;
            }
        }

        private void InitializeNode()
        {
            try
            { 
                var ast = Globals.InstrumentationResult.fileIdToSyntaxTree[FileId];
                var span = Microsoft.CodeAnalysis.Text.TextSpan.FromBounds(SpanStart, SpanEnd);
                syntaxNode = (CSharpSyntaxNode)ast.GetCompilationUnitRoot().FindNode(span);
                var lineInRoslyn = syntaxNode.GetStatementContainer().GetLocation().GetLineSpan().StartLinePosition.Line;
                Line = lineInRoslyn + 1;
            }
            catch
            {
                ;
            }
        }

        public TraceType TraceType { get; set; }
        
        public override bool Equals(object obj)
        {
            var item = obj as Stmt;
            return ((item != null) && (obj == this || (SpanStart == item.SpanStart && SpanEnd == item.SpanEnd && FileId == item.FileId)));
        }

        public override int GetHashCode()
        {
            int prime = 31;
            int result = 1;
            result = prime * result + FileId;
            result = prime * result + SpanStart;
            result = prime * result + SpanEnd;
            return result;
        }

        public override string ToString()
        {
            return "Line=" + Line + ", Node=" + CSharpSyntaxNode;
        }
    }

    public class StmtFileAndLineEqualityComparer : IEqualityComparer<Stmt>
    {
        public bool Equals(Stmt x, Stmt y)
        {
            return x != null && y != null && x.FileId == y.FileId && x.Line == y.Line;
        }

        public int GetHashCode(Stmt obj)
        {
            return obj.FileId * 701 + obj.Line;
        }
    }

    public class StmtFileNameAndLineEqualityComparer : IEqualityComparer<Stmt>
    {
        public bool Equals(Stmt x, Stmt y)
        {
            return x != null && y != null && x.FileName == y.FileName && x.Line == y.Line;
        }

        public int GetHashCode(Stmt obj)
        {
            return obj.FileName.GetHashCode() + obj.Line * 701;
        }
    }
}