using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;
using System;
using System.Collections.Generic;
using System.Linq;
using DynAbs.Tracing;

namespace DynAbs
{
    public partial class ProcessConsumer
    {
        Term CreateArgument(IArgumentOperation operation)
        {
            var realTerm = Visit(operation);
            if (realTerm == null)
                return null;

            var newTerm = _termFactory.Create(operation, realTerm.Last.Symbol, realTerm.IsGlobal, TermFactory.GetFreshName());
            _broker.Assign(newTerm, realTerm);

            newTerm.IsOutOrRef = realTerm.IsOutOrRef;
            newTerm.IsRef = realTerm.IsRef;
            newTerm.ReferencedTerm = realTerm;
            return newTerm;
        }

        Term CreateArgument(ArgumentSyntax syntaxNode)
        {
            var realTerm = Visit(GetOperation(syntaxNode.Expression));
            if (realTerm == null)
                return null;

            var newTerm = _termFactory.Create(syntaxNode, realTerm.Last.Symbol, realTerm.IsGlobal, TermFactory.GetFreshName());
            _broker.Assign(newTerm, realTerm);

            newTerm.IsOutOrRef = realTerm.IsOutOrRef;
            newTerm.IsRef = realTerm.IsRef;
            newTerm.ReferencedTerm = realTerm;
            return newTerm;
        }

        Term CreateArgument(Term realTerm)
        {
            if (realTerm == null)
                return null;

            var newTerm = _termFactory.Create(realTerm.Stmt.CSharpSyntaxNode, realTerm.Last.Symbol, realTerm.IsGlobal, TermFactory.GetFreshName());
            _broker.Assign(newTerm, realTerm);

            newTerm.IsOutOrRef = realTerm.IsOutOrRef;
            newTerm.IsRef = realTerm.IsRef;
            newTerm.ReferencedTerm = realTerm;
            return newTerm;
        }

        bool HasAccesor(string Name, IDictionary<ITypeSymbol, ITypeSymbol> typesDictionary, out bool isSetAccessor, bool comesFromIndexedProperty = false)
        {
            var nextStatement = ObserveNextStatement(true, typesDictionary);
            var syntaxNode = nextStatement.CSharpSyntaxNode;
            isSetAccessor = false;

            if ((nextStatement.TraceType == TraceType.EnterMethod || nextStatement.TraceType == TraceType.EnterStaticMethod)
                && ((syntaxNode is ArrowExpressionClauseSyntax && (!(syntaxNode.Parent is MethodDeclarationSyntax)) &&
                ((syntaxNode.Parent is PropertyDeclarationSyntax && ((PropertyDeclarationSyntax)syntaxNode.Parent).Identifier.ValueText.Equals(Name)) ||
                (syntaxNode.Parent.Parent.Parent is PropertyDeclarationSyntax && ((PropertyDeclarationSyntax)syntaxNode.Parent.Parent.Parent).Identifier.ValueText.Equals(Name)))) ||
                (syntaxNode is AccessorDeclarationSyntax && ((comesFromIndexedProperty && syntaxNode.Parent.Parent is IndexerDeclarationSyntax) ||
                (!comesFromIndexedProperty && (Name == "" || (syntaxNode.Parent.Parent is PropertyDeclarationSyntax && 
                ((PropertyDeclarationSyntax)(syntaxNode).Parent.Parent).Identifier.ValueText.Equals(Name))))))))
            {
                isSetAccessor = ((syntaxNode is AccessorDeclarationSyntax) &&
                    ((AccessorDeclarationSyntax)syntaxNode).Keyword.ValueText.Equals("set", StringComparison.OrdinalIgnoreCase)) ||
                    ((syntaxNode is ArrowExpressionClauseSyntax) && (syntaxNode.Parent is AccessorDeclarationSyntax) &&
                    ((AccessorDeclarationSyntax)syntaxNode.Parent).Keyword.ValueText.Equals("set", StringComparison.OrdinalIgnoreCase));
                return true;
            }

            return false;
        }

