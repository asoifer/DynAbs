using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DynAbs
{
    public class TermFactory
    {
        public static Term Create(IList<Field> parts, bool isGlobal, bool isTemporal, bool isCallbackAlloc)
        {
            var returnTerm = new Term(parts, isGlobal);
            returnTerm.IsScalar = parts.Last().Symbol.IsScalar;
            returnTerm.IsTemporal = isTemporal;
            returnTerm.IsCallbackAlloc = isCallbackAlloc;
            return returnTerm;
        }

        public Term Create(IOperation operation, ISlicerSymbol slicerSymbol, bool isStatic = false, string name = null, bool isTemporal = true, bool isCallbackAlloc = false)
        {
            return Create((CSharpSyntaxNode)operation.Syntax, slicerSymbol, isStatic, name, isTemporal, isCallbackAlloc);
        }

        public Term Create(CSharpSyntaxNode syntaxNode, ISlicerSymbol slicerSymbol, bool isStatic = false, string name = null, bool isTemporal = true, bool isCallbackAlloc = false)
        {
            var realSyntaxNode = GetNodeToUse(syntaxNode);

            var returnTerm = new Term(name ?? NameFromSyntaxNode(realSyntaxNode), slicerSymbol);
            returnTerm.IsScalar = slicerSymbol.IsScalar;
            returnTerm.IsGlobal = isStatic;
            returnTerm.Stmt = Utils.StmtFromSyntaxNode(realSyntaxNode, Globals.InstrumentationResult);
            returnTerm.IsTemporal = isTemporal;
            returnTerm.IsCallbackAlloc = isCallbackAlloc;
            return returnTerm;
        }

        public Term CreateParameterTerm(ParameterSyntax parameter, ISlicerSymbol slicerSymbol = null)
        {
            var name = parameter.Identifier.ValueText;
            var term = Create(parameter, slicerSymbol, false, name);

            // We just want to know if the parameter is object
            term.IsTypeObject = (parameter.Type.ToString() == "object");

            term.IsStruct = slicerSymbol.Symbol.CustomIsStruct();
            term.IsOutOrRef = parameter.Modifiers.Any(x => x.IsKind(SyntaxKind.RefKeyword) || x.IsKind(SyntaxKind.OutKeyword));
            term.IsTemporal = false;

            return term;
        }

        public Term CreateValueParameterTerm(Term bindingTerm)
        {
            var newTerm = new Term("value", bindingTerm.Last.Symbol);
            newTerm.Stmt = bindingTerm.Stmt;
            newTerm.IsScalar = bindingTerm.IsScalar;
            newTerm.IsTemporal = false;
            return newTerm;
        }

        public Term Copy(Term term, string id)
        {
            var newTerm = new Term(id, term.Last.Symbol);
            newTerm.IsGlobal = term.IsGlobal;
            newTerm.IsScalar = term.IsScalar;
            newTerm.IsTemporal = term.IsTemporal;
            return newTerm;
        }

        string NameFromSyntaxNode(CSharpSyntaxNode syntaxNode)
        {
            return string.Format("{0}/{1}-{2}",
                    Globals.InstrumentationResult.FileToIdDictionary[syntaxNode.SyntaxTree.FilePath],
                    syntaxNode.Span.Start,
                    syntaxNode.Span.End
                );
        }

        public static string GetFreshName()
        {
            return (++Globals.TermInternalCounter).ToString();
        }

        public static string GetFileTextBySyntaxNodeName(string syntaxNodeName)
        {
            var fileId = Convert.ToInt32(syntaxNodeName.Split('/')[0]);
            var spanStart = Convert.ToInt32((syntaxNodeName.Split('/')[1]).Split('-')[0]);
            var spanEnd = Convert.ToInt32((syntaxNodeName.Split('/')[1]).Split('-')[1]);
            return Globals.InstrumentationResult.fileIdToSyntaxTree[fileId].GetRoot()
                .DescendantNodes(x => x.Span.Start <= spanStart && x.Span.End >= spanEnd)
                .Where(x => x.Span.Start == spanStart && x.Span.End == spanEnd).First().GetText().ToString();
        }

        public static CSharpSyntaxNode GetNodeToUse(CSharpSyntaxNode node)
        {
            if (Globals.reduce_dg)
                return (CSharpSyntaxNode)node.GetStatementContainer();

            // Identity
            return node;
        }
    }
}
