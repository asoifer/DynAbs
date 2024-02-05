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
        #region Constructor
        public ProcessConsumer(
            UserSliceConfiguration userSliceConfiguration, 
            ITraceConsumer traceConsumer,
            IBroker broker,
            ISemanticModelsContainer semanticModelsContainer,
            InstrumentationResult instrumentationResult,
            TermFactory termFactory,
            ExecutedStatementsContainer executedStatements)
        {
            _configuration = userSliceConfiguration;
            _traceConsumer = traceConsumer;
            _semanticModelsContainer = semanticModelsContainer;
            _instrumentationResult = instrumentationResult;
            _termFactory = termFactory;
            _broker = broker;
            _executedStatements = executedStatements;
            _usingStack = new Stack<Term>();
        }
        #endregion

        #region Public
        public bool Process(Term term = null, List<Term> argumentList = null, Term @this = null, Stmt controlDependency = null, IDictionary<ITypeSymbol, ITypeSymbol> typeArguments = null, Term retExceptionTerm = null)
        {
            var enterMethodStatement = _traceConsumer.GetNextStatement();
            var isStatic = Utils.IsStaticTrace(enterMethodStatement.TraceType);

            if (!Utils.IsEnterMethodOrConstructor(enterMethodStatement.TraceType))
                throw new UnexpectedTrace(_traceConsumer);

            if (enterMethodStatement.TraceType == TraceType.BeforeConstructor)
            {
                // Va a haber un BeforeConstructor único por cada clase, por eso da bien el conteo
                // Si se entra por enter constructor no hace falta el base call del mismo método...
                _executedStatements.AddClass(enterMethodStatement);
                var temp = LookupForEnterConstructor(enterMethodStatement, argumentList != null ? (int?)argumentList.Count : null);
                if (temp == null)
                    ;
                enterMethodStatement = temp;
            }

            // Habrá un executed statement por cada método distinto
            if (enterMethodStatement == null || enterMethodStatement.CSharpSyntaxNode == null)
            {
                // In this case the constructor is not instrumented
                var currentOp = GetOperation(controlDependency.CSharpSyntaxNode);
                HandleNonInstrumentedMethod(currentOp, argumentList, @this, null, currentOp.Type, ".ctor");
                return false;
            }
            _executedStatements.AddMethod(enterMethodStatement);
            var syntaxNode = (CSharpSyntaxNode)enterMethodStatement.CSharpSyntaxNode;
            if (syntaxNode is ArgumentSyntax)
            {
                syntaxNode = (CSharpSyntaxNode)syntaxNode.GetContainer();
                ConsumeBeforeConstructor((CSharpSyntaxNode)syntaxNode.GetContainerClass(), enterMethodStatement.FileId);
            }

            _methodNode = syntaxNode;
            _currentFileId = enterMethodStatement.FileId;

            var currentSymbol = _semanticModelsContainer.GetBySyntaxNode(syntaxNode)
                .GetDeclaredSymbol((syntaxNode is ArrowExpressionClauseSyntax) ? syntaxNode.Parent : syntaxNode);
            var currentNameSymbol = Utils.GetNamespaceName(currentSymbol) + "." + Utils.GetClassName(currentSymbol) + "." + Utils.GetPropertyName(syntaxNode, currentSymbol);

            // XXX: Necesitamos saber si devuelve un tipo complejo (object por ejemplo) por si le mandamos un int (por ejemplo) y debe crear el nodo
            var returnTypeSyntax = syntaxNode.ReturnTypeSyntax();
            if (returnTypeSyntax != null)
            {
                if (returnTypeSyntax is RefTypeSyntax)
                    returnTypeSyntax = ((RefTypeSyntax)returnTypeSyntax).Type;
                _returnComplexType = ((ITypeSymbol)_semanticModelsContainer.GetBySyntaxNode(syntaxNode).GetSymbolInfo(returnTypeSyntax).Symbol).IsNotScalar();
            }

            _typeArguments = typeArguments;
            _returnObject = term;
            _returnExceptionTerm = retExceptionTerm;
            if (isStatic)
                @this = _thisObject = null; // Si la entrada es estática no hay this
            else
            {
                if (@this != null)
                    _thisObject = _termFactory.Create(syntaxNode, Utils.GetThisBySyntaxNode(syntaxNode, _semanticModelsContainer.GetBySyntaxNode(syntaxNode)), false, "this", false);
                else
                    ;
            }

            argumentList = argumentList ?? new List<Term>();
            var parameters = GetParameters(syntaxNode, argumentList);

            if (parameters.Count > argumentList.Count)
                ;

            if (argumentList.Count > parameters.Count)
                ;

            if (enterMethodStatement.FileId == 10091 && enterMethodStatement.Line == 385 && parameters.Count != argumentList.Count)
                ;

            _broker.EnterMethod(Utils.GetRealName(currentNameSymbol, typeArguments), argumentList, parameters, @this, _thisObject, controlDependency, enterMethodStatement);

            if (parameters.Count > argumentList.Count)
            {
                var parameterSyntax = GetParameterSyntax(syntaxNode);
                for (var i = argumentList.Count; i < parameterSyntax.Count; i++)
                {
                    if (parameterSyntax[i].Default != null)
                    {
                        var newTerm = _termFactory.CreateParameterTerm(parameterSyntax[i], ISlicerSymbol.Create(_semanticModelsContainer.GetBySyntaxNode(parameterSyntax[i]).GetTypeInfo(parameterSyntax[i].Type).Type));
                        _broker.DefUseOperation(newTerm);
                    }
                    else
                        ;
                }
            }

            // Setea los parámetros no definidos, ejemplo ARGS al comienzo, puede ser "peligroso" hacerlo así.
            if (argumentList.Count == 0 && (syntaxNode is MethodDeclarationSyntax || syntaxNode is LocalFunctionStatementSyntax))
                foreach (var param in syntaxNode is MethodDeclarationSyntax ? ((MethodDeclarationSyntax)syntaxNode).ParameterList.Parameters : ((LocalFunctionStatementSyntax)syntaxNode).ParameterList.Parameters)
                {
                    var newTerm = _termFactory.CreateParameterTerm(param, ISlicerSymbol.Create(_semanticModelsContainer.GetBySyntaxNode(param.Type).GetTypeInfo(param.Type).Type));
                    _broker.DefUseOperation(newTerm);
                    if (!newTerm.IsScalar)
                    {
                        _broker.Alloc(newTerm);
                        _broker.DefUseOperation(newTerm, new Term[] { });

                        var otherField = new Field(Field.SIGMA_FIELD);
                        if (!(newTerm.Parts[0].Symbol.Symbol is IArrayTypeSymbol))
                            ;
                        otherField.Symbol = ISlicerSymbol.Create(((IArrayTypeSymbol)newTerm.Parts[0].Symbol.Symbol).ElementType);

                        var otherTerm = newTerm.AddingField(otherField);
                        otherTerm.Stmt = newTerm.Stmt;
                        _broker.DefUseOperation(otherTerm, new Term[] { newTerm });
                    }
                }


            if (!isStatic && (syntaxNode is ConstructorDeclarationSyntax || syntaxNode is ClassDeclarationSyntax || syntaxNode is StructDeclarationSyntax))
                HandleBaseConstructor(term, syntaxNode);

            // Si entraste por BaseCall, tenés que consumir el enter constructor
            if (enterMethodStatement.TraceType == TraceType.BaseCall)
                GetNextStatement(TraceType.EnterConstructor);

            BodyConsume();
            return true;
        }

        public void FirstProcess(out string entryPointClassName)
        {
            // TODO: Esta garcha la voy a sacar
            entryPointClassName = "";
            var acumTerms = new List<Term>();
            try
            {
                while (_traceConsumer.HasNext())
                {
                    var nextStmt = _traceConsumer.ObserveNextStatement();

                    if (nextStmt.TraceType == TraceType.SimpleStatement)
                    {
                        _traceConsumer.GetNextStatement();
                        continue;
                    }

                    var isStatic = Utils.IsStaticTrace(nextStmt.TraceType);
                    var isDispose = ((nextStmt.CSharpSyntaxNode) is MethodDeclarationSyntax &&
                        ((MethodDeclarationSyntax)nextStmt.CSharpSyntaxNode).Identifier.ValueText == "Dispose");

                    if (isDispose)
                        break;

                    var processconsumer = new ProcessConsumer(_configuration, _traceConsumer, _broker, _semanticModelsContainer, Globals.InstrumentationResult, _termFactory, _executedStatements);
                    entryPointClassName = nextStmt.FileName;
                    // TODO: Si es estático pero devuelve algo no lo estamos capturando
                    //if (isStatic)
                    //    processconsumer.Process();
                    //else
                    //{
                    // 1era vez: inicializamos el contexto:
                    if (acumTerms.Count == 0)
                        _broker.EnterMethod("FirstProcess", new List<Term>(), new List<Term>(), null, null, null, null);

                    // Si entra acá captura todo en el WaitForEnd
                    acumTerms.AddRange(WaitForEnd(nextStmt.CSharpSyntaxNode, acumTerms, null, "FirstProcess", null, false));
                    //}
                }
            }
            catch (SliceCriteriaReachedException)
            {

            }
        }
        #endregion

        #region Private
        void BodyConsume()
        {
            Stmt currentStatement = null;
            var consumingFinallyAfterReturn = false;
            while (_traceConsumer.HasNext())
            {
                currentStatement = _traceConsumer.GetNextStatement();

                var @break = false;
                var @continue = false;

                if (_setReturn && currentStatement.TraceType == TraceType.ExitLoop)
                    @continue = true;

                if (!@continue)
                {
                    if (_setReturn && currentStatement.TraceType == TraceType.EnterFinally)
                        consumingFinallyAfterReturn = true;
                    else if (_setReturn && !consumingFinallyAfterReturn)
                        @break = true;
                    else if (_setReturn && consumingFinallyAfterReturn && currentStatement.TraceType == TraceType.EnterFinalFinally)
                        @break = true;
                }

                if (currentStatement.TraceType == TraceType.SimpleStatement)
                    _broker.SliceCriteriaReached = _traceConsumer.SliceCriteriaReached(currentStatement);
                else
                    _broker.SliceCriteriaReached = _traceConsumer.SliceCriteriaReached();

                if (@break)
                    break;
                if (@continue)
                    continue;

                switch (currentStatement.TraceType)
                {
                    case TraceType.EnterCondition: EnterCondition(currentStatement); break;
                    case TraceType.Break: _broker.Break(); break;
                    case TraceType.ExitUsing: HandleExitUsing(); break;
                    case TraceType.ExitCondition: ExitCondition(currentStatement); break;
                    case TraceType.ExitLoop: ExitLoop(currentStatement); break;
                    case TraceType.ExitStaticMethod:
                    case TraceType.ExitMethod:
                    case TraceType.ExitStaticConstructor:
                    case TraceType.ExitConstructor: ExitMethod(currentStatement); break;
                    case TraceType.EnterCatch: HandleCatch(currentStatement); break;
                    case TraceType.ExitCatch: ExceptionTerm = null; break;
                    case TraceType.EnterFinally: break;
                    case TraceType.ExitFinally: HandleFinnaly(currentStatement); break;
                    case TraceType.SimpleStatement: StatementConsume(currentStatement); break;
                    case TraceType.EnterFinalCatch: HandleFinalCatch(currentStatement); break;
                    default: HandleBodyUnexpectedTrace(currentStatement); break;
                }
            }
            while (currentStatement != null)
            {
                if (currentStatement.TraceType == TraceType.EnterFinalFinally)
                    break;

                switch (currentStatement.TraceType)
                {
                    case TraceType.ExitLoop: break;
                    default: HandleBodyUnexpectedTrace(currentStatement); break;
                }

                if (_traceConsumer.HasNext())
                    currentStatement = _traceConsumer.GetNextStatement();
                else
                    currentStatement = null;
            }
        }
                
        void StatementConsume(Stmt currentStatement)
        {
            if (_traceConsumer.TotalTracedLines >= Globals.LastTraceAmount)
            {
                Utils.Print(_traceConsumer.TotalTracedLines + " " + DateTime.Now.ToString("HH:mm"));
                Globals.LastTraceAmount += 75000;
            }

            var line = currentStatement.Line;
            if ((currentStatement.FileId == fileId && line == lineNumber)
                || (currentStatement.SpanStart == spanStart && currentStatement.SpanEnd == spanEnd))
                ;

            _originalStatement = currentStatement;
            _executedStatements.Add(currentStatement);

            try
            {
                var operation = GetOperation(currentStatement.CSharpSyntaxNode);
                if (operation == null && currentStatement.CSharpSyntaxNode is WhenClauseSyntax)
                    VisitCaseWhenSyntax(currentStatement);
                else if (operation == null)
                    VisitPropertyOrFieldDeclaration(currentStatement);
                else if (_methodNode is ArrowExpressionClauseSyntax && (_methodNode.Parent is PropertyDeclarationSyntax || _methodNode.Parent is ConversionOperatorDeclarationSyntax ||
                    (_methodNode.Parent is AccessorDeclarationSyntax &&
                    ((AccessorDeclarationSyntax)_methodNode.Parent).Keyword.ValueText.Equals("get", StringComparison.OrdinalIgnoreCase)) ||
                    ((_methodNode.Parent is MethodDeclarationSyntax || _methodNode.Parent is LocalFunctionStatementSyntax) &&
                    !((IMethodSymbol)_semanticModelsContainer.GetBySyntaxNode((CSharpSyntaxNode)_methodNode).GetDeclaredSymbol(_methodNode.Parent)).ReturnsVoid)))
                    VisitArrowExpressionClause(operation);
                else
                    Visit(operation);

                #region Mediciones
                #if DEBUG
                //GlobalPerformanceValues.MemoryConsumptionValues.Eval();

                if (_configuration.User.results.printGraphForEachStatement)
                    _broker.Solver.DumpPTG(System.IO.Path.Combine(_configuration.User.results.mainResultsFolder, _executedStatements.DistinctExecutedStatements + ".dot"),
                        (currentStatement.CSharpSyntaxNode).ToString().Length > 30 ? (currentStatement.CSharpSyntaxNode).ToString().Substring(0, 30)
                        : (currentStatement.CSharpSyntaxNode).ToString());
                #endif
                #endregion

                #region Extras
                if (_sliceCriteriaReached || (!_setReturn && _broker.SliceCriteriaReached))
                {
                    _broker.Slice(new ResultSummaryData(currentStatement.FileName, currentStatement.Line,
                        _traceConsumer, _executedStatements, DateTime.Now.Subtract(Globals.start_time)));

                    if (_configuration.User.criteria.mode != UserConfiguration.Criteria.CriteriaMode.AtEndWithCriteria
                        && _traceConsumer.RemoveCriteria())
                        throw new SliceCriteriaReachedException();
                }
                #endregion

            }
            catch (CatchedProgramException)
            {

            }
        }
        #endregion

        #region Helpers
        bool HandleInstrumentedMethod(Stmt controlDependency, Term term = null, List<Term> argumentList = null, Term @this = null, IDictionary<ITypeSymbol, ITypeSymbol> typeArguments = null)
        {
            Term retExceptionTerm = controlDependency != null ? _termFactory.Create(controlDependency.CSharpSyntaxNode, ISlicerSymbol.CreateObjectSymbol()) : null;
            
            var processConsumer = new ProcessConsumer(_configuration, _traceConsumer, _broker, _semanticModelsContainer, _instrumentationResult, _termFactory, _executedStatements);
            try
            {
                return processConsumer.Process(term, argumentList, @this, controlDependency, typeArguments, retExceptionTerm);
            }
            catch (ProgramException)
            {
                if (retExceptionTerm != null && retExceptionTerm.IsInitializedForException)
                    ExceptionTerm = retExceptionTerm;
                GetNextStatement(TraceType.EnterFinalFinally);
                CheckExceptions();
            }
            return true;
        }

        bool HandleInstrumentedMethod(IOperation operation, Term term = null, List<Term> argumentList = null, Term @this = null, IDictionary<ITypeSymbol, ITypeSymbol> typeArguments = null)
        {
            if (operation != null)
                return HandleInstrumentedMethod(Utils.StmtFromSyntaxNode((CSharpSyntaxNode)operation.Syntax, _instrumentationResult), term, argumentList, @this, typeArguments);
            else
                return HandleInstrumentedMethod((Stmt)null, term, argumentList, @this, typeArguments);
        }

        void HandleNonInstrumentedMethod(IOperation operation, List<Term> argumentList, Term @this, Term hub, ISymbol symbol, string methodName = null, SyntaxNode nodeToCheckException = null)
        {
            var dependentTerms = new List<Term>(argumentList);
            if (@this != null)
                dependentTerms.Add(@this);

            List<Term> returnedTerms = null;
            var thisStmt = ((hub ?? @this).Stmt);
            var spanStartToUse = nodeToCheckException != null ? nodeToCheckException.Span.Start : thisStmt.SpanStart;
            var spanEndToUse = nodeToCheckException != null ? nodeToCheckException.Span.End : thisStmt.SpanEnd;
            var isException = structsExceptions.Any(x => x.Item1 == thisStmt.FileId && x.Item2 < spanStartToUse && x.Item3 > spanEndToUse);
            if (isException)
                ;
            if (!Globals.wrap_structs_calls && symbol != null && symbol is IMethodSymbol && ((IMethodSymbol)symbol).MethodKind != MethodKind.Constructor && ((IMethodSymbol)symbol).CustomStructReceiver() && !isException)
                returnedTerms = new List<Term>();
            else
                returnedTerms = WaitForEnd(thisStmt.CSharpSyntaxNode, dependentTerms, symbol, methodName, TraceType.EndInvocation);

            // XXX: returnedTerms have to go with parameters... (for now)
            if (returnedTerms.Count > 0)
                argumentList.AddRange(returnedTerms);
            _broker.HandleNonInstrumentedMethod(argumentList, @this, returnedTerms, hub, symbol, methodName);
        }

        void CheckForSetCallbacks(Term def, Term use, List<Term> dependentTerms, IPropertySymbol propertySymbol)
        {
            var nextStmt = ObserveNextStatement();
            if (!Utils.IsEnterMethodOrConstructor(nextStmt.TraceType))
            {
                if (!Globals.properties_as_fields)
                    // Corresponde a la ejecución de código no instrumentado en una asignación // XXX: TODOSPEED (REVISAR)
                    _broker.HandleNonInstrumentedMethod(new List<Term>(), def.Parts.Count > 1 ? def.DiscardLast() : def, new List<Term>(), def, propertySymbol);
            }
            else
            {
                var receiver = def.Parts.Count > 1 ? def.DiscardLast() : def;

                var involvedTerms = dependentTerms != null ? new List<Term>(dependentTerms) : new List<Term>();
                involvedTerms.Add(receiver);
                involvedTerms.Add(use);
                var returnedTerms = WaitForEnd(def.Stmt.CSharpSyntaxNode, involvedTerms, propertySymbol, null, null);
                _broker.HandleNonInstrumentedMethod(dependentTerms ?? new List<Term>(), receiver, returnedTerms, null, propertySymbol);
            }
        }

        void CheckForMoveNextCallbacks(IForEachLoopOperation operation)
        {
            var term = _foreachHubsDictionary[(CSharpSyntaxNode)operation.Syntax];
            var currentStmt = term.Stmt;
            var returnedValues = WaitForEnd(term.Stmt.CSharpSyntaxNode, new List<Term>() { term }, operation.Collection.Type, "MoveNext");

            Term _definedVariable = null;
            List<Term> _definedVariables = new List<Term>();

            if (operation.LoopControlVariable is IDeclarationExpressionOperation)
            {
                var elements =
                    ((ITupleOperation)((IDeclarationExpressionOperation)operation.LoopControlVariable).Expression).Elements;
                foreach (var e in elements)
                    _definedVariables.Add(Visit(e));
            }
            else
                _definedVariable = _termFactory.Create((CSharpSyntaxNode)operation.Syntax,
                ISlicerSymbol.Create(((IVariableDeclaratorOperation)operation.LoopControlVariable).Symbol.Type), false, ((IVariableDeclaratorOperation)operation.LoopControlVariable).Symbol.Name, false);

            // XXX: O bien venimos del foreach y esperamos el entercondition o estamos en el exitcondition y esperamos un simple statement
            if (returnedValues.Count > 0)
            {
                if (_configuration.ForeachAnnotation || true)
                {
                    if (_definedVariable == null)
                        throw new NotImplementedException();

                    if (returnedValues.Count == 1)
                        _broker.Assign(_definedVariable, returnedValues.Single());
                    else if (returnedValues.Where(x => x.Last.Symbol.Equals(_definedVariable.Last.Symbol)).Count() == 1)
                    {
                        // TODO
                        var selected = returnedValues.Where(x => x.Last.Symbol.Equals(_definedVariable.Last.Symbol)).Single();
                        _broker.Assign(_definedVariable, selected);
                    }
                    else
                        // TODO: XXX: 
                        _broker.Assign(_definedVariable, returnedValues.Last());
                    //throw new NotImplementedException();
                }
                else
                    // TODO XXX
                    _broker.CustomEvent(new List<Term>(), term, returnedValues, _definedVariable, "MoveNext");
            }
            else
            {
                if (_configuration.ForeachAnnotation)
                {
                    _broker.CustomEvent(new List<Term>(), term, new List<Term>(), null, "MoveNext");
                    if (_definedVariable != null)
                        _broker.CustomEvent(new List<Term>(), term, new List<Term>(), _definedVariable, "Current");
                    else
                        foreach (var defVar in _definedVariables)
                            _broker.CustomEvent(new List<Term>(), term, new List<Term>(), defVar, "Current");
                }
                else
                {
                    if (_definedVariable != null)
                    {
                        var currentTerm = term.AddingField("Current", _definedVariable.Last.Symbol);
                        currentTerm.Stmt = currentStmt;
                        _broker.Assign(_definedVariable, currentTerm);
                    }
                    else
                    {
                        foreach (var defVar in _definedVariables)
                        {
                            var currentTerm = term.AddingField("Current", defVar.Last.Symbol);
                            currentTerm.Stmt = currentStmt;
                            _broker.Assign(defVar, currentTerm);
                        }
                    }
                }
            }
        }

        void HandleBaseConstructor(Term term, CSharpSyntaxNode syntaxNode)
        {
            var arguments = new List<Term>();
            // Caso llamado a base explicito
            if (syntaxNode is ConstructorDeclarationSyntax && ((ConstructorDeclarationSyntax)syntaxNode).Initializer != null)
                try 
                {
                    // TODOTEMP
                    // ((ConstructorDeclarationSyntax)syntaxNode).Initializer.ArgumentList.Arguments.ToList().ForEach(x => arguments.Add(CreateArgument(Visit(GetOperation(x.Expression)))));
                    var baseInvocation = (IInvocationOperation)GetOperation(((ConstructorDeclarationSyntax)syntaxNode).Initializer);

                    if (baseInvocation.Arguments.Length == baseInvocation.TargetMethod.Parameters.Length)
                    {
                        var argDict = new Dictionary<IArgumentOperation, Term>();
                        foreach (var arg in baseInvocation.Arguments)
                        {
                            var argTerm = CreateArgument(arg);
                            argDict.Add(arg, argTerm);
                        }
                        arguments = baseInvocation.TargetMethod.Parameters
                            .Select(x => argDict[baseInvocation.Arguments.Single(y => y.Parameter.Equals(x))]).ToList();
                    }
                    else
                        // Default, as always, TODO: do the same with constructors, or not?
                        arguments = baseInvocation.Arguments.Select(x => CreateArgument(x)).Where(x => x != null).ToList();
                }
                catch (Exception ex)
                {
                    throw;
                    ;
                }

            // Caso llamado al base implicito.
            // Puede ocurrir que estés invocando a un constructor propio! Entonces no va a venir otro BeforeConstructor sino un EnterMethod de la misma clase.
            // XXX: Los chequeos de tipos "ClassDeclarationSyntax" por ahí están de más o deberían ir según distintos casos
            var nextStmt = ObserveNextStatement();
            var mayInitializerSyntax = nextStmt.CSharpSyntaxNode.GetContainerOrConstructorInitializerSyntax();
            if (nextStmt.TraceType == TraceType.EnterConstructor && nextStmt.CSharpSyntaxNode == syntaxNode)
            {
                // Se llamó a un base call, pero te encontrás con vos mismo, entonces podemos asumir que hubo un no instrumentado.
                // Pero no tiró callbacks,... no hay wait for end. 
                var execSymbol = _semanticModelsContainer.GetBySyntaxNode(syntaxNode).GetDeclaredSymbol(syntaxNode);
                _broker.HandleNonInstrumentedMethod(arguments, _thisObject, new List<Term>(), null, execSymbol);
            }
            else if (nextStmt.TraceType != TraceType.BeforeConstructor &&
                nextStmt.TraceType != TraceType.BaseCall &&
                nextStmt.TraceType != TraceType.EnterConstructor &&
                mayInitializerSyntax is ConstructorInitializerSyntax &&
                ((mayInitializerSyntax.Parent.Parent is ClassDeclarationSyntax &&
                ((ClassDeclarationSyntax)mayInitializerSyntax.Parent.Parent).Identifier.ValueText
                == ((ClassDeclarationSyntax)syntaxNode.Parent).Identifier.ValueText) ||
                (mayInitializerSyntax.Parent.Parent is StructDeclarationSyntax &&
                ((StructDeclarationSyntax)mayInitializerSyntax.Parent.Parent).Identifier.ValueText
                == ((StructDeclarationSyntax)syntaxNode.Parent).Identifier.ValueText)))
            {
                var enterConstructor = LookupForBaseCall((CSharpSyntaxNode)((ConstructorInitializerSyntax)mayInitializerSyntax).Parent, nextStmt.FileId);
                HandleInstrumentedMethod(Utils.StmtFromSyntaxNode(enterConstructor.CSharpSyntaxNode, _instrumentationResult), term, arguments, _thisObject, _typeArguments);
                nextStmt = ObserveNextStatement();
            }
            else if (nextStmt.TraceType == TraceType.BeforeConstructor ||
                nextStmt.TraceType == TraceType.BaseCall ||
                ((nextStmt.TraceType == TraceType.EnterConstructor) &&
                (
                (syntaxNode.Parent is ClassDeclarationSyntax &&
                (nextStmt.CSharpSyntaxNode).Parent is ClassDeclarationSyntax &&
                ((ClassDeclarationSyntax)syntaxNode.Parent).Identifier.ValueText ==
                ((ClassDeclarationSyntax)(nextStmt.CSharpSyntaxNode).Parent).Identifier.ValueText) ||
                (syntaxNode.Parent is StructDeclarationSyntax &&
                (nextStmt.CSharpSyntaxNode).Parent is StructDeclarationSyntax &&
                ((StructDeclarationSyntax)syntaxNode.Parent).Identifier.ValueText ==
                ((StructDeclarationSyntax)(nextStmt.CSharpSyntaxNode).Parent).Identifier.ValueText))

                ))
            {
                INamedTypeSymbol namedType = null;
                BaseTypeSyntax complexTypeName = null;
                if (syntaxNode is ClassDeclarationSyntax)
                    // TODO: Solo va a haber un único nombre de clase raro, los demás serán interfaces. CASO: .NET Frameworks test
                    complexTypeName = ((ClassDeclarationSyntax)syntaxNode).BaseList.Types.FirstOrDefault(x => x.Type is GenericNameSyntax);
                else if (syntaxNode is StructDeclarationSyntax)
                    complexTypeName = ((StructDeclarationSyntax)syntaxNode).BaseList.Types.FirstOrDefault(x => x.Type is GenericNameSyntax);
                if (complexTypeName != null)
                    namedType = (INamedTypeSymbol)_semanticModelsContainer.GetBySyntaxNode(complexTypeName).GetTypeInfo((GenericNameSyntax)complexTypeName.Type).Type;

                HandleInstrumentedMethod(Utils.StmtFromSyntaxNode(syntaxNode, _instrumentationResult), term, arguments, _thisObject, Utils.GetTypesDictionary(namedType, _typeArguments));
            }
            // Las clases heredan por default de object y los structs de System.ValueType
            else if (syntaxNode is ConstructorDeclarationSyntax || syntaxNode is ClassDeclarationSyntax)
            {
                var semanticModel = _semanticModelsContainer.GetBySyntaxNode(syntaxNode);
                INamedTypeSymbol typeSymbol = null;
                if (syntaxNode is ClassDeclarationSyntax)
                {
                    if (((ClassDeclarationSyntax)(syntaxNode)).BaseList != null)
                        foreach (var t in ((ClassDeclarationSyntax)(syntaxNode)).BaseList.Types)
                        {
                            var typeInfo = semanticModel.GetTypeInfo(t.Type);
                            if (typeInfo.Type != null && typeInfo.Type.TypeKind == TypeKind.Class)
                                typeSymbol = (INamedTypeSymbol)typeInfo.Type;
                        }
                }
                else
                {
                    var symbol = semanticModel.GetDeclaredSymbol((ConstructorDeclarationSyntax)syntaxNode);
                    typeSymbol = symbol.ReceiverType.BaseType;
                }

                if (typeSymbol != null && typeSymbol.ToString() != "object" && typeSymbol.ToString() != "System.ValueType")
                {
                    // XXX: Por default tomamos el 1ero que tiene
                    // Lo mejor es mandar el símbolo correcto pero no se puede, ya que hay que preguntar por el tipo de 
                    // los argumentos y el IsAssignable y el params[] y todo eso para que después el summaries no diferencie la aridad de los constructores
                    #region Guardar
                    //// TODO: Puede pinchar con el params[]...
                    //var constructorSymbols = symbol.ReceiverType.BaseType.Constructors.Where(x => x.Arity == baseArity);
                    //ISymbol mainConstructorSymbol = constructorSymbols.FirstOrDefault();
                    //if (baseCall != null)
                    //    foreach(var constructorSymbol in constructorSymbols)
                    //    {
                    //        var typeArguments = constructorSymbol.TypeArguments;
                    //        for(var i = 0; i < typeArguments.Count(); i++)
                    //        {
                    //            var arg = baseCall.ArgumentList.Arguments[i];
                    //            var typeArg = typeArguments[i];
                    //            // ACÁ QUERRÍA PREGUNTAR ISASSIGNABLEFROM POR CADA PARAM/ARG PERO NO SE PUEDE SABER EL TIPO, SOLO EL SÍMBOLO
                    //            // DEJO ACÁ, POR LA EXPLICACIÓN ANTERIOR (VER ESTA REGIÓN)
                    //        }
                    //    }
                    #endregion

                    var executedSymbol = typeSymbol.Constructors.FirstOrDefault();

                    // A esta altura no tiene inicializados los fields externos
                    _broker.DefUseOperation(_thisObject);
                    _broker.HandleNonInstrumentedMethod(arguments, _thisObject, new List<Term>(), null, executedSymbol);

                    var dependentTerms = new List<Term>(arguments);
                    dependentTerms.Add(_thisObject);
                    // TODO: Al estar mandando null en el returnValue si cae un callback pincha, 
                    // XXX: Por ahora mandamos returned values a argumentos
                    var returnTerms = WaitForEnd(_thisObject.Stmt.CSharpSyntaxNode, dependentTerms, typeSymbol, "ctor", (TraceType?)null);
                    if (returnTerms.Count > 0)
                        arguments.AddRange(returnTerms);

                    _broker.HandleNonInstrumentedMethod(arguments, _thisObject, returnTerms, null, executedSymbol);
                }
            }
        }

        List<Term> WaitForEnd(CSharpSyntaxNode node, List<Term> involvedTerms, ISymbol caller, string callerMethodName = null, TraceType? finalTraceType = null, bool consume = true)
        {
            var nextStmt = ObserveNextStatement();
            // Términos que se van retornando. Qué hacemos con eso.
            var returnTerms = new List<Term>();
            // REPRESENTARÁ LA UNIÓN DE LO QUE TENEMOS
            var regionHub = _termFactory.Create(node, ISlicerSymbol.CreateObjectSymbol(), false, TermFactory.GetFreshName(), true, true);
            regionHub.ReferencedTerm = regionHub;
            bool regionCreated = false;
            // Para chequear entrada a modo estático
            var enterCallbackTraces = new HashSet<Stmt>();
            Stmt lastCallback = null;
            var enterStaticLoop = false;

            while (nextStmt != null && Utils.IsEnterMethodOrConstructor(nextStmt.TraceType))
            {
                // Caso particular del callback del ToString
                if (nextStmt.TraceType == TraceType.EnterMethod && involvedTerms.Count == 1 && nextStmt.CSharpSyntaxNode is MethodDeclarationSyntax methodDeclarationSyntax && methodDeclarationSyntax.ParameterList.Parameters.Count == 0)
                {
                    var methodSymbol = _semanticModelsContainer.GetBySyntaxNode(nextStmt.CSharpSyntaxNode).GetDeclaredSymbol(nextStmt.CSharpSyntaxNode);
                    if (methodSymbol != null && methodSymbol.Name == "ToString")
                    {
                        var stringTerm = _termFactory.Create(node, ISlicerSymbol.Create(((IMethodSymbol)methodSymbol).ReturnType), false, TermFactory.GetFreshName());
                        HandleInstrumentedMethod(Utils.StmtFromSyntaxNode(node, _instrumentationResult), stringTerm, null, involvedTerms.Single(), _typeArguments);
                        returnTerms.Add(stringTerm);
                        nextStmt = ObserveNextStatement();
                        continue;
                    }
                }

                // Estamos repitiendo entrada, potencialmente podría haber un loop externo
                if (!enterStaticLoop && lastCallback != null && lastCallback.Equals(nextStmt))
                    enterStaticLoop = _broker.EnterStaticMode(true);

                if (enterStaticLoop && lastCallback != null && !lastCallback.Equals(nextStmt))
                {
                    _broker.ExitStaticMode();
                    enterStaticLoop = false;
                }

                lastCallback = nextStmt;

                // Obtenemos el nodo de la entrada al método
                CSharpSyntaxNode currentSyntaxNode = null;
                if (nextStmt.TraceType == TraceType.BeforeConstructor)
                {
                    var enterConstructorStatement = LookupForEnterConstructor(nextStmt, null, false);
                    if (enterConstructorStatement == null)
                    {
                        GetNextStatement();
                        nextStmt = _traceConsumer.HasNext() ? ObserveNextStatement(allowNulls: true) : null;
                        continue;
                    }
                    currentSyntaxNode = enterConstructorStatement.CSharpSyntaxNode;
                }
                else
                    currentSyntaxNode = nextStmt.CSharpSyntaxNode;

                ISlicerSymbol currentSymbol;
                bool isConstructor, returnsValue;
                int arity;
                // Obtenemos el símbolo del método, o puede ser "clase" si no existe en el código la entrada formal
                // Luego obtenemos el nuestro símbolo
                var isArrowExpression = false;
                if (currentSyntaxNode is ArrowExpressionClauseSyntax)
                {
                    isArrowExpression = true;
                    currentSyntaxNode = (CSharpSyntaxNode)currentSyntaxNode.Parent;
                }
                var declaredSymbol = _semanticModelsContainer.GetBySyntaxNode(currentSyntaxNode).GetDeclaredSymbol(currentSyntaxNode);
                if (declaredSymbol is IPropertySymbol && isArrowExpression)
                    declaredSymbol = ((IPropertySymbol)declaredSymbol).GetMethod;
                if (declaredSymbol is IMethodSymbol)
                {
                    isConstructor = ((IMethodSymbol)declaredSymbol).MethodKind == MethodKind.Constructor;
                    currentSymbol = ISlicerSymbol.Create(isConstructor ?
                        ((IMethodSymbol)declaredSymbol).ContainingType : ((IMethodSymbol)declaredSymbol).ReturnType);
                    arity = ((IMethodSymbol)declaredSymbol).Parameters.Count();
                    returnsValue = isConstructor || !((IMethodSymbol)declaredSymbol).ReturnsVoid;
                }
                else
                {
                    isConstructor = returnsValue = true;
                    arity = 0;
                    currentSymbol = ISlicerSymbol.Create(((INamedTypeSymbol)declaredSymbol).ConstructedFrom);
                }
                
                _broker.LogCallback(declaredSymbol is IMethodSymbol ? declaredSymbol : currentSymbol.Symbol, caller, callerMethodName);

                // Objeto que se retorna del callback
                var returnHub = _termFactory.Create(node, currentSymbol, false, TermFactory.GetFreshName(), true, true);
                // Si no hay argumentos y es constructor no hace falta crear ninguna fucking región
                bool needToCreateRegion = true;

                // Si es un constructor, lo tratamos como a nuestros object creation, es decir: lo alocamos, y definimos utilizando lo que podamos. Se entiende.
                if (isConstructor)
                {
                    _broker.Alloc(returnHub);
                    // Si la región está creada 
                    _broker.DefUseOperation(returnHub, regionCreated ? new Term[] { regionHub } : involvedTerms.ToArray());
                    needToCreateRegion = arity > 0;
                }

                if (!regionCreated && needToCreateRegion)
                {
                    // Supongamos que son varios constructores sin argumentos, nunca se creó la región. Ahora que se crea tenemos que incluir todos los objetos devueltos previamente.
                    _broker.CreateNonInstrumentedRegion(involvedTerms.Union(returnTerms).ToList(), regionHub);
                    regionCreated = true;
                }

                // Si la región no fue creada, no importa porque es que no hay argumentos y es un constructor
                var parameters = Enumerable.Repeat(regionHub, arity).ToList();
                // Ahora bien, si es constructor el @this es lo que creamos antes, sino el @this es la región y devolvemos el returnHub
                var localExceptionTerm = ExceptionTerm;
                ExceptionTerm = null;
                if (isConstructor)
                    HandleInstrumentedMethod(Utils.StmtFromSyntaxNode(node, _instrumentationResult), null, parameters, returnHub);
                else
                    HandleInstrumentedMethod(Utils.StmtFromSyntaxNode(node, _instrumentationResult), returnHub, parameters, regionHub);

                // Hubo una exceptión, no tenemos seteado nada
                if (ExceptionTerm != null)
                {
                    returnTerms.Add(ExceptionTerm);
                    if (regionCreated)
                        _broker.CatchReturnedValueIntoRegion(regionHub, ExceptionTerm);

                    _broker.SliceCriteriaReached = _traceConsumer.SliceCriteriaReached();
                }
                else if (returnsValue)
                {
                    returnTerms.Add(returnHub);
                    // Se devolvió algo, entonces si hay región tenemos que agregarlo
                    if (regionCreated)
                        _broker.CatchReturnedValueIntoRegion(regionHub, returnHub);
                }
                ExceptionTerm = localExceptionTerm;

                nextStmt = _traceConsumer.HasNext() ? ObserveNextStatement(allowNulls: true) : null;
            }

            if (enterStaticLoop)
                _broker.ExitStaticMode();

            if (nextStmt != null)
            {
                if (finalTraceType.HasValue && nextStmt.TraceType == finalTraceType.Value && consume)
                    GetNextStatement(finalTraceType.Value, false); // TODO: El false del final está para que no pinche IOP...
                // TODO: Soluciona TEMPORALMENTE el problema de Lazy Initialization y similares (test LazyInitialization)
                // POR FAVOR VER BIEN
                else if (consume && nextStmt.TraceType == TraceType.EndInvocation && nextStmt.CSharpSyntaxNode.Parent is ParenthesizedLambdaExpressionSyntax)
                {
                    GetNextStatement(TraceType.EndInvocation);
                    nextStmt = ObserveNextStatement();
                    if (finalTraceType.HasValue && nextStmt.TraceType == finalTraceType.Value)
                        GetNextStatement(finalTraceType.Value);
                }
            }
            else
                ;

            return returnTerms;
        }

        void EnterCondition(Stmt statement)
        {
            _broker.EnterCondition(statement);
            if (DynAbs.ControlManagement.IsLoopStatement(statement))
                if (!_enterLoopSet.ContainsKey(statement))
                {
                    _enterLoopSet.Add(statement, _broker.EnterStaticMode());
                    _broker.EnterLoop();
                }
                else
                    _broker.NextLoopIteration();

            //var nextStmt = ObserveNextStatement();
            //if (statement.CSharpSyntaxNode is SwitchStatementSyntax && 
            //    (nextStmt.CSharpSyntaxNode.Parent is SwitchSectionSyntax ||
            //    (nextStmt.CSharpSyntaxNode.Parent is BlockSyntax bs &&
            //    bs.Parent is SwitchSectionSyntax)))
            //{
            //    var switchSyntax = nextStmt.CSharpSyntaxNode.Parent is SwitchSectionSyntax ? 
            //        (SwitchSectionSyntax)nextStmt.CSharpSyntaxNode.Parent : (SwitchSectionSyntax)nextStmt.CSharpSyntaxNode.Parent.Parent;
            //    var switchOperation = (ISwitchCaseOperation)GetOperation(switchSyntax);
            //    if (switchOperation.Clauses.Length == 1 && switchOperation.Clauses.Single() is ICaseClauseOperation
            //        && ((ICaseClauseOperation)switchOperation.Clauses.Single()).CaseKind == CaseKind.Pattern)
            //    {
            //        var pattern = ((IPatternCaseClauseOperation)switchOperation.Clauses.Single()).Pattern;
            //        DealWithPatterns(temporarySwitchTerm, pattern, new List<Term>());
            //    }
            //}
        }

        void ExitCondition(Stmt statement)
        {
            _broker.ExitCondition(statement);
            if (statement.CSharpSyntaxNode is ForEachStatementSyntax)
                CheckForMoveNextCallbacks((IForEachLoopOperation)GetOperation(statement.CSharpSyntaxNode));
        }

        void ExitLoop(Stmt statement)
        {
            if (DynAbs.ControlManagement.IsLoopStatement(statement))
            {
                var tobj = _enterLoopSet.Where(x => x.Key.FileId == statement.FileId &&
                                x.Key.SpanStart == statement.SpanStart && x.Key.SpanEnd == statement.SpanEnd).FirstOrDefault();
                // Se puede aplicar porque la key es string, nullable
                if (tobj.Key != null)
                {
                    if (tobj.Value)
                        _broker.ExitStaticMode();
                    _enterLoopSet.Remove(tobj.Key);
                    _broker.ExitLoop();
                }
            }
        }

        void AssignRV(Term term)
        {
            _broker.AssignRV(term);
            _rv_assigned = true;
        }

        void ExitMethod(Stmt currentStatement)
        {
            if (!_rv_assigned && _methodNode is MethodDeclarationSyntax && yieldReturnValuesContainer == null)
            {
                var typeSymbol = ((IMethodSymbol)_semanticModelsContainer.GetBySyntaxNode((CSharpSyntaxNode)_methodNode).GetDeclaredSymbol((CSharpSyntaxNode)_methodNode)).ReturnType;
                var enumerableNames = new string[] { "IEnumerable", "IEnumerator" };
                var isEnumerable = enumerableNames.Contains(typeSymbol.Name) ||
                    typeSymbol.AllInterfaces.Any(x => enumerableNames.Any(y => x.Name.Contains(y)));
                if (isEnumerable)
                    InitializeYieldReturnContainer((CSharpSyntaxNode)_methodNode);
            }

            if (yieldReturnValuesContainer != null)
                AssignRV(yieldReturnValuesContainer);

            _broker.ExitMethod(currentStatement, _returnObject, _returnExceptionTerm, null);
            _setReturn = true;
        }

        void HandleExitUsing()
        {
            var nextStmt = ObserveNextStatement(false);
            var currentObject = _usingStack.Pop();
            if ((nextStmt.TraceType == TraceType.EnterMethod || nextStmt.TraceType == TraceType.EnterStaticMethod) &&
                ((nextStmt.CSharpSyntaxNode) is MethodDeclarationSyntax) &&
                ClosingMethods.Any(x => x.Equals(((MethodDeclarationSyntax)nextStmt.CSharpSyntaxNode).Identifier.ValueText)))
                HandleInstrumentedMethod(currentObject.Stmt, null, null, currentObject);
        }

        void HandleBodyUnexpectedTrace(Stmt statement)
        {
            // XXX: Se llama cuando hay un error de tipo de traza no esperada en el body
            // El único caso que admitimos son los callbacks del destructor que llaman al Dispose. 
            // Si es así, consumimos la traza sin darle mayor importancia
            // Según comprobaos puede caer un enter dentro de otro, por eso permitimos llamadas recursivas
            if (statement.TraceType == TraceType.EnterMethod &&
                (statement.CSharpSyntaxNode) is MethodDeclarationSyntax &&
                ClosingMethods.Any(x => x.Equals(((MethodDeclarationSyntax)statement.CSharpSyntaxNode).Identifier.ValueText)))
            {
                Stmt currentStatement = null;
                do
                {
                    currentStatement = GetNextStatement();
                    if (currentStatement.TraceType == TraceType.EnterMethod &&
                        (currentStatement.CSharpSyntaxNode) is MethodDeclarationSyntax &&
                        ClosingMethods.Any(x => x.Equals(((MethodDeclarationSyntax)currentStatement.CSharpSyntaxNode).Identifier.ValueText)))
                        HandleBodyUnexpectedTrace(currentStatement);

                } while (!(currentStatement.TraceType == TraceType.EnterFinalFinally &&
                (currentStatement.CSharpSyntaxNode) is MethodDeclarationSyntax &&
                ClosingMethods.Any(x => x.Equals(((MethodDeclarationSyntax)currentStatement.CSharpSyntaxNode).Identifier.ValueText))));
            }
            else
            {
                // TODOHACK
                if (statement.TraceType == TraceType.EndMemberAccess || statement.TraceType == TraceType.EndInvocation)
                    return;

                var lastStmt = _traceConsumer.Stack.ToList()[1];
                var localSpanStart = 0;
                var localSpanEnd = 0;
                var localFileId = lastStmt.FileId;
                if (lastStmt.CSharpSyntaxNode is ReturnStatementSyntax lastStmtRet)
                {
                    localSpanStart = lastStmtRet.Expression.Span.Start;
                    localSpanEnd = lastStmtRet.Expression.Span.End;
                } 
                else if (lastStmt.CSharpSyntaxNode is LocalDeclarationStatementSyntax lastStmtLoc)
                {
                    var lastLocN = lastStmtLoc.Declaration.Variables.FirstOrDefault().Initializer.Value;
                    localSpanStart = lastLocN.Span.Start;
                    localSpanEnd = lastLocN.Span.End;
                }
                else if (lastStmt.CSharpSyntaxNode is PrefixUnaryExpressionSyntax lastStmtPref)
                {
                    localSpanStart = lastStmtPref.Operand.Span.Start;
                    localSpanEnd = lastStmtPref.Operand.Span.End;
                } 
                else if (lastStmt.CSharpSyntaxNode is ExpressionStatementSyntax lastStmtExp)
                {
                    localSpanStart = lastStmtExp.Span.Start;
                    localSpanEnd = lastStmtExp.Span.End;
                }
                else if (lastStmt.CSharpSyntaxNode is InvocationExpressionSyntax lastStmtInv)
                {
                    localSpanStart = lastStmtInv.Span.Start;
                    localSpanEnd = lastStmtInv.Span.End;
                }

                if (localSpanStart > 0)
                { 
                    var newTuple = new Tuple<int, int, int>(localFileId, localSpanStart-1, localSpanEnd+1);
                    structsExceptions.Add(newTuple);
                    Console.WriteLine($"new Tuple<int, int, int>({localFileId}, {localSpanStart-1}, {localSpanEnd+1}),");
                }

                if (true)
                    throw new UnexpectedTrace(_traceConsumer);
            }
        }
        
        void HandleCatch(Stmt stmt)
        {
            var catchExpression = (CatchClauseSyntax)stmt.CSharpSyntaxNode;
            if (catchExpression.Declaration != null && catchExpression.Declaration.Identifier != null && catchExpression.Declaration.Identifier.Value != null)
            {
                var catchOperation = (ICatchClauseOperation)GetOperation(catchExpression);

                var term = _termFactory.Create(catchOperation.ExceptionDeclarationOrExpression,
                    ISlicerSymbol.Create(((IVariableDeclaratorOperation)catchOperation.ExceptionDeclarationOrExpression).Symbol.Type), false, 
                    ((ILocalSymbol)((IVariableDeclaratorOperation)catchOperation.ExceptionDeclarationOrExpression).Symbol).Name,
                    false);

                if (ExceptionTerm != null)
                    _broker.Assign(term, ExceptionTerm);
                else
                    HandleNonInstrumentedMethod((IOperation)null, new List<Term>(), null, term, term.Last.Symbol.Symbol, "ctor");

                // XXX
                ExceptionTerm = term;
            }
            throw new CatchedProgramException();
        }

        void HandleFinalCatch(Stmt currentStatement)
        {
            // XXX: Se sale del método actual inmediatamente
            _broker.ExitMethod(currentStatement, null, _returnExceptionTerm, ExceptionTerm);
            throw new ProgramException();
        }

        void HandleFinnaly(Stmt currentStatement)
        {
            bool hayOtroFinally = _traceConsumer.HasNext() && ObserveNextStatement().TraceType == TraceType.EnterFinally;
            if (_returnPostponed && !hayOtroFinally)
            {
                _sliceCriteriaReached = _broker.SliceCriteriaReached;
                _broker.ExitMethod(currentStatement, _returnObject, _returnExceptionTerm, ExceptionTerm);
                _setReturn = true;
            }
        }

        void InitializeYieldReturnContainer(CSharpSyntaxNode node)
        {
            var returnType = _methodNode 
                switch {
                    MethodDeclarationSyntax methodDeclarationSyntax => methodDeclarationSyntax.ReturnType,
                    LocalFunctionStatementSyntax localFunctionStatementSyntax => localFunctionStatementSyntax.ReturnType,
                    _ => ((PropertyDeclarationSyntax)_methodNode.Parent.Parent).Type,
                };

            yieldReturnValuesContainer = _termFactory.Create(node,
                    ISlicerSymbol.Create(((ITypeSymbol)_semanticModelsContainer.GetBySyntaxNode((CSharpSyntaxNode)_methodNode)
                    .GetSymbolInfo(returnType).Symbol)), false, TermFactory.GetFreshName(), false);

            _broker.HandleNonInstrumentedMethod(new List<Term>(), null, new List<Term>(), yieldReturnValuesContainer,
                ((ITypeSymbol)_semanticModelsContainer.GetBySyntaxNode((CSharpSyntaxNode)_methodNode)
                .GetSymbolInfo(returnType).Symbol), "ctor");
        }
        #endregion
    }
}