        bool IsInstrumented(string invokedFunc, 
            IDictionary<ITypeSymbol, ITypeSymbol> typesDictionary = null,
            bool throwExceptionOnUnexpectedTrace = true, 
            bool structCall = false,
            IObjectCreationOperation objectCreationOperation = null, 
            int? @params = null, 
            bool isStatic = false,
            bool isProperty = false)
        {
            var stmt = ObserveNextStatement(true, typesDictionary);

            if (stmt.TraceType == TraceType.EndInvocation)
                return false;

            if (!Utils.IsEnterMethodOrConstructor(stmt.TraceType))
            {
                if (objectCreationOperation != null)
                {
                    var parentContainer = stmt.CSharpSyntaxNode.GetContainerOrConstructorInitializerSyntax();
                    if (parentContainer is ConstructorInitializerSyntax)
                    {
                        var constructor = parentContainer.GetContainer();
                        if (constructor is ConstructorDeclarationSyntax)
                        {
                            var constructorName = ((ConstructorDeclarationSyntax)constructor).Identifier.ValueText;
                            if (invokedFunc == constructorName)
                            {
                                // Look into the base call statement and get to the top
                                LookupForBaseCall((ConstructorInitializerSyntax)parentContainer);
                                return true;
                            }
                        }
                        else
                            throw new SlicerException("Unexpected behavior");
                    }
                }

                if (throwExceptionOnUnexpectedTrace && (Globals.wrap_structs_calls || !structCall))
                {
                    // TODOHACK
                    //if (stmt.TraceType == TraceType.EndMemberAccess)
                    //{
                    //    GetNextStatement();
                    //    return IsInstrumented(invokedFunc, typesDictionary, throwExceptionOnUnexpectedTrace, structCall, objectCreationOperation);
                    //}
                    return false;
                    throw new UnexpectedTrace(_traceConsumer);
                }
                else
                    return false;
            }

            CSharpSyntaxNode node = stmt.CSharpSyntaxNode;

            var methodName = "";

            if (node is ConstructorDeclarationSyntax)
                methodName = ((ConstructorDeclarationSyntax)node).Identifier.ValueText;
            else if (node is MethodDeclarationSyntax methodDeclarationSyntax)
            {
                methodName = methodDeclarationSyntax.Identifier.ValueText;
                if (invokedFunc == methodName && @params.HasValue &&
                    !methodDeclarationSyntax.ParameterList.Parameters.Any(x => x.ToString().Contains("params")))
                {
                    var paramCount = methodDeclarationSyntax.ParameterList.Parameters.Where(x => x.Default is null && !x.Modifiers.Any(y => y.ValueText == "this")).Count();
                    if (paramCount > @params.Value
                        //|| isStatic == methodDeclarationSyntax.IsStatic()
                        )
                        methodName = methodName + "!";
                }
            }
            else if (node is ArrowExpressionClauseSyntax && node.Parent is MethodDeclarationSyntax)
            {
                methodName = ((MethodDeclarationSyntax)((ArrowExpressionClauseSyntax)node).Parent).Identifier.ValueText;
                if (invokedFunc == methodName && isStatic != ((MethodDeclarationSyntax)((ArrowExpressionClauseSyntax)node).Parent).IsStatic())
                    methodName = methodName + "!";
            }
            else if (isProperty && node is ArrowExpressionClauseSyntax && node.Parent is PropertyDeclarationSyntax)
                methodName = ((PropertyDeclarationSyntax)((ArrowExpressionClauseSyntax)node).Parent).Identifier.ValueText;
            else if (isProperty && node is ArrowExpressionClauseSyntax && node.Parent is AccessorDeclarationSyntax && node.Parent.Parent.Parent is PropertyDeclarationSyntax)
                methodName = ((PropertyDeclarationSyntax)node.Parent.Parent.Parent).Identifier.ValueText;
            else if (node is LocalFunctionStatementSyntax)
                methodName = ((LocalFunctionStatementSyntax)node).Identifier.ValueText;
            else if (node is ArrowExpressionClauseSyntax && node.Parent is LocalFunctionStatementSyntax)
                methodName = ((LocalFunctionStatementSyntax)((ArrowExpressionClauseSyntax)node).Parent).Identifier.ValueText;
            else if (isProperty && node is AccessorDeclarationSyntax)
                methodName = ((PropertyDeclarationSyntax)node.Parent.Parent).Identifier.ValueText;
            else if (node is ClassDeclarationSyntax)
                methodName = ((ClassDeclarationSyntax)node).Identifier.ValueText;
            else if (node is ConversionOperatorDeclarationSyntax)
                methodName = ((ConversionOperatorDeclarationSyntax)node).Type.ToString();
            else if (node is ArrowExpressionClauseSyntax && node.Parent is ConversionOperatorDeclarationSyntax)
                methodName = ((ConversionOperatorDeclarationSyntax)((ArrowExpressionClauseSyntax)node).Parent).Type.ToString();
            else if (node is OperatorDeclarationSyntax)
                methodName = ((OperatorDeclarationSyntax)node).GetName();
            else if (node is ArgumentSyntax)
                methodName = ((ArgumentSyntax)node).GetContainer().GetName();
            //if (!isProperty && string.IsNullOrEmpty(methodName))
            //    throw new SlicerException("No se encontró el nombre del método de entrada");

            return invokedFunc == methodName;
        }

