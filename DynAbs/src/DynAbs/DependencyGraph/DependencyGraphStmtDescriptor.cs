using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DynAbs.Tracing;

namespace DynAbs
{
    public class DependencyGraphStmtDescriptor
    {
        Dictionary<Stmt, int> stmtOccurrenceCounter = new Dictionary<Stmt, int>();
        
        public string NodeDescription(Stmt stmt)
        {
            var node = stmt.CSharpSyntaxNode;
            string str = null;
            if (node is ParameterSyntax)
            {
                ParameterSyntax param = (ParameterSyntax)node;
                if (node.AncestorsAndSelf().OfType<MethodDeclarationSyntax>().Count() > 0)
                {
                    var methodDecl = node.AncestorsAndSelf().OfType<MethodDeclarationSyntax>().First();
                    string ret = "param " + methodDecl.Identifier.ValueText + "(" + param.ToString() + ")";
                    return Utils.CleanupDotNodeLabel(ret);
                }

                if (node.AncestorsAndSelf().OfType<OperatorDeclarationSyntax>().Count() > 0)
                {
                    var methodDecl = node.AncestorsAndSelf().OfType<OperatorDeclarationSyntax>().First();
                    string ret = "param " + methodDecl.OperatorToken.ValueText + "(" + param.ToString() + ")";
                    return Utils.CleanupDotNodeLabel(ret);
                }

                if (node.AncestorsAndSelf().OfType<ConstructorDeclarationSyntax>().Count() > 0)
                {
                    var methodDecl = node.AncestorsAndSelf().OfType<ConstructorDeclarationSyntax>().First();
                    string ret = "param " + methodDecl.Identifier.ValueText + "(" + param.ToString() + ")";
                    return Utils.CleanupDotNodeLabel(ret);
                }

                if (node.AncestorsAndSelf().OfType<IndexerDeclarationSyntax>().Count() > 0)
                {
                    var methodDecl = node.AncestorsAndSelf().OfType<IndexerDeclarationSyntax>().First();
                    string ret = "param " + methodDecl.ThisKeyword.ValueText + "(" + param.ToString() + ")";
                    return Utils.CleanupDotNodeLabel(ret);
                }

                if (node.AncestorsAndSelf().OfType<ConversionOperatorDeclarationSyntax>().Count() > 0)
                {
                    var methodDecl = node.AncestorsAndSelf().OfType<ConversionOperatorDeclarationSyntax>().First();
                    string ret = "param " + methodDecl.OperatorKeyword + "(" + param.ToString() + ")";
                    return Utils.CleanupDotNodeLabel(ret);
                }

                if (node.AncestorsAndSelf().OfType<LocalFunctionStatementSyntax>().Count() > 0)
                {
                    var methodDecl = node.AncestorsAndSelf().OfType<LocalFunctionStatementSyntax>().First();
                    string ret = "param " + methodDecl.Identifier.ValueText + "(" + param.ToString() + ")";
                    return Utils.CleanupDotNodeLabel(ret);
                }
            }
            else if (node is MethodDeclarationSyntax)
            {
                var mnode = node as MethodDeclarationSyntax;
                str = "param " + mnode.Identifier.ToString() + "(this)";
            }
            else if (node is ConstructorDeclarationSyntax)
            {
                var mnode = node as ConstructorDeclarationSyntax;
                str = "param " + mnode.Identifier.ToString() + "(this)";
            }
            else if (node is ClassDeclarationSyntax)
            {
                var mnode = node as ClassDeclarationSyntax;
                str = "param " + mnode.Identifier.ToString() + "(this)";
            }
            else
            {
                str = node.ToString();
            }
            return Utils.CleanupDotNodeLabel(str);
        }

        public string VertexNameForStmt(Stmt stmt)
        {
            // The name is the start of the span plus the number of occurrences. It is useful to accomplish the
            // Approach 3 of the "Dynamic Program Slicing" paper, that requires copy each statement appearance.
            return (stmt.FileId + ":[" + stmt.SpanStart + ".." + stmt.SpanEnd + ")." + stmtOccurrenceCounter[stmt]);
        }

        public const string VertexNameForExternal = "EXTERNAL";
        public const uint VertexIdForExternal = 1;

        public void UpdateStmtOcurrence(Stmt stmt)
        {
            if (stmtOccurrenceCounter.ContainsKey(stmt))
            {
                stmtOccurrenceCounter[stmt]++;
            }
            else
            {
                stmtOccurrenceCounter.Add(stmt, 1);
            }
        }
    }
}