        void CheckExceptions()
        {
            if (_traceConsumer.HasNext())
            {
                var nextStatement = ObserveNextStatement();
                if (nextStatement.TraceType == TraceType.EnterCatch)
                    HandleCatch(_traceConsumer.GetNextStatement());
                else if (nextStatement.TraceType == TraceType.EnterFinalCatch)
                    HandleFinalCatch(_traceConsumer.GetNextStatement());
                else if (nextStatement.TraceType == TraceType.ExitLoopByException)
                {
                    var tobj = _enterLoopSet.Where(x => x.Key.FileId == nextStatement.FileId && x.Key.SpanStart == nextStatement.SpanStart && x.Key.SpanEnd == nextStatement.SpanEnd).First();
                    if (tobj.Value)
                        _broker.ExitStaticMode();
                    _enterLoopSet.Remove(tobj.Key);
                    CheckExceptions();
                }
            }
        }

        List<Term> GetParameters(CSharpSyntaxNode syntaxNode, List<Term> argumentList)
        {
            var parameterSyntax = new SeparatedSyntaxList<ParameterSyntax>();
            if (syntaxNode is MethodDeclarationSyntax)
                parameterSyntax = ((MethodDeclarationSyntax)syntaxNode).ParameterList.Parameters;
            if (syntaxNode is ArrowExpressionClauseSyntax && syntaxNode.Parent is MethodDeclarationSyntax)
                parameterSyntax = ((MethodDeclarationSyntax)syntaxNode.Parent).ParameterList.Parameters;
            if (syntaxNode is LocalFunctionStatementSyntax)
                parameterSyntax = ((LocalFunctionStatementSyntax)syntaxNode).ParameterList.Parameters;
            if (syntaxNode is ArrowExpressionClauseSyntax && syntaxNode.Parent is LocalFunctionStatementSyntax)
                parameterSyntax = ((LocalFunctionStatementSyntax)syntaxNode.Parent).ParameterList.Parameters;
            if (syntaxNode is OperatorDeclarationSyntax)
                parameterSyntax = ((OperatorDeclarationSyntax)syntaxNode).ParameterList.Parameters;
            if (syntaxNode is ConversionOperatorDeclarationSyntax)
                parameterSyntax = ((ConversionOperatorDeclarationSyntax)syntaxNode).ParameterList.Parameters;
            if (syntaxNode is ArrowExpressionClauseSyntax && syntaxNode.Parent is ConversionOperatorDeclarationSyntax)
                parameterSyntax = ((ConversionOperatorDeclarationSyntax)syntaxNode.Parent).ParameterList.Parameters;
            if (syntaxNode is AccessorDeclarationSyntax &&
                ((AccessorDeclarationSyntax)syntaxNode).Keyword.ValueText.Equals("set", StringComparison.OrdinalIgnoreCase))
            {
                if (syntaxNode.Parent.Parent is IndexerDeclarationSyntax)
                {
                    parameterSyntax = ((IndexerDeclarationSyntax)syntaxNode.Parent.Parent).ParameterList.Parameters;
                    var parameters = parameterSyntax.Select(x => _termFactory.CreateParameterTerm(x,
                            ISlicerSymbol.Create(_semanticModelsContainer.GetBySyntaxNode(x.Type).GetTypeInfo(x.Type).Type))).ToList();
                    parameters.Add(_termFactory.CreateValueParameterTerm(argumentList.First()));
                    return parameters;
                }
                return new List<Term>() { _termFactory.CreateValueParameterTerm(argumentList.Last()) };
            }
            else if (syntaxNode is ArrowExpressionClauseSyntax && syntaxNode.Parent is AccessorDeclarationSyntax &&
                ((AccessorDeclarationSyntax)syntaxNode.Parent).Keyword.ValueText.Equals("set", StringComparison.OrdinalIgnoreCase))
                return new List<Term>() { _termFactory.CreateValueParameterTerm(argumentList.Last()) };
            if (syntaxNode is AccessorDeclarationSyntax &&
                ((AccessorDeclarationSyntax)syntaxNode).Keyword.ValueText.Equals("get", StringComparison.OrdinalIgnoreCase) &&
                ((AccessorDeclarationSyntax)syntaxNode).Parent.Parent is IndexerDeclarationSyntax)
                parameterSyntax = ((IndexerDeclarationSyntax)(((AccessorDeclarationSyntax)syntaxNode).Parent.Parent)).ParameterList.Parameters;
            if (syntaxNode is ConstructorDeclarationSyntax)
                parameterSyntax = ((ConstructorDeclarationSyntax)syntaxNode).ParameterList.Parameters;

            return parameterSyntax.Select(x => _termFactory.CreateParameterTerm(x,
                    ISlicerSymbol.Create(_semanticModelsContainer.GetBySyntaxNode(x.Type).GetTypeInfo(x.Type).Type))).ToList();
        }

        List<ParameterSyntax> GetParameterSyntax(CSharpSyntaxNode syntaxNode)
        {
            var parameterSyntax = new SeparatedSyntaxList<ParameterSyntax>();
            if (syntaxNode is MethodDeclarationSyntax)
                parameterSyntax = ((MethodDeclarationSyntax)syntaxNode).ParameterList.Parameters;
            if (syntaxNode is ArrowExpressionClauseSyntax && syntaxNode.Parent is MethodDeclarationSyntax)
                parameterSyntax = ((MethodDeclarationSyntax)syntaxNode.Parent).ParameterList.Parameters;
            if (syntaxNode is LocalFunctionStatementSyntax)
                parameterSyntax = ((LocalFunctionStatementSyntax)syntaxNode).ParameterList.Parameters;
            if (syntaxNode is ArrowExpressionClauseSyntax && syntaxNode.Parent is LocalFunctionStatementSyntax)
                parameterSyntax = ((LocalFunctionStatementSyntax)syntaxNode.Parent).ParameterList.Parameters;
            if (syntaxNode is OperatorDeclarationSyntax)
                parameterSyntax = ((OperatorDeclarationSyntax)syntaxNode).ParameterList.Parameters;
            if (syntaxNode is ConversionOperatorDeclarationSyntax)
                parameterSyntax = ((ConversionOperatorDeclarationSyntax)syntaxNode).ParameterList.Parameters;
            if (syntaxNode is ArrowExpressionClauseSyntax && syntaxNode.Parent is ConversionOperatorDeclarationSyntax)
                parameterSyntax = ((ConversionOperatorDeclarationSyntax)syntaxNode.Parent).ParameterList.Parameters;
            if (syntaxNode is AccessorDeclarationSyntax &&
                ((AccessorDeclarationSyntax)syntaxNode).Keyword.ValueText.Equals("set", StringComparison.OrdinalIgnoreCase))
            {
                if (syntaxNode.Parent.Parent is IndexerDeclarationSyntax)
                {
                    parameterSyntax = ((IndexerDeclarationSyntax)syntaxNode.Parent.Parent).ParameterList.Parameters;
                    return parameterSyntax.ToList();
                }
                return new List<ParameterSyntax>() {  };
            }
            else if (syntaxNode is ArrowExpressionClauseSyntax && syntaxNode.Parent is AccessorDeclarationSyntax &&
                ((AccessorDeclarationSyntax)syntaxNode.Parent).Keyword.ValueText.Equals("set", StringComparison.OrdinalIgnoreCase))
                return new List<ParameterSyntax>() {  };
            if (syntaxNode is AccessorDeclarationSyntax &&
                ((AccessorDeclarationSyntax)syntaxNode).Keyword.ValueText.Equals("get", StringComparison.OrdinalIgnoreCase) &&
                ((AccessorDeclarationSyntax)syntaxNode).Parent.Parent is IndexerDeclarationSyntax)
                parameterSyntax = ((IndexerDeclarationSyntax)(((AccessorDeclarationSyntax)syntaxNode).Parent.Parent)).ParameterList.Parameters;
            if (syntaxNode is ConstructorDeclarationSyntax)
                parameterSyntax = ((ConstructorDeclarationSyntax)syntaxNode).ParameterList.Parameters;

            return parameterSyntax.ToList();
        }

        Stmt GetNextStatement(TraceType? traceType = null, bool throwException = true)
        {
            var stmt = ObserveNextStatement();
            if (traceType.HasValue && stmt.TraceType != traceType.Value)
            {
                if (stmt.TraceType == TraceType.EnterCatch)
                    HandleCatch(_traceConsumer.GetNextStatement());
                else if (stmt.TraceType == TraceType.EnterFinalCatch)
                    HandleFinalCatch(_traceConsumer.GetNextStatement());
                else if (throwException)
                    throw new UnexpectedTrace(_traceConsumer);
                else
                    return null;
            }
            return _traceConsumer.GetNextStatement();
        }

        Stmt OptionalGetNextStatement(TraceType traceType, int spanEnd)
        {
            var stmt = ObserveNextStatement();
            if (stmt.TraceType == traceType && stmt.SpanEnd == spanEnd)
                return _traceConsumer.GetNextStatement();
            return null;
        }

        Stmt ObserveNextStatement(bool consumeDispose = true, IDictionary<ITypeSymbol, ITypeSymbol> typesDictionary = null, bool allowNulls = false)
        {
            var tmp = _traceConsumer.ObserveNextStatement();
            while (tmp != null && (tmp.TraceType == TraceType.EnterStaticConstructor ||
                tmp.TraceType == TraceType.ExitLoopByException ||
                (consumeDispose && tmp.TraceType == TraceType.EnterMethod && 
                (tmp.CSharpSyntaxNode) is MethodDeclarationSyntax &&
                ((MethodDeclarationSyntax)tmp.CSharpSyntaxNode).Identifier.ValueText == "Dispose" &&
                !DealingWithDisposing())

                // XXX TODO YYY IMPORTANTE
                //|| (tmp.TraceType == TraceType.SimpleStatement &&
                //tmp.CSharpSyntaxNode is VariableDeclaratorSyntax &&
                //tmp.CSharpSyntaxNode.Parent.Parent is FieldDeclarationSyntax &&
                //((VariableDeclaratorSyntax)tmp.CSharpSyntaxNode).CustomIsStatic())

                ))
            {
                if (tmp.TraceType == TraceType.EnterStaticConstructor)
                    HandleInstrumentedMethod((Stmt)null /*XXX: para mi está bien*/, null, null, null, typesDictionary);
                else if (tmp.TraceType == TraceType.ExitLoopByException)
                {
                    var tobj = _enterLoopSet.Where(x => x.Key.FileId == tmp.FileId && x.Key.SpanStart == tmp.SpanStart && x.Key.SpanEnd == tmp.SpanEnd).FirstOrDefault();
                    if (tobj.Key == null)
                        ;
                    if (tobj.Value)
                        _broker.ExitStaticMode();
                    _enterLoopSet.Remove(tobj.Key);
                    _traceConsumer.GetNextStatement();
                }
                //else if(tmp.TraceType == TraceType.SimpleStatement)
                //{
                //    _traceConsumer.GetNextStatement();
                //    _executedStatements.Add(tmp);
                //    VisitPropertyOrFieldDeclaration(tmp);
                //}
                else
                    HandleBodyUnexpectedTrace(_traceConsumer.GetNextStatement());

                if (allowNulls && !_traceConsumer.HasNext())
                    tmp = null;
                else
                    tmp = _traceConsumer.ObserveNextStatement();
            }
            return tmp;
        }

        void ConsumeExitLoops()
        {
            var nextStmt = ObserveNextStatement();
            while (nextStmt.TraceType == TraceType.ExitLoop)
            {
                GetNextStatement();
                nextStmt = ObserveNextStatement();
            }
        }

        Stmt LookupForEnterConstructor(Stmt enterMethodStatement, int? argsSize = null, bool consume = true)
        {
            // HACK: Por ahora, si el proximo es BeforeConstructor, lo consumimos.
            var className = ((ClassDeclarationSyntax)enterMethodStatement.CSharpSyntaxNode).Identifier.ValueText;
            var fullClassName = ((ClassDeclarationSyntax)enterMethodStatement.CSharpSyntaxNode).Identifier.ValueText + ((ClassDeclarationSyntax)enterMethodStatement.CSharpSyntaxNode).TypeParameterList?.ToString();
            Queue<Stmt> queue = new Queue<Stmt>();
            var firstTimeBefore = false;
            while (true)
            {
                Stmt lookedUpStmt = null;
                try
                {
                    lookedUpStmt = _traceConsumer.GetNextStatement(false);
                }
                catch (Exception ex)
                {
                    // TODO: In this case is callback! (derived class outside the code non instrumented)
                    _traceConsumer.ReturnStatementsToBuffer(queue);
                    return null;
                }

                if (lookedUpStmt == null)
                {
                    _traceConsumer.ReturnStatementsToBuffer(queue);
                    return null;
                }

                var skip = false;
                if (lookedUpStmt.TraceType == TraceType.EnterConstructor ||
                    lookedUpStmt.TraceType == TraceType.BaseCall)
                {
                    string constructorClassName = null;
                    string fullConstructorClassName = null;
                    if (lookedUpStmt.CSharpSyntaxNode is ConstructorDeclarationSyntax)
                    {
                        constructorClassName = ((ConstructorDeclarationSyntax)lookedUpStmt.CSharpSyntaxNode).Identifier.ValueText;
                        if (lookedUpStmt.CSharpSyntaxNode.Parent is ClassDeclarationSyntax)
                            fullConstructorClassName = constructorClassName + ((ClassDeclarationSyntax)lookedUpStmt.CSharpSyntaxNode.Parent).TypeParameterList?.ToString();
                        else
                            fullConstructorClassName = constructorClassName;
                        //throw new SlicerException("Unexpected");
                    }
                    else if (lookedUpStmt.CSharpSyntaxNode is ClassDeclarationSyntax)
                    {
                        constructorClassName = ((ClassDeclarationSyntax)lookedUpStmt.CSharpSyntaxNode).Identifier.ValueText;
                        fullConstructorClassName = constructorClassName + ((ClassDeclarationSyntax)lookedUpStmt.CSharpSyntaxNode).TypeParameterList?.ToString();
                    }

                    // La condición larga es por si hay varios constructores, y uno llama a otro, queremos que coincidan la cantidad de argumentos.
                    // XXX: TODO: Creo que no cubre todos los casos, especialmente si se cumple la cantidad pero son de distinto tipo... (quiero pasar el test ComplexBase para un caso de Jolden)

                    var equalClassName = className == constructorClassName;
                    var newEqualClassName = fullClassName == fullConstructorClassName;
                    if (equalClassName != newEqualClassName)
                        ;

                    if (newEqualClassName &&
                        ((!(lookedUpStmt.CSharpSyntaxNode is ConstructorDeclarationSyntax))
                        || argsSize == null || (argsSize.Value == ((ConstructorDeclarationSyntax)lookedUpStmt.CSharpSyntaxNode).ParameterList.Parameters.Count) ||
                        (Utils.InRange(argsSize.Value, ((ConstructorDeclarationSyntax)lookedUpStmt.CSharpSyntaxNode).GetParametersRangeCount()))))
                    {

                        if (lookedUpStmt.TraceType == TraceType.EnterConstructor)
                        {
                            enterMethodStatement = lookedUpStmt;
                            if (!consume)
                                queue.Enqueue(lookedUpStmt);
                            else
                                _traceConsumer.AppendToStack(lookedUpStmt);
                            break;
                        }
                        else
                            skip = true;
                    }
                }
                else if (lookedUpStmt.TraceType == TraceType.BeforeConstructor &&
                    lookedUpStmt.CSharpSyntaxNode == enterMethodStatement.CSharpSyntaxNode)
                {
                    if (!firstTimeBefore)
                        firstTimeBefore = true;
                    else
                    {
                        // There is a callback for derived class
                        queue.Enqueue(lookedUpStmt);
                        _traceConsumer.ReturnStatementsToBuffer(queue);
                        return null;
                    }
                }
                if (!skip)
                    queue.Enqueue(lookedUpStmt);
                else
                    _traceConsumer.AppendToStack(lookedUpStmt);
            }
            _traceConsumer.ReturnStatementsToBuffer(queue);
            return enterMethodStatement;
        }

        Stmt LookupForBaseCall(ConstructorInitializerSyntax constructorInitializerSyntax)
        {
            var constructorDeclarationSyntax = ((ConstructorDeclarationSyntax)constructorInitializerSyntax.GetContainer());
            var className = constructorDeclarationSyntax.Identifier.ValueText;
            var argSize = constructorDeclarationSyntax.GetParametersCount();
            Stmt enterMethodStatement = null;
            Queue<Stmt> queue = new Queue<Stmt>();
            while (enterMethodStatement == null)
            {
                var lookedUpStmt = _traceConsumer.GetNextStatement(false);
                if (lookedUpStmt.TraceType == TraceType.BaseCall)
                {
                    string constructorClassName = null;
                    if (lookedUpStmt.CSharpSyntaxNode is ConstructorDeclarationSyntax)
                        constructorClassName = ((ConstructorDeclarationSyntax)lookedUpStmt.CSharpSyntaxNode).Identifier.ValueText;
                    else if (lookedUpStmt.CSharpSyntaxNode is ClassDeclarationSyntax)
                        constructorClassName = ((ClassDeclarationSyntax)lookedUpStmt.CSharpSyntaxNode).Identifier.ValueText;
                    if (className == constructorClassName &&
                        ((!(lookedUpStmt.CSharpSyntaxNode is ConstructorDeclarationSyntax))
                        || argSize == ((ConstructorDeclarationSyntax)lookedUpStmt.CSharpSyntaxNode).ParameterList.Parameters.Count))
                    {
                        enterMethodStatement = lookedUpStmt;
                        break;
                    }
                }
                queue.Enqueue(lookedUpStmt);
            }
            _traceConsumer.ReturnStatementsToBuffer(queue);
            _traceConsumer.ReturnStatementsToBuffer(new Queue<Stmt>(new Stmt[] { enterMethodStatement }));

            return enterMethodStatement;
        }

        Stmt LookupForBaseCall(CSharpSyntaxNode syntaxNode, int fileId)
        {
            Queue<Stmt> queue = new Queue<Stmt>();
            Stmt lookedUpStmt = null;
            while (true)
            {
                lookedUpStmt = _traceConsumer.GetNextStatement(false);
                if (lookedUpStmt == null)
                    throw new SlicerException($"Lookup for base call not found: {syntaxNode.ToString()}");

                if (lookedUpStmt.TraceType == TraceType.BaseCall &&
                    lookedUpStmt.SpanStart == syntaxNode.Span.Start
                    && lookedUpStmt.SpanEnd == syntaxNode.Span.End
                    && lookedUpStmt.FileId == fileId)
                {
                    _traceConsumer.ReturnStatementsToBuffer(queue);
                    var tempQueue = new Queue<Stmt>();
                    tempQueue.Enqueue(lookedUpStmt);
                    _traceConsumer.ReturnStatementsToBuffer(tempQueue);
                    break;
                }
                else
                    queue.Enqueue(lookedUpStmt);
            }

            return lookedUpStmt;
        }

        void ConsumeBeforeConstructor(CSharpSyntaxNode syntaxNode, int fileId)
        {
            Queue<Stmt> queue = new Queue<Stmt>();
            Stmt lookedUpStmt = null;
            while (true)
            {
                lookedUpStmt = _traceConsumer.GetNextStatement(false);
                if (lookedUpStmt == null)
                {
                    _traceConsumer.ReturnStatementsToBuffer(queue);
                    break;
                }

                if (lookedUpStmt.TraceType == TraceType.BeforeConstructor &&
                    lookedUpStmt.SpanStart == syntaxNode.Span.Start
                    && lookedUpStmt.SpanEnd == syntaxNode.Span.End
                    && lookedUpStmt.FileId == fileId)
                {
                    _traceConsumer.ReturnStatementsToBuffer(queue);
                    break;
                }
                else
                    queue.Enqueue(lookedUpStmt);
            }
        }

        List<Term> GetDependentTerms(CSharpSyntaxNode expression)
        {
            var returnTerms = new List<Term>();

            // Se entra en el caso de los diccionarios cuando hay 2 valores (key, value)
            if (expression is InitializerExpressionSyntax)
                foreach (var anotherExpression in ((InitializerExpressionSyntax)expression).Expressions)
                    returnTerms.AddRange(GetDependentTerms(anotherExpression));
            else
                returnTerms.Add(Visit(GetOperation(expression)));

            return returnTerms;
        }

        IOperation GetOperation(CSharpSyntaxNode syntaxNode)
        {
            if (syntaxNode is ParenthesizedExpressionSyntax)
                syntaxNode = ((ParenthesizedExpressionSyntax)syntaxNode).Expression;

            if (syntaxNode is RefExpressionSyntax)
                syntaxNode = ((RefExpressionSyntax)syntaxNode).Expression;

            if (syntaxNode is PostfixUnaryExpressionSyntax && ((PostfixUnaryExpressionSyntax)syntaxNode).OperatorToken.ValueText == "!")
                syntaxNode = ((PostfixUnaryExpressionSyntax)syntaxNode).Operand;

            var p_semanticModel = _semanticModelsContainer.GetBySyntaxNode(syntaxNode);
            var p_operation = p_semanticModel.GetOperation(syntaxNode);
            return p_operation;
        }

        bool DealingWithDisposing()
        {
            return _originalStatement != null &&
                _originalStatement.CSharpSyntaxNode is ExpressionStatementSyntax expressionStatementSyntax &&
                expressionStatementSyntax.Expression is InvocationExpressionSyntax invocationExpressionSyntax &&
                invocationExpressionSyntax.Expression is IdentifierNameSyntax identifierNameSyntax &&
                identifierNameSyntax.Identifier.Text == "Dispose";
        }
    }
}
