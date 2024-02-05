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
        Term Visit(IOperation operation)
        {
            if (operation == null)
                return null;

            Term returnValue = null;
            switch (operation.Kind)
            {
                case OperationKind.IsType:
                    returnValue = Visit(((IIsTypeOperation)operation).ValueOperand);
                    break;
                case OperationKind.ExpressionStatement:
                    returnValue = Visit(((IExpressionStatementOperation)operation).Operation);
                    break;
                case OperationKind.Conversion:
                    returnValue = VisitConversionExpression((IConversionOperation)operation);
                    break;
                case OperationKind.Empty:
                    VisitEmptyStatement((IEmptyOperation)operation);
                    break;
                case OperationKind.SimpleAssignment:
                    returnValue = VisitAssignmentExpression((IAssignmentOperation)operation);
                    break;
                case OperationKind.DeconstructionAssignment:
                    returnValue = VisitDeconstructionAssignmentExpression((IDeconstructionAssignmentOperation)operation);
                    break;
                case OperationKind.EventAssignment:
                    VisitEventAssignmentExpression((IEventAssignmentOperation)operation);
                    break;
                case OperationKind.DelegateCreation:
                    returnValue = VisitDelegateCreationOperation((IDelegateCreationOperation)operation);
                    break;
                case OperationKind.MethodReference:
                    returnValue = VisitMethodReferenceOperation((IMethodReferenceOperation)operation);
                    break;
                case OperationKind.CompoundAssignment:
                    returnValue = VisitCompoundAssignmentExpression((ICompoundAssignmentOperation)operation);
                    break;
                case OperationKind.Increment:
                    returnValue = VisitIncrementExpression((IIncrementOrDecrementOperation)operation);
                    break;
                case OperationKind.Decrement:
                    returnValue = VisitIncrementExpression((IIncrementOrDecrementOperation)operation);
                    break;
                case OperationKind.UnaryOperator:
                    returnValue = VisitUnaryOperatorExpression((IUnaryOperation)operation);
                    break;
                case OperationKind.BinaryOperator:
                    returnValue = VisitBinaryOperatorExpression((IBinaryOperation)operation);
                    break;
                case OperationKind.Using:
                    VisitUsingStatement((IUsingOperation)operation);
                    break;
                case OperationKind.UsingDeclaration:
                    VisitUsingDeclarationStatement((IUsingDeclarationOperation)operation);
                    break;
                case OperationKind.Conditional:
                    if (((IConditionalOperation)operation).Syntax is IfStatementSyntax)
                        VisitIfStatement((IConditionalOperation)operation);
                    else
                        returnValue = VisitConditionalChoiceExpression((IConditionalOperation)operation);
                    break;
                case OperationKind.Loop:
                    switch (((ILoopOperation)operation).LoopKind)
                    {
                        case LoopKind.ForEach:
                            VisitForEachLoopStatement((IForEachLoopOperation)operation);
                            break;
                        case LoopKind.While:
                            VisitWhileUntilLoopStatement((IWhileLoopOperation)operation);
                            break;
                        case LoopKind.For:
                            VisitForOperation((IForLoopOperation)operation);
                            break;
                    }
                    break;
                case OperationKind.Switch:
                    VisitSwitchStatement((ISwitchOperation)operation);
                    break;
                case OperationKind.SwitchExpression:
                    returnValue = VisitSwitchExpression((ISwitchExpressionOperation)operation);
                    break;
                case OperationKind.SwitchCase:
                    VisitSwitchCase((ISwitchCaseOperation)operation);
                    break;
                case OperationKind.CaseClause:
                    VisitCaseClause((ICaseClauseOperation)operation);
                    break;
                case OperationKind.VariableDeclarationGroup:
                    VisitVariableDeclarationStatement((IVariableDeclarationGroupOperation)operation);
                    break;
                case OperationKind.VariableDeclaration:
                    VisitVariableDeclaration((IVariableDeclarationOperation)operation);
                    break;
                case OperationKind.VariableDeclarator:
                    VisitVariableDeclarator((IVariableDeclaratorOperation)operation);
                    break;
                case OperationKind.DeclarationExpression:
                    returnValue = VisitDeclarationExpression((IDeclarationExpressionOperation)operation);
                    break;
                case OperationKind.VariableInitializer:
                    return Visit(((IVariableInitializerOperation)operation).Value);
                    break;
                case OperationKind.TypeOf:
                    returnValue = VisitTypeOperationExpression((ITypeOfOperation)operation);
                    break;
                case OperationKind.IsPattern:
                    returnValue = VisitIsPatternOperationExpression((IIsPatternOperation)operation);
                    break;
                case OperationKind.DefaultValue:
                    returnValue = VisitDefaultValueExpression((IDefaultValueOperation)operation);
                    break;
                case OperationKind.ArrayCreation:
                    returnValue = VisitArrayCreationExpression((IArrayCreationOperation)operation);
                    break;
                case OperationKind.ArrayInitializer:
                    returnValue = VisitArrayInitializer((IArrayInitializerOperation)operation);
                    break;
                case OperationKind.Coalesce:
                    returnValue = VisitNullCoalescingExpression((ICoalesceOperation)operation);
                    break;
                case OperationKind.CoalesceAssignment:
                    returnValue = VisitCoalesceAssignmentOperation((ICoalesceAssignmentOperation)operation);
                    break;
                case OperationKind.Return:
                    VisitReturnStatement((IReturnOperation)operation);
                    break;
                case OperationKind.YieldReturn:
                    VisitYieldReturnStatement((IReturnOperation)operation);
                    break;
                case OperationKind.YieldBreak:
                    VisitYieldBreakStatement((IReturnOperation)operation);
                    break;
                case OperationKind.Invocation:
                    returnValue = VisitInvocationExpression((IInvocationOperation)operation);
                    break;
                case OperationKind.ObjectCreation:
                    returnValue = VisitObjectCreationExpression((IObjectCreationOperation)operation);
                    break;
                case OperationKind.AnonymousObjectCreation:
                    returnValue = VisitAnonymousObjectCreationExpression((IAnonymousObjectCreationOperation)operation);
                    break;
                case OperationKind.TypeParameterObjectCreation:
                    returnValue = VisitTypeParameterObjectCreationExpression((ITypeParameterObjectCreationOperation)operation);
                    break;
                case OperationKind.Argument:
                    returnValue = VisitArgument((IArgumentOperation)operation);
                    break;
                case OperationKind.LocalReference:
                    returnValue = VisitLocalReferenceExpression((ILocalReferenceOperation)operation);
                    break;
                case OperationKind.EventReference:
                    returnValue = VisitEventReferenceExpression((IEventReferenceOperation)operation);
                    break;
                case OperationKind.FieldReference:
                    returnValue = VisitFieldReferenceExpression((IFieldReferenceOperation)operation);
                    break;
                case OperationKind.PropertyReference:
                    returnValue = (((IPropertyReferenceOperation)operation).IsIndexer()) ?
                        VisitIndexedPropertyReferenceExpression((IPropertyReferenceOperation)operation) :
                        VisitPropertyReferenceExpression((IPropertyReferenceOperation)operation);
                    break;
                case OperationKind.ConditionalAccess:
                    returnValue = VisitConditionalAccessOperation((IConditionalAccessOperation)operation);
                    break;
                case OperationKind.ConditionalAccessInstance:
                    returnValue = VisitConditionalAccessInstance((IConditionalAccessInstanceOperation)operation);
                    break;
                case OperationKind.ParameterReference:
                    returnValue = VisitParameterReferenceExpression((IParameterReferenceOperation)operation);
                    break;
                case OperationKind.ArrayElementReference:
                    returnValue = VisitArrayElementReferenceExpression((IArrayElementReferenceOperation)operation);
                    break;
                case OperationKind.InstanceReference:
                    returnValue = VisitInstanceReferenceExpression((IInstanceReferenceOperation)operation);
                    break;
                case OperationKind.AnonymousFunction:
                    returnValue = VisitLambdaExpression((IAnonymousFunctionOperation)operation);
                    break;
                case OperationKind.InterpolatedStringText:
                    returnValue = Visit(((IInterpolatedStringTextOperation)operation).Text);
                    break;
                case OperationKind.Interpolation:
                    returnValue = Visit(((IInterpolationOperation)operation).Expression);
                    break;
                case OperationKind.InterpolatedString:
                    returnValue = VisitInterpolatedStringOperation((IInterpolatedStringOperation)operation);
                    break;
                case OperationKind.Literal:
                    returnValue = VisitLiteralExpression((ILiteralOperation)operation);
                    break;
                case OperationKind.SizeOf:
                    returnValue = VisitSizeOfOperation((ISizeOfOperation)operation);
                    break;
                case OperationKind.Branch:
                    VisitBranchStatement((IBranchOperation)operation);
                    break;
                case OperationKind.Lock:
                    VisitLockStatement((ILockOperation)operation);
                    break;
                case OperationKind.NameOf:
                    returnValue = VisitNameOf((INameOfOperation)operation);
                    break;
                case OperationKind.Throw:
                    VisitThrowStatement((IThrowOperation)operation);
                    break;
                case OperationKind.TranslatedQuery:
                    returnValue = VisitQueryExpression((ITranslatedQueryOperation)operation);
                    break;
                case OperationKind.DynamicMemberReference:
                    returnValue = VisitDynamicMemberReference((IDynamicMemberReferenceOperation)operation);
                    break;
                case OperationKind.DynamicIndexerAccess:
                    returnValue = VisitDynamicAccess((IDynamicIndexerAccessOperation)operation);
                    break;
                case OperationKind.Tuple:
                    returnValue = VisitTuple((ITupleOperation)operation);
                    break;
                case OperationKind.Discard:
                    returnValue = VisitDiscard((IDiscardOperation)operation);
                    break;
                case OperationKind.Await:
                    returnValue = Visit(((IAwaitOperation)operation).Operation);
                    break;
                case OperationKind.Invalid:
                case OperationKind.None:
                    returnValue = VisitInvalidOperation(operation);
                    break;
                default:
                    throw new SlicerException("Operación no desarrollada");
            }

            if (!_throwingException)
                CheckExceptions();

            return returnValue;
        }

        #region Special cases
        void VisitEmptyStatement(IEmptyOperation operation)
        {
            _broker.UseOperation(Utils.StmtFromSyntaxNode((CSharpSyntaxNode)operation.Syntax, _instrumentationResult), new List<Term>());
        }

        void VisitLockStatement(ILockOperation operation)
        {
            var internalTerm = Visit(operation.LockedValue);
            _broker.UseOperation(Utils.StmtFromSyntaxNode((CSharpSyntaxNode)operation.Syntax, _instrumentationResult), new List<Term>() { internalTerm });
        }

        Term VisitNameOf(INameOfOperation operation)
        {
            // XXX: No voy a hacer que utilice la variable porque no tiene sentido, no la utiliza! así que es como un literal. 
            var slicerSymbol = ISlicerSymbol.Create(operation.Type);
            var newTerm = _termFactory.Create(operation, slicerSymbol, false, TermFactory.GetFreshName());
            _broker.DefUseOperation(newTerm);
            return newTerm;
        }

        void VisitEventAssignmentExpression(IEventAssignmentOperation operation)
        {
            // XXX: Asumimos que no hay callbacks
            var def = Visit(operation.EventReference);
            var use = Visit(operation.HandlerValue);
            // XXX: En realidad es new EventHandler(<MethodBinding>) pero MethodBinding no es lo que tiene el End, de hecho se puede invocar desde otros lados
            // Como es un new()... tiene un EndInvocation que no se consume
            // TODOX
            //if (operation.HandlerValue.Kind == OperationKind.MethodBindingExpression)
            //    OptionalGetNextStatement(TraceType.EndInvocation, operation.Syntax.Span.End);
            if (use != null)
                _broker.Assign(def, use, null);
        }

        Term VisitDelegateCreationOperation(IDelegateCreationOperation operation)
        {
            var internalMethod = Visit(operation.Target);
            OptionalGetNextStatement(TraceType.EndInvocation, operation.Syntax.Span.End);

            // No sé si vale la pena devolver algo. TODOX IMPORTANTE REVISAR QUE QUEREMOS DEVOLVER
            return internalMethod;
        }

        Term VisitMethodReferenceOperation(IMethodReferenceOperation operation)
        {
            Term newTerm;
            IDictionary<ITypeSymbol, ITypeSymbol> typesDictionary = null;
            var recTerm = Visit(operation.Instance);
            if (recTerm == null)
            {
                newTerm = _termFactory.Create(operation, ISlicerSymbol.Create(operation.Type, _typeArguments), operation.Method.IsStatic,
                    Utils.GetRealName(operation.Method.ToString(), _typeArguments));
                typesDictionary = Utils.GetTypesDictionary(operation.Method.ContainingType, _typeArguments);
            }
            else if (recTerm.IsScalar)
                newTerm = recTerm;
            else
            {
                // TODO: Habría que tener un tipo especial para "métodos" pero no tiene sentido. 
                newTerm = recTerm.AddingField(new Field(operation.Method.Name, ISlicerSymbol.CreateObjectSymbol()));
                newTerm.Stmt = Utils.StmtFromSyntaxNode((CSharpSyntaxNode)operation.Syntax, _instrumentationResult);
            }
            return newTerm;
        }

        void VisitUsingStatement(IUsingOperation operation)
        {
            // Inicialización de variables
            var definedTerm = Visit(operation.Resources);

            List<Term> defs;
            if (operation.Resources is IVariableDeclarationGroupOperation)
                defs = ((IVariableDeclarationGroupOperation)operation.Resources).Declarations.SelectMany(x => x.Declarators.Select(y =>
                _termFactory.Create(x, ISlicerSymbol.Create(((IVariableDeclaratorOperation)y).Symbol.Type), false, ((IVariableDeclaratorOperation)y).Symbol.Name))).ToList();
            else if (operation.Resources is ILocalReferenceOperation)
                defs = new List<Term>() { _termFactory.Create(((ILocalReferenceOperation)operation.Resources),
                    ISlicerSymbol.Create(((ILocalReferenceOperation)operation.Resources).Local.Type), false, ((ILocalReferenceOperation)operation.Resources).Local.Name)};
            else if (operation.Resources is IInvocationOperation)
                defs = new List<Term>() { definedTerm };
            else if (operation.Resources is IObjectCreationOperation)
                defs = new List<Term>() { Visit(operation.Resources) };
            else
                throw new NotImplementedException();

            defs.Reverse();

            foreach (var def in defs)
                _usingStack.Push(def);
        }

        void VisitUsingDeclarationStatement(IUsingDeclarationOperation operation)
        {
            Visit(operation.DeclarationGroup);
        }

        Term VisitTypeOperationExpression(ITypeOfOperation operation)
        {
            var returnHub = _termFactory.Create(operation, ISlicerSymbol.Create(operation.Type), false, TermFactory.GetFreshName());
            _broker.CustomEvent(new List<Term>(), null, new List<Term>(), returnHub, "TypeOf");
            return returnHub;
        }

        Term VisitIsPatternOperationExpression(IIsPatternOperation operation)
        {
            var valueTerm = Visit(operation.Value);
            var slicerSymbol = ISlicerSymbol.Create(operation.Type);
            var newTerm = _termFactory.Create(operation, slicerSymbol, false, TermFactory.GetFreshName());
            var usedTerms = new List<Term>() { valueTerm };
            _broker.DefUseOperation(newTerm, usedTerms.ToArray());

            if (operation.Pattern != null)
            {
                var pattern = operation.Pattern;
                if (pattern is INegatedPatternOperation negatedPatternOperation)
                    pattern = negatedPatternOperation.Pattern;

                if (pattern is IDeclarationPatternOperation declarationPatternOperation)
                {
                    var declaredVariable = _termFactory.Create(pattern, ISlicerSymbol.Create(pattern.NarrowedType), false, declarationPatternOperation.DeclaredSymbol.Name, false, false);
                    _broker.Assign(declaredVariable, valueTerm, new List<Term>() { newTerm });
                }
                else if (pattern is IRecursivePatternOperation recPatternOp)
                {
                    PatternOperationReceiver = valueTerm;
                    foreach (var propSupPat in recPatternOp.PropertySubpatterns)
                    {
                        var member = Visit(propSupPat.Member);
                        DealWithPatterns(member, propSupPat.Pattern, usedTerms);
                    }
                    PatternOperationReceiver = null;

                    if (recPatternOp.DeclaredSymbol != null)
                    {
                        // TODO: Repeated code
                        var declaredVariable = _termFactory.Create(recPatternOp, ISlicerSymbol.Create(recPatternOp.NarrowedType), false, recPatternOp.DeclaredSymbol.Name, false, false);
                        _broker.Assign(declaredVariable, valueTerm, new List<Term>() { newTerm });
                    }
                }
            }

            while (usedTerms.Count > 1)
            {
                try
                {
                    _broker.DefUseOperation(newTerm, usedTerms.ToArray());
                    break;
                }
                catch (NonGlobalUninitializedTerm ex)
                {
                    usedTerms.Remove(ex.Term);
                }
            }
            return newTerm;
        }

        Term VisitDefaultValueExpression(IDefaultValueOperation operation)
        {
            var returnTerm = _termFactory.Create(operation, ISlicerSymbol.Create(operation.Type), false, TermFactory.GetFreshName(), true);
            //_broker.Alloc(returnTerm);
            //_broker.DefUseOperation(returnTerm);
            _broker.HandleNonInstrumentedMethod(new List<Term>(), null, new List<Term>(), returnTerm, operation.Type, ".ctor");
            return returnTerm;
        }

        void VisitThrowStatement(IThrowOperation operation)
        {
            if (operation.Exception != null)
            {
                _throwingException = true;
                Term expTerm = Visit(operation.Exception);
                ExceptionTerm = _termFactory.Create(operation, expTerm.Last.Symbol, true, TermFactory.GetFreshName());
                _broker.Assign(ExceptionTerm, expTerm);
                _throwingException = false;
            }
            else
            {
                var throwTerm = _termFactory.Create(operation, ISlicerSymbol.CreateObjectSymbol());
                _broker.DefUseOperation(throwTerm, new Term[] { });
                if (ExceptionTerm != null)
                    _broker.DefUseOperation(ExceptionTerm, new Term[] { ExceptionTerm, throwTerm });
            }
        }

        Term VisitLambdaExpression(IAnonymousFunctionOperation operation)
        {
            // Hay casos particulares donde estás haciendo una invocación y esto es un parámetro. 
            // Veamos caso por caso y analicemos en concreto.
            var slicerSymbol = ISlicerSymbol.CreateLambdaSlicerSymbol();
            if (operation.Parent != null && operation.Parent.Parent != null && operation.Parent.Parent is IArgumentOperation)
                slicerSymbol = ISlicerSymbol.Create(((IArgumentOperation)operation.Parent.Parent).Parameter.Type, _typeArguments);
            else if (operation.Parent != null && operation.Parent is IDelegateCreationOperation)
                slicerSymbol = ISlicerSymbol.Create(((IDelegateCreationOperation)operation.Parent).Type, _typeArguments);
            else if (operation.Parent is IInvalidOperation)
                slicerSymbol = ISlicerSymbol.CreateObjectSymbol();
            else
                throw new NotImplementedException("REVISAR");

            var newTerm = _termFactory.Create(operation, slicerSymbol, false, TermFactory.GetFreshName());

            var parameterList = new List<string>();

            if (operation.Syntax is SimpleLambdaExpressionSyntax)
                parameterList.Add(((SimpleLambdaExpressionSyntax)operation.Syntax).Parameter.Identifier.ValueText);
            else if (operation.Syntax is ParenthesizedLambdaExpressionSyntax)
                parameterList.AddRange(((ParenthesizedLambdaExpressionSyntax)operation.Syntax)
                    .ParameterList.Parameters.Select(x => x.Identifier.ValueText));
            else if (operation.Syntax is AnonymousMethodExpressionSyntax &&
                ((AnonymousMethodExpressionSyntax)operation.Syntax).ParameterList != null)
                parameterList.AddRange(((AnonymousMethodExpressionSyntax)operation.Syntax)
                    .ParameterList.Parameters.Select(x => x.Identifier.ValueText));

            IOperation bodyStatement = operation.Body.Operations.First();

            var dependentTerms = new HashSet<Term>();
            // AGREGAMOS TODOS LOS USOS, SERÍA COMO UN HAVOC BENÉVOLO
            var usesVisitor = new UsesVisitor(_semanticModelsContainer, _instrumentationResult, _termFactory);
            usesVisitor.Visit(bodyStatement)
                .Where(x => !parameterList.Contains(x.First.ToString()))
                .ToList()
                .ForEach(x => dependentTerms.Add(x));

            /// XXX Lo que queremos es que funcione como lista, que apunte a todos los usos, pero estos no apunten al lambda
            _broker.CustomEvent(dependentTerms.ToList(), null, new List<Term>(), newTerm, "Lambda");
            return newTerm;
        }

        Term VisitQueryExpression(ITranslatedQueryOperation operation)
        {
            // El término que vamos a devolver
            //TODO: Esto no tiene tipo?
            Term newTerm = _termFactory.Create(operation, ISlicerSymbol.Create(operation.Type), false, TermFactory.GetFreshName());
            // Las variables que se definen dentro de la query (los elementos de las listas)
            // Sirve para filtrar las variables externas de las internas
            var listIdentifiers = new List<string>();
            // Las variables que vamos a ligar a la operation
            var dependentTerms = new HashSet<Term>();
            // Instanciamos el visitor que no consume traza
            var usesVisitor = new UsesVisitor(_semanticModelsContainer, _instrumentationResult, _termFactory);

            // XXX: (QUERYEXPRESSIONSYNTAX): LO TRATO CON EJEMPLOS
            /*
                from unaCasa in lista
                join otraCasa in lista2 
                on unaCasa.cantVentanas equals otraCasa.cantVentanas - variableAuxiliar2
                where otraCasa.cantVentanas < variableAuxiliar3
                select new { unaCasa.cantVentanas, unaCasa.propietario.nombre, variableAuxiliar }
            */

            // CONSTRUCCIÓN SINTÁCTICA:
            /*
	            1. BODY: select new { unaCasa.cantVentanas, unaCasa.propietario.nombre, variableAuxiliar }
                    ==> De acá me importan solo las variables libres externas como variableAuxiliar, al resto se llega por lista
	            2. FROM CLAUSE: El resto
		            A. Expression ==> Lo que SI me importa
		            B. FROM KEYWORD
		            C. Identifier (nombre que le pongo al elemento, puede servir para filtrar el body)
		            D. IN KEYWORD
            */

            // Si fuera un from solo sería: operation.FromClause.Expression... pero puede haber otros from anidados, por eso consultamos así
            foreach (var fromClause in operation.Syntax.DescendantNodes().OfType<FromClauseSyntax>())
            {
                // 1. Arrancamos obteniendo las variables declaradas en el FROM (identifiers)
                listIdentifiers.Add(fromClause.Identifier.Text);
                // 2. Luego obtenemos nuestras variables externas (vamos filtrando porque pueden ser parte de las variables ya leidas)
                usesVisitor.Visit(GetOperation(fromClause.Expression))
                    .Where(x => !listIdentifiers.Contains(x.First.ToString().Split('.')[0]))
                    .ToList()
                    .ForEach(x => dependentTerms.Add(x));
            }

            // 3. De las cláusulas obtenemos los términos que importan
            foreach (var clause in ((QueryExpressionSyntax)operation.Syntax).Body.Clauses)
            {
                if (clause is JoinClauseSyntax)
                {
                    // EJEMPLO DE JOIN:
                    /* join otraCasa in lista2 on unaCasa.cantVentanas equals otraCasa.cantVentanas - variableAuxiliar2 */

                    // 1. JOIN KEYWORD
                    // 2. Identifier: otraCasa
                    // 3. IN KEYWORD
                    // 4. InExpression: lista2
                    // 5. ON KEYWORD
                    // 6. LeftExpression: unaCasa.cantVentanas
                    // 7. EQUALS KEYWORD
                    // 8. RightExpression: otraCasa.cantVentanas - variableAuxiliar2

                    listIdentifiers.Add(((JoinClauseSyntax)clause).Identifier.Text);

                    usesVisitor.Visit(GetOperation(((JoinClauseSyntax)clause).InExpression))
                                .Where(x => !listIdentifiers.Contains(x.First.ToString().Split('.')[0]))
                               .ToList()
                               .ForEach(x => dependentTerms.Add(x));

                    usesVisitor.Visit(GetOperation(((JoinClauseSyntax)clause).LeftExpression))
                               .Where(x => !listIdentifiers.Contains(x.First.ToString().Split('.')[0]))
                               .ToList()
                               .ForEach(x => dependentTerms.Add(x));

                    usesVisitor.Visit(GetOperation(((JoinClauseSyntax)clause).RightExpression))
                               .Where(x => !listIdentifiers.Contains(x.First.ToString().Split('.')[0]))
                               .ToList()
                               .ForEach(x => dependentTerms.Add(x));
                }
                if (clause is WhereClauseSyntax)
                {
                    // EJEMPLO DE WHERE: where otraCasa.cantVentanas < variableAuxiliar3
                    // Lo único que importa es la expresión

                    usesVisitor.Visit(GetOperation(((WhereClauseSyntax)clause).Condition))
                               .Where(x => !listIdentifiers.Contains(x.First.ToString().Split('.')[0]))
                               .ToList()
                               .ForEach(x => dependentTerms.Add(x));
                }
            }

            // 2. Obtenemos del body las variables utilizadas
            // TODO: FALTA EL GROUPCLAUSESYNTAX
            if (((QueryExpressionSyntax)operation.Syntax).Body.SelectOrGroup is SelectClauseSyntax)
                usesVisitor.Visit(GetOperation(((SelectClauseSyntax)((QueryExpressionSyntax)operation.Syntax).Body.SelectOrGroup).Expression))
                    .Where(x => !listIdentifiers.Contains(x.First.ToString().Split('.')[0]))
                    .ToList()
                    .ForEach(x => dependentTerms.Add(x));

            var returnedTerms = WaitForEnd(newTerm.Stmt.CSharpSyntaxNode, dependentTerms.ToList(), null, "QUERY", TraceType.EndInvocation, true);
            _broker.CustomEvent(dependentTerms.ToList(), null, new List<Term>(), newTerm, "Query");
            return newTerm;
        }

        Term VisitConversionExpression(IConversionOperation operation)
        {
            var recTerm = Visit(operation.Operand);
            if (recTerm == null || operation.Operand.Type == null)
                return recTerm;
            // Si se tiene que devolvear algo lo devolvemos a través de algo de otro tipo.

            if (operation.IsImplicit && operation.Type != operation.Operand.Type && !TypesUtils.Compatibles(operation.Type, operation.Operand.Type))
            {
                var realType = operation.Type;
                if (operation.Operand.Type.TypeKind == TypeKind.TypeParameter && _typeArguments == null)
                    ;

                if (operation.Operand.Type.TypeKind == TypeKind.TypeParameter && _typeArguments != null)
                {
                    var tempType = _typeArguments.Where(x => x.Key.Name == operation.Operand.Type.Name).FirstOrDefault().Value;
                    if (tempType != null)
                        realType = tempType;
                }

                // La conversión está overraideada
                if (_traceConsumer.HasNext() &&
                    ObserveNextStatement().TraceType == TraceType.EnterStaticMethod &&
                    ((ObserveNextStatement().CSharpSyntaxNode is ConversionOperatorDeclarationSyntax) ||
                    (ObserveNextStatement().CSharpSyntaxNode.Parent is ConversionOperatorDeclarationSyntax)))
                {
                    var useTerms = new List<Term>() { CreateArgument(recTerm) };
                    var newTerm = _termFactory.Create(operation, ISlicerSymbol.Create(realType), false, TermFactory.GetFreshName());
                    HandleInstrumentedMethod(operation, newTerm, useTerms, null);
                    return newTerm;
                }

                // Cuando se castean object a string por ej.
                if (!realType.IsNotScalar() && operation.Operand.Type.IsNotScalar())
                {
                    // TODOX: chequear porque sigue alocado... 
                    var newTerm = _termFactory.Create(operation, ISlicerSymbol.Create(realType));
                    _broker.DefUseOperation(newTerm, new Term[] { recTerm });
                    return newTerm;
                }
                else if (realType.IsNotScalar() && operation.Operand.Type.IsNotScalar())
                {
                    recTerm.Last.Symbol = ISlicerSymbol.Create(realType);
                    _broker.RedefineType(recTerm);
                }
                else if (realType.IsNotScalar() && !operation.Operand.Type.IsNotScalar())
                {
                    var newTerm = _termFactory.Create(operation, ISlicerSymbol.Create(realType), false, TermFactory.GetFreshName());
                    // XXX: We're not expecting callbacks
                    _broker.HandleNonInstrumentedMethod(new List<Term>() { recTerm }, null, new List<Term>(), newTerm, operation.OperatorMethod);
                    return newTerm;
                }
            }
            else if (operation.Operand.Kind == OperationKind.Tuple && operation.Type is INamedTypeSymbol && ((INamedTypeSymbol)operation.Type).IsTupleType)
            {
                var newTerm = _termFactory.Create(operation, ISlicerSymbol.Create(operation.Type, _typeArguments), false, TermFactory.GetFreshName());
                _broker.Alloc(newTerm);
                _broker.DefUseOperation(newTerm);
                var i = 0;
                foreach (var e in ((INamedTypeSymbol)operation.Type).TupleElements)
                {
                    var tempTerm_old = recTerm.AddingField("x" + i, ISlicerSymbol.Create(((INamedTypeSymbol)operation.Operand.Type).TupleElements[i].Type, _typeArguments));
                    var tempTerm_new = recTerm.AddingField(e.Name, ISlicerSymbol.Create(e.Type, _typeArguments));
                    tempTerm_old.Stmt = tempTerm_new.Stmt = Utils.StmtFromSyntaxNode(((TupleExpressionSyntax)operation.Operand.Syntax).Arguments[i], _instrumentationResult);
                    _broker.Assign(tempTerm_new, tempTerm_old);

                }
            }
            else if (_traceConsumer.HasNext() &&
                    ObserveNextStatement().TraceType == TraceType.EnterStaticMethod &&
                    ((ObserveNextStatement().CSharpSyntaxNode is ConversionOperatorDeclarationSyntax) ||
                    (ObserveNextStatement().CSharpSyntaxNode.Parent is ConversionOperatorDeclarationSyntax)))
            {
                var useTerms = new List<Term>() { CreateArgument(recTerm) };
                var newTerm = _termFactory.Create(operation, ISlicerSymbol.Create(operation.Type), false, TermFactory.GetFreshName());
                HandleInstrumentedMethod(operation, newTerm, useTerms, null);
                return newTerm;
            }
            return recTerm;
        }

        void VisitCaseWhenSyntax(Stmt stmt)
        {
            var caseClauseOp = GetOperation((CSharpSyntaxNode)stmt.CSharpSyntaxNode.Parent) as ICaseClauseOperation;
            var uses = new List<Term>();
            if (caseClauseOp != null && caseClauseOp.CaseKind == CaseKind.Pattern)
                DealWithPatterns(temporarySwitchTerm, ((IPatternCaseClauseOperation)caseClauseOp).Pattern, uses);

            var operation = GetOperation(((WhenClauseSyntax)stmt.CSharpSyntaxNode).Condition);

            // Before entering to the visit I want to keep the temporary switch term outside
            var localTemporarySwitchTerm = temporarySwitchTerm;
            temporarySwitchTerm = null;
            var recTerm = Visit(operation);
            temporarySwitchTerm = localTemporarySwitchTerm;

            var nextStmt = ObserveNextStatement();
            if (nextStmt.TraceType == TraceType.EnterExpression && nextStmt.CSharpSyntaxNode == stmt.CSharpSyntaxNode)
            {
                GetNextStatement();
                // The next trace has to be EnterCondition (section body)
                var switchStmt = GetNextStatement();
                if (switchStmt.TraceType != TraceType.EnterCondition)
                    throw new SlicerException("Unexpected behavior");

                EnterCondition(switchStmt);
                uses.Add(recTerm);
                _broker.UseOperation(stmt, uses);
                ExitCondition(switchStmt);
                EnterCondition(stmt);
            }
        }

        void VisitPropertyOrFieldDeclaration(Stmt stmt)
        {
            try
            {
                _executedStatements.AddPropertyOrFieldInitialization(stmt);
                var syntaxNode = stmt.CSharpSyntaxNode;
                var semanticModel = _semanticModelsContainer.GetBySyntaxNode(syntaxNode);
                Term _definedVariable;

                if (syntaxNode is VariableDeclaratorSyntax)
                {
                    var identifier = ((VariableDeclaratorSyntax)syntaxNode);
                    var type = semanticModel.GetTypeInfo(identifier);
                    var symbol = semanticModel.GetSymbolInfo(identifier);
                    var isGlobal = ((FieldDeclarationSyntax)syntaxNode.Parent.Parent).Modifiers.Any(x => x.ToString() == "static" || x.ToString() == "const");
                    var name = isGlobal ? Utils.GetRealName(semanticModel.GetDeclaredSymbol(syntaxNode).ToString(), _typeArguments) : identifier.Identifier.ValueText;

                    if (((VariableDeclaratorSyntax)syntaxNode).Initializer != null)
                    {
                        var operation = GetOperation(((VariableDeclaratorSyntax)syntaxNode).Initializer.Value);
                        var recTerm = Visit(operation);

                        // Trying to get the correct type
                        var s = semanticModel.GetDeclaredSymbol(syntaxNode);
                        ISlicerSymbol slicerType = null;
                        if (s != null)
                        {
                            ITypeSymbol t = null;
                            if (s is IFieldSymbol)
                                t = ((IFieldSymbol)s).Type;
                            if (s is IPropertySymbol)
                                t = ((IPropertySymbol)s).Type;
                            if (t != null)
                                slicerType = ISlicerSymbol.Create(t, _typeArguments);
                        }
                        if (!isGlobal)
                            _definedVariable = _thisObject.AddingField(name, slicerType ?? recTerm.Last.Symbol);
                        else
                            _definedVariable = _termFactory.Create(syntaxNode, slicerType ?? recTerm.Last.Symbol, isGlobal, name);
                        _definedVariable.Stmt = Utils.StmtFromSyntaxNode(syntaxNode, _instrumentationResult);

                        // Possible implícit conversion
                        if (ObserveNextStatement().TraceType == TraceType.EnterStaticMethod &&
                            _semanticModelsContainer.GetBySyntaxNode(ObserveNextStatement().CSharpSyntaxNode).GetDeclaredSymbol(ObserveNextStatement().CSharpSyntaxNode) is IMethodSymbol &&
                            ((IMethodSymbol)_semanticModelsContainer.GetBySyntaxNode(ObserveNextStatement().CSharpSyntaxNode).GetDeclaredSymbol(ObserveNextStatement().CSharpSyntaxNode)).MethodKind == MethodKind.Conversion)
                        {
                            var newTerm = _termFactory.Create(operation, slicerType ?? recTerm.Last.Symbol, false, TermFactory.GetFreshName());
                            HandleInstrumentedMethod(_definedVariable.Stmt, newTerm, new List<Term> { recTerm }, null, _typeArguments);
                            _broker.Assign(_definedVariable, newTerm);
                        }
                        else
                            _broker.Assign(_definedVariable, recTerm);
                    }
                    else
                    {
                        var declaredSymbol = (IFieldSymbol)semanticModel.GetDeclaredSymbol(syntaxNode);
                        var currentSymbol = declaredSymbol != null && declaredSymbol.Type != null ? ISlicerSymbol.Create(declaredSymbol.Type, _typeArguments) : ISlicerSymbol.CreateNullTypeSymbol();
                        if (!isGlobal)
                            _definedVariable = _thisObject.AddingField(name, currentSymbol);
                        else
                            _definedVariable = _termFactory.Create(syntaxNode, currentSymbol, isGlobal, name);
                        _definedVariable.Stmt = Utils.StmtFromSyntaxNode(syntaxNode, _instrumentationResult);

                        _broker.DefUseOperation(_definedVariable);
                        if (currentSymbol.Symbol.CustomIsStruct())
                        {
                            _broker.Alloc(_definedVariable);
                            foreach (var f in currentSymbol.Symbol.GetMembers().Where(x => x is IFieldSymbol))
                            {
                                var fieldMember = _definedVariable.AddingField(f.Name, ISlicerSymbol.Create(((IFieldSymbol)f).Type, _typeArguments));
                                fieldMember.Stmt = _definedVariable.Stmt;
                                _broker.DefUseOperation(fieldMember);
                            }
                        }
                    }
                }
                else
                {
                    var identifier = ((PropertyDeclarationSyntax)syntaxNode);
                    var type = semanticModel.GetSymbolInfo(((PropertyDeclarationSyntax)syntaxNode).Type).Symbol;
                    var slicerType = type == null ? ISlicerSymbol.CreateNullTypeSymbol() : ISlicerSymbol.Create((ITypeSymbol)type);
                    var isGlobal = ((PropertyDeclarationSyntax)syntaxNode).Modifiers.Any(x => x.ToString() == "static" || x.ToString() == "const");
                    var name = isGlobal ? Utils.GetRealName(semanticModel.GetDeclaredSymbol(syntaxNode).ToString(), _typeArguments) : identifier.Identifier.ValueText;
                    if (isGlobal)
                        _definedVariable = _termFactory.Create((PropertyDeclarationSyntax)syntaxNode, slicerType, isGlobal, name);
                    else
                        _definedVariable = _thisObject.AddingField(name, slicerType);
                    _definedVariable.Stmt = Utils.StmtFromSyntaxNode(syntaxNode, _instrumentationResult);

                    if (((PropertyDeclarationSyntax)syntaxNode).Initializer != null)
                    {
                        var recTerm = Visit(GetOperation(((PropertyDeclarationSyntax)syntaxNode).Initializer.Value));

                        var obsNextStmt = ObserveNextStatement();
                        if (obsNextStmt.TraceType == TraceType.EnterStaticMethod && obsNextStmt.CSharpSyntaxNode is ConversionOperatorDeclarationSyntax)
                        {
                            var convTerm = _termFactory.Create((PropertyDeclarationSyntax)syntaxNode, slicerType);
                            HandleInstrumentedMethod(_originalStatement, convTerm, new List<Term>() { recTerm }, null, _typeArguments);
                            if (convTerm != null)
                                recTerm = convTerm;
                        }
                        _broker.Assign(_definedVariable, recTerm);
                    }
                    else
                        _broker.DefUseOperation(_definedVariable);
                }
            }
            catch (Exception ex)
            {
                ;
            }
        }

        Term VisitDynamicAccess(IDynamicIndexerAccessOperation operation)
        {
            var recTerm = Visit(operation.Operation);
            var useTerms = operation.Arguments.Select(x => Visit(x)).ToList();

            Term newTerm;
            IDictionary<ITypeSymbol, ITypeSymbol> typesDictionary = null;

            bool isSetAccessor;
            var hasGetAccesor = HasAccesor("", typesDictionary, out isSetAccessor) && !isSetAccessor; // TODOX: me sumo a cualquier get

            newTerm = recTerm.AddingField(new Field(/*hasGetAccesor ? (TODOX: si tenía nombre se lo ponía antes)*/ TermFactory.GetFreshName(), ISlicerSymbol.Create(operation.Type)));
            newTerm.Stmt = Utils.StmtFromSyntaxNode((CSharpSyntaxNode)operation.Syntax, _instrumentationResult);

            if (hasGetAccesor)
                HandleInstrumentedMethod(operation, newTerm, null, recTerm, typesDictionary);

            if (!((operation.Syntax.Parent is AssignmentExpressionSyntax &&
                ((AssignmentExpressionSyntax)operation.Syntax.Parent).Left == operation &&
                ((AssignmentExpressionSyntax)operation.Syntax.Parent).OperatorToken.Kind() == SyntaxKind.EqualsToken) ||
                //(operation.Syntax.Parent is MemberAccessExpressionSyntax // Structs
                //&& operation.Operation.Type.CustomIsStruct()) || // TODOX (hay que ver si es el tipo)
                (operation.Syntax.Parent is ArgumentSyntax && ((ArgumentSyntax)operation.Syntax.Parent).RefOrOutKeyword.Value != null)))
            {
                var nextStmt = ObserveNextStatement();
                if (newTerm.Count == 1)
                    GetNextStatement(TraceType.EndMemberAccess);
                else
                {
                    var returnTerms = WaitForEnd(newTerm.Stmt.CSharpSyntaxNode, new List<Term>() { newTerm }, null, "INIT", TraceType.EndMemberAccess);
                    _broker.HandleNonInstrumentedMethod(new List<Term>(), newTerm.IsGlobal ? null : newTerm.DiscardLast(), returnTerms, newTerm, operation.Type); // No sabemos siempre el nombre de la operación
                }
            }

            return newTerm;
        }

        Term VisitTuple(ITupleOperation operation)
        {
            List<Term> recTerms = new List<Term>();
            foreach (var o in operation.Elements)
                recTerms.Add(Visit(o));
            // Build the new object... 
            var returnTerm = _termFactory.Create(operation, ISlicerSymbol.Create(operation.Type, _typeArguments));
            _broker.Alloc(returnTerm);
            _broker.DefUseOperation(returnTerm);
            var i = 0;
            foreach (var e in ((INamedTypeSymbol)operation.Type).TupleElements)
            {
                var newTerm = returnTerm.AddingField("x" + i, ISlicerSymbol.Create(e.Type));
                newTerm.Stmt = Utils.StmtFromSyntaxNode(((TupleExpressionSyntax)operation.Syntax).Arguments[i], _instrumentationResult);
                _broker.Assign(newTerm, recTerms[i++]);
            }
            return returnTerm;
        }

        Term VisitDiscard(IDiscardOperation operation)
        {
            var returnTerm = _termFactory.Create(operation, ISlicerSymbol.CreateNullTypeSymbol(), false, TermFactory.GetFreshName());
            _broker.DefUseOperation(returnTerm);
            return returnTerm;
        }

        Term VisitInvalidOperation(IOperation operation)
        {
            var syntaxNode = (CSharpSyntaxNode)operation.Syntax;
            var syntaxKind = operation.Syntax.Kind();
            var model = _semanticModelsContainer.GetBySyntaxNode((CSharpSyntaxNode)operation.Syntax);
            switch (syntaxKind)
            {
                case SyntaxKind.InvocationExpression:
                    #region Invocation
                    var invocationSyntaxNode = (InvocationExpressionSyntax)syntaxNode;

                    //Console.WriteLine(invocationSyntaxNode.ToString() + "&" + _originalStatement.FileId + "&" + invocationSyntaxNode.SpanStart + "&" + invocationSyntaxNode.Span.End);

                    var symbolInfo = Utils.GetMethodSymbolInfo(model, invocationSyntaxNode);
                    if (symbolInfo == null)
                        BugLogging.Log(_originalStatement.FileId, syntaxNode, BugLogging.Behavior.WithoutSymbol);
                    else
                        BugLogging.Log(_originalStatement.FileId, syntaxNode, BugLogging.Behavior.WithoutIOperation);

                    Term thisTerm = null;
                    if ((symbolInfo == null || !symbolInfo.IsStatic) && invocationSyntaxNode.Expression is MemberAccessExpressionSyntax)
                    {
                        var receiverOp = GetOperation(((MemberAccessExpressionSyntax)invocationSyntaxNode.Expression).Expression);
                        thisTerm = Visit(receiverOp);
                    }

                    var isStatic = symbolInfo != null ? symbolInfo.IsStatic : thisTerm == null /* Not always... */;

                    var argumentos = invocationSyntaxNode.ArgumentList.Arguments
                                         .Select(x => CreateArgument(x))
                                         .Where(x => x != null)
                                         .ToList();

                    var dictionary = symbolInfo != null ? Utils.GetTypesDictionary(symbolInfo, _typeArguments) : new Dictionary<ITypeSymbol, ITypeSymbol>();
                    var returnType = symbolInfo != null ? ISlicerSymbol.Create(symbolInfo.ReturnType, _typeArguments) : ISlicerSymbol.CreateObjectSymbol();
                    var newTerm = _termFactory.Create(operation, returnType, false, TermFactory.GetFreshName());

                    var name = symbolInfo != null ? symbolInfo.Name : invocationSyntaxNode.Expression.ToString().Split('.').Last();
                    // El false del final está para que no pinche
                    if (IsInstrumented(name, dictionary, false))
                    {
                        HandleInstrumentedMethod(operation, newTerm, argumentos, isStatic ? null : thisTerm, dictionary);
                        GetNextStatement(TraceType.EndInvocation);
                        if (symbolInfo != null && symbolInfo.ReturnsVoid)
                            _broker.DefUseOperation(newTerm);
                    }
                    else
                        HandleNonInstrumentedMethod(operation, argumentos, thisTerm, newTerm, symbolInfo, symbolInfo != null ? null : ((InvocationExpressionSyntax)operation.Syntax).Expression.ToString());
                    return newTerm;
                #endregion
                case SyntaxKind.SimpleMemberAccessExpression:
                    #region MemberAccess
                    var localSymbolInfo = Utils.GetSymbolInfo(model, syntaxNode);
                    BugLogging.Log(_originalStatement.FileId, syntaxNode, localSymbolInfo == null ? BugLogging.Behavior.WithoutSymbol : BugLogging.Behavior.WithoutIOperation);
                    Term t = null;
                    if (((MemberAccessExpressionSyntax)syntaxNode).Expression is BaseExpressionSyntax ||
                        ((MemberAccessExpressionSyntax)syntaxNode).Expression is ThisExpressionSyntax)
                    {
                        // TODO: se puede poner algo más específico en cada caso
                        var l = _termFactory.Create(operation, ISlicerSymbol.CreateObjectSymbol(), false, "this", false);
                        t = l.AddingField(new Field(((MemberAccessExpressionSyntax)syntaxNode).Name.Identifier.ValueText, ISlicerSymbol.CreateObjectSymbol()));
                        t.Stmt = Utils.StmtFromSyntaxNode((CSharpSyntaxNode)syntaxNode, _instrumentationResult);
                    }
                    else
                    {
                        if (localSymbolInfo != null)
                        {
                            #region WithSymbol
                            if (localSymbolInfo is IPropertySymbol)
                            {
                                Term recTerm = null;
                                IDictionary<ITypeSymbol, ITypeSymbol> typesDictionary = null;
                                bool isSetAccessor;
                                var hasGetAccesor = HasAccesor(localSymbolInfo.Name, typesDictionary, out isSetAccessor) && !isSetAccessor;

                                var staticProp = ((IPropertySymbol)localSymbolInfo).IsStatic;
                                var type = ((IPropertySymbol)localSymbolInfo).Type;
                                if (staticProp)
                                {
                                    t = _termFactory.Create(syntaxNode, ISlicerSymbol.Create(type, _typeArguments), staticProp,
                                        hasGetAccesor ? TermFactory.GetFreshName() : Utils.GetRealName(((IPropertySymbol)localSymbolInfo).Name, _typeArguments));
                                    typesDictionary = Utils.GetTypesDictionary(((IPropertySymbol)localSymbolInfo).ContainingType, _typeArguments);
                                }
                                else
                                {
                                    recTerm = Visit(GetOperation(((MemberAccessExpressionSyntax)syntaxNode).Expression));
                                    if (recTerm.IsScalar)
                                        throw new NotImplementedException();

                                    if (hasGetAccesor)
                                        t = _termFactory.Create(operation, ISlicerSymbol.Create(type, _typeArguments), staticProp, TermFactory.GetFreshName());
                                    else
                                    {
                                        t = recTerm.AddingField(new Field(hasGetAccesor ? TermFactory.GetFreshName() : ((IPropertySymbol)localSymbolInfo).Name, ISlicerSymbol.Create(type)));
                                        t.Stmt = Utils.StmtFromSyntaxNode((CSharpSyntaxNode)operation.Syntax, _instrumentationResult);
                                    }
                                }

                                if (hasGetAccesor)
                                    HandleInstrumentedMethod(operation, t, null, staticProp ? null : recTerm, typesDictionary);

                                var Model = _semanticModelsContainer.GetBySyntaxNode((CSharpSyntaxNode)operation.Syntax);
                                if (/*(!forSet) &&*/ !((operation.Syntax.Parent is AssignmentExpressionSyntax &&
                                    ((AssignmentExpressionSyntax)operation.Syntax.Parent).Left == operation.Syntax &&
                                    ((AssignmentExpressionSyntax)operation.Syntax.Parent).OperatorToken.Kind() == SyntaxKind.EqualsToken) ||
                                    //(operation.Syntax.Parent is MemberAccessExpressionSyntax // Structs
                                    //&& (Model.GetTypeInfo(((MemberAccessExpressionSyntax)operation.Syntax.Parent).Expression).Type).CustomIsStruct()) ||
                                    (operation.Syntax.Parent is ArgumentSyntax && ((ArgumentSyntax)operation.Syntax.Parent).RefOrOutKeyword.Value != null)))
                                {
                                    var nextStmt = ObserveNextStatement();
                                    if (t.Count == 1)
                                        GetNextStatement(TraceType.EndMemberAccess, false); // Siempre false
                                    else
                                    {
                                        var returnTerms = WaitForEnd(t.Stmt.CSharpSyntaxNode, new List<Term>() { t }, ((IPropertySymbol)localSymbolInfo).GetMethod, null, TraceType.EndMemberAccess);
                                        if (returnTerms.Count > 0)
                                            throw new NotImplementedException();

                                        if (!Globals.properties_as_fields)
                                            _broker.HandleNonInstrumentedMethod(new List<Term>(), t.IsGlobal ? null : t.DiscardLast(), returnTerms, t, ((IPropertySymbol)localSymbolInfo).GetMethod);
                                    }
                                }
                            }
                            else if (localSymbolInfo is IFieldSymbol)
                            {
                                //if (!((IFieldSymbol)localSymbolInfo).IsStatic)
                                //    Console.WriteLine("Field no estático: " + syntaxNode.ToString());
                                var type = ((IFieldSymbol)localSymbolInfo).Type;
                                t = _termFactory.Create(syntaxNode, ISlicerSymbol.Create(type, _typeArguments), true, syntaxNode.ToString(), false);
                            }
                            else
                                t = _termFactory.Create(syntaxNode, ISlicerSymbol.CreateObjectSymbol(), true, syntaxNode.ToString(), false);
                            #endregion
                        }
                        else
                        {
                            // Intentemos que sea una property y veamos que podemos hacer
                            //t = _termFactory.Create(syntaxNode, ISlicerSymbol.CreateObjectSymbol(), true, syntaxNode.ToString(), false);

                            Term recTerm = Visit(GetOperation(((MemberAccessExpressionSyntax)syntaxNode).Expression));
                            IDictionary<ITypeSymbol, ITypeSymbol> typesDictionary = null;
                            bool isSetAccessor;
                            var hasGetAccesor = HasAccesor(((MemberAccessExpressionSyntax)syntaxNode).Name.Identifier.ValueText, typesDictionary, out isSetAccessor) && !isSetAccessor;

                            // TODO: Vamos a suponer que es false...
                            var staticProp = recTerm.IsGlobal;
                            //var type = ((IPropertySymbol)localSymbolInfo).Type;
                            if (staticProp)
                            {
                                throw new SlicerException("Funcionalidad no implementada: member access con globals");
                                //t = _termFactory.Create(syntaxNode, ISlicerSymbol.Create(type, _typeArguments), staticProp,
                                //    hasGetAccesor ? TermFactory.GetFreshName() : Utils.GetRealName(((IPropertySymbol)localSymbolInfo).Name, _typeArguments));
                                //typesDictionary = Utils.GetTypesDictionary(((IPropertySymbol)localSymbolInfo).ContainingType, _typeArguments);
                            }
                            else
                            {

                                if (recTerm.IsScalar)
                                    throw new NotImplementedException();

                                if (hasGetAccesor)
                                    t = _termFactory.Create(operation, ISlicerSymbol.CreateObjectSymbol(), staticProp, TermFactory.GetFreshName());
                                else
                                {
                                    t = recTerm.AddingField(new Field(hasGetAccesor ? TermFactory.GetFreshName() : ((MemberAccessExpressionSyntax)syntaxNode).Name.Identifier.ValueText, ISlicerSymbol.CreateObjectSymbol()));
                                    t.Stmt = Utils.StmtFromSyntaxNode((CSharpSyntaxNode)operation.Syntax, _instrumentationResult);
                                }
                            }

                            if (hasGetAccesor)
                                HandleInstrumentedMethod(operation, t, null, staticProp ? null : recTerm, typesDictionary);

                            var Model = _semanticModelsContainer.GetBySyntaxNode((CSharpSyntaxNode)operation.Syntax);
                            if (/*(!forSet) &&*/ !((operation.Syntax.Parent is AssignmentExpressionSyntax &&
                                ((AssignmentExpressionSyntax)operation.Syntax.Parent).Left == operation.Syntax &&
                                ((AssignmentExpressionSyntax)operation.Syntax.Parent).OperatorToken.Kind() == SyntaxKind.EqualsToken) ||
                                //(operation.Syntax.Parent is MemberAccessExpressionSyntax // Structs
                                //&& (Model.GetTypeInfo(((MemberAccessExpressionSyntax)operation.Syntax.Parent).Expression).Type).CustomIsStruct()) ||
                                (operation.Syntax.Parent is ArgumentSyntax && ((ArgumentSyntax)operation.Syntax.Parent).RefOrOutKeyword.Value != null)))
                            {
                                var nextStmt = ObserveNextStatement();
                                if (t.Count == 1)
                                    GetNextStatement(TraceType.EndMemberAccess, false); // Siempre false
                                else
                                {
                                    var returnTerms = WaitForEnd(t.Stmt.CSharpSyntaxNode, new List<Term>() { t }, null, null, TraceType.EndMemberAccess);
                                    if (returnTerms.Count > 0)
                                        throw new NotImplementedException();

                                    // TODO: Si no se tratan las propiedades como fields se hace algo, pero no sabemos si es property...
                                    //if (!Globals.properties_as_fields)
                                    //    _broker.HandleNonInstrumentedMethod(new List<Term>(), t.IsGlobal ? null : t.DiscardLast(), returnTerms, t, ((IPropertySymbol)localSymbolInfo).GetMethod);
                                }
                            }
                        }
                    }

                    return t;
                #endregion
                case SyntaxKind.ObjectCreationExpression:
                    #region ObjectCreation

                    var objectCreationSyntax = (ObjectCreationExpressionSyntax)syntaxNode;
                    var symbolInfoObj = Utils.GetMethodSymbolInfo(model, objectCreationSyntax);
                    if (symbolInfoObj == null)
                        BugLogging.Log(_originalStatement.FileId, syntaxNode, BugLogging.Behavior.WithoutSymbol);
                    else
                        BugLogging.Log(_originalStatement.FileId, syntaxNode, BugLogging.Behavior.WithoutIOperation);

                    var args = objectCreationSyntax.ArgumentList.Arguments
                                         .Select(x => CreateArgument(x))
                                         .Where(x => x != null)
                                         .ToList();


                    var newScalar = symbolInfoObj != null && !symbolInfoObj.ReceiverType.IsNotScalar();
                    // XXX: Constructor de Struct sin parametros no ejecuta codigo, debe venir EndInvocation. Opción 2: es new string()
                    var noExecution = symbolInfoObj != null && (symbolInfoObj.ReceiverType.CustomIsStruct() && args.Count == 0) || newScalar;
                    var isInstrumented = symbolInfoObj != null && IsInstrumented(symbolInfoObj.ReceiverType.Name, null, false);
                    var newTermObj = _termFactory.Create(operation, symbolInfoObj != null ? ISlicerSymbol.Create(symbolInfoObj.ReceiverType, null) : ISlicerSymbol.CreateObjectSymbol(), false, TermFactory.GetFreshName());

                    if (newScalar)
                    {
                        _broker.DefUseOperation(newTermObj, args.ToArray());
                        GetNextStatement(TraceType.EndInvocation, false);
                    }
                    else
                    {
                        if (isInstrumented || noExecution)
                        {
                            _broker.Alloc(newTermObj);
                            _broker.DefUseOperation(newTermObj);
                        }

                        if (isInstrumented)
                            HandleInstrumentedMethod(operation, null, args, newTermObj, null);

                        if (!isInstrumented && !noExecution)
                            HandleNonInstrumentedMethod(operation, args, null, newTermObj, symbolInfoObj?.ReceiverType);

                        if (isInstrumented || noExecution)
                        {
                            GetNextStatement(TraceType.EndInvocation, false);
                            if (symbolInfoObj.ReceiverType.CustomIsStruct() && args.Count() == 0)
                            {
                                var m = symbolInfoObj.ReceiverType.GetMembers().Where(x => (x is IFieldSymbol || x is IPropertySymbol) && !x.IsStatic).ToList();
                                foreach (var n in m)
                                {

                                    var tTerm = newTermObj.AddingField(new Field(n.Name, ISlicerSymbol.Create((n is IFieldSymbol) ? ((IFieldSymbol)n).Type : ((IPropertySymbol)n).Type, _typeArguments)));
                                    tTerm.Stmt = Utils.StmtFromSyntaxNode((CSharpSyntaxNode)operation.Syntax, _instrumentationResult);
                                    _broker.DefUseOperation(tTerm);
                                }
                            }
                        }


                    }

                    return newTermObj;

                #endregion
                case SyntaxKind.StackAllocArrayCreationExpression:
                    #region StackAllocArrayCreation
                    var stackAllocSyntaxNode = (StackAllocArrayCreationExpressionSyntax)syntaxNode;

                    //Console.WriteLine(invocationSyntaxNode.ToString() + "&" + _originalStatement.FileId + "&" + invocationSyntaxNode.SpanStart + "&" + invocationSyntaxNode.Span.End);

                    var typeInfoSA = model.GetTypeInfo(stackAllocSyntaxNode).Type;

                    BugLogging.Log(_originalStatement.FileId, syntaxNode, BugLogging.Behavior.WithoutIOperation);

                    var argsSA = ((ArrayRankSpecifierSyntax)(((ArrayTypeSyntax)((StackAllocArrayCreationExpressionSyntax)syntaxNode).Type).RankSpecifiers).FirstOrDefault()).Sizes
                                         .Select(x => Visit(GetOperation(x)))
                                         .Where(x => x != null)
                                         .ToList();

                    var returnTypeSA = ISlicerSymbol.Create(typeInfoSA, _typeArguments);
                    var newTermSA = _termFactory.Create(operation, returnTypeSA, false, TermFactory.GetFreshName());
                    HandleNonInstrumentedMethod(operation, argsSA, null, newTermSA, typeInfoSA, ".ctor");
                    return newTermSA;
                #endregion
                case SyntaxKind.FixedStatement:
                    #region FixedStatement
                    Visit(GetOperation(((FixedStatementSyntax)operation.Syntax).Declaration));
                    #endregion
                    break;
                case SyntaxKind.AddressOfExpression:
                    #region AddressOfExpression
                    // TODO XXX This expression is not well resolved since external method can "write" on memory addresses
                    var recTermAE = Visit(GetOperation(((PrefixUnaryExpressionSyntax)operation.Syntax).Operand));
                    var typeInfoAE = model.GetTypeInfo((PrefixUnaryExpressionSyntax)operation.Syntax).Type;
                    var returnTypeAE = ISlicerSymbol.Create(typeInfoAE, _typeArguments);
                    var newTermAE = _termFactory.Create(((PrefixUnaryExpressionSyntax)operation.Syntax), returnTypeAE, false, TermFactory.GetFreshName());
                    _broker.DefUseOperation(newTermAE, new Term[] { recTermAE });
                    return newTermAE;
                #endregion
                case SyntaxKind.IdentifierName:
                    return new Term(syntaxNode.ToString(), ISlicerSymbol.CreateObjectSymbol());
                case SyntaxKind.PointerIndirectionExpression:
                    return Visit(GetOperation(((PrefixUnaryExpressionSyntax)syntaxNode).Operand));
                default:
                    throw new NotImplementedException();
            }

            return null;
        }
        #endregion

        #region Assignments and declarations
        void VisitVariableDeclarationStatement(IVariableDeclarationGroupOperation operation)
        {
            foreach (var v in operation.Declarations)
                Visit(v);
        }

        void VisitVariableDeclaration(IVariableDeclarationOperation operation)
        {
            foreach (var d in operation.Declarators)
                Visit(d);
        }

        void VisitVariableDeclarator(IVariableDeclaratorOperation operation)
        {
            var use = Visit(operation.Initializer);
            var def = _termFactory.Create(operation, ISlicerSymbol.Create(operation.Symbol.Type), false, ((ILocalSymbol)operation.Symbol).Name, false);

            // Special case, structs don't need an implicit initializer
            if (use == null && operation.Symbol.Type.CustomIsStruct())
            {
                _broker.Alloc(def);
                _broker.DefUseOperation(def);

                var m = operation.Symbol.Type.GetMembers()
                    .Where(x => (x is IFieldSymbol || x is IPropertySymbol) && !x.IsStatic).ToList();
                foreach (var n in m)
                {
                    var tTerm = def.AddingField(new Field(n.Name, ISlicerSymbol.Create((n is IFieldSymbol) ? ((IFieldSymbol)n).Type : ((IPropertySymbol)n).Type, _typeArguments)));
                    tTerm.Stmt = Utils.StmtFromSyntaxNode((CSharpSyntaxNode)operation.Syntax, _instrumentationResult);
                    _broker.DefUseOperation(tTerm);
                }
            }
            else
                _broker.Assign(def, use);
        }

        Term VisitDeclarationExpression(IDeclarationExpressionOperation operation)
        {
            var def = Visit(operation.Expression);
            _broker.DefUseOperation(def);
            return def;
        }

        Term VisitIncrementExpression(IIncrementOrDecrementOperation operation)
        {
            Term def = null;
            List<Term> dependentTerms = new List<Term>();
            if (operation.Target is IArrayElementReferenceOperation)
            {
                var t = VisitArrayElementReferenceExpression((IArrayElementReferenceOperation)operation.Target, true);
                def = t.Item1;
                dependentTerms = t.Item2;
            }
            else if (operation.Target is IPropertyReferenceOperation && ((IPropertyReferenceOperation)operation.Target).IsIndexer())
            {
                var t = VisitIndexedPropertyReferenceExpression((IPropertyReferenceOperation)operation.Target, true);
                def = t.Item1;
                dependentTerms = t.Item2;
            }
            else
                def = Visit(operation.Target);

            // OVERRIDE DEL OPERADOR --/++
            var tmp = _traceConsumer.ObserveNextStatement();
            if (tmp.TraceType == TraceType.EnterStaticMethod &&
                tmp.CSharpSyntaxNode is OperatorDeclarationSyntax &&
                ((((OperatorDeclarationSyntax)tmp.CSharpSyntaxNode).OperatorToken.ValueText.Equals("++")) ||
                (((OperatorDeclarationSyntax)tmp.CSharpSyntaxNode).OperatorToken.ValueText.Equals("--"))))
            {
                // Como es estático esto va como parámetro, no como "this"
                // TODO: Hacer el no estático (si existe...)
                var returnTerm = _termFactory.Create(operation, ISlicerSymbol.Create(operation.Type), false, TermFactory.GetFreshName());
                HandleInstrumentedMethod(operation, returnTerm, new List<Term> { def }, null);
                return returnTerm;
            }
            else
            {
                if (operation.Target is IPropertyReferenceOperation && !operation.Target.IsInSource())
                    CheckForSetCallbacks(def, def, dependentTerms, ((IPropertyReferenceOperation)operation.Target).Property);
                dependentTerms.Add(def);
                _broker.DefUseOperation(def, dependentTerms.ToArray());
                return def;
            }
        }

        Term VisitCompoundAssignmentExpression(ICompoundAssignmentOperation operation)
        {
            // TODO: El operador += con string y override del + creo que fallaría
            // NOTA IMPORTANTE: El funcionamiento es así (PROBADO):
            // 1: Se obtiene el objecto principal (el reciver del lado izquierdo)
            // 2: Se le hace GET a la property
            // 3: Se le hace SET a la property
            // ESO IMPLICA:
            // Si hacés A().B().myProperty += 1;
            // 1. SE EJECUTA A().B() ==> Esto lo estamos haciendo visitando el GET
            // 2. SE EJECUTA EL GET
            // 3. SE EJECUTA EL LADO DERECHO
            // 4. SE EJECUTA EL SET

            // ACÁ CONSUME TODO. INCLUSIVE CAPTURA EL GET SI LO TIENE! Ya que detecta que hay un get al leer la property!
            // POR OTRO LADO... Los posibles accessos o callbacks del GET necesitan un EndMemberAccess 
            // que se agregó antes de que ejecute el lado derecho lo consume el Visit de la property izquierda!
            // Quedó genial
            List<Term> dependentTerms = new List<Term>();
            Term def;
            Term set_receiver = null;
            if (operation.Target is IArrayElementReferenceOperation)
            {
                var t = VisitArrayElementReferenceExpression((IArrayElementReferenceOperation)operation.Target, true);
                def = t.Item1;
                dependentTerms = t.Item2;
            }
            else if (operation.Target is IPropertyReferenceOperation && ((IPropertyReferenceOperation)operation.Target).IsIndexer())
            {
                var t = VisitIndexedPropertyReferenceExpression((IPropertyReferenceOperation)operation.Target, true);
                def = t.Item1;
                dependentTerms = t.Item2;
            }
            else if (operation.Target is IPropertyReferenceOperation)
            {
                var temp = VisitPropertyReferenceExpression((IPropertyReferenceOperation)operation.Target, false);
                set_receiver = temp.Item1;
                def = temp.Item2;
            }
            else
                def = Visit(operation.Target);
            var use = Visit(operation.Value);

            var binaryResult = _termFactory.Create(operation, def.Last.Symbol);
            if (!binaryResult.IsScalar)
                _broker.Alloc(binaryResult);
            _broker.DefUseOperation(binaryResult, new Term[] { def, use });

            bool isSetAssignment;
            if (operation.Target is IPropertyReferenceOperation &&
                HasAccesor(((IPropertyReferenceOperation)operation.Target).Property.Name, null, out isSetAssignment))
            {
                Term @this = null;
                List<Term> arguments = null;
                if (((IPropertyReferenceOperation)operation.Target).IsIndexer())
                {
                    @this = Visit(((IPropertyReferenceOperation)operation.Target).Instance);
                    var args = ((IPropertyReferenceOperation)operation.Target).Arguments;
                    arguments = new List<Term>();
                    foreach (var arg in args)
                        arguments.Add(Visit(arg));
                    if (binaryResult != null)
                        arguments.Add(binaryResult);
                }
                else
                {
                    @this = set_receiver;
                    arguments = binaryResult != null ? new List<Term>() { binaryResult } : null;
                }

                HandleInstrumentedMethod(operation, null, arguments, @this);
            }
            else
            {
                if (operation.Target is IPropertyReferenceOperation && !operation.Target.IsInSource())
                    CheckForSetCallbacks(def, use, dependentTerms, ((IPropertyReferenceOperation)operation.Target).Property);

                _broker.Assign(def, binaryResult, dependentTerms);
            }

            return use;
        }

        Term VisitAssignmentExpression(IAssignmentOperation operation)
        {
            Term def = null;
            List<Term> dependentTerms = new List<Term>();
            var isIndexer = false;
            if (operation.Target is IArrayElementReferenceOperation)
            {
                var t = VisitArrayElementReferenceExpression((IArrayElementReferenceOperation)operation.Target, true);
                def = t.Item1;
                dependentTerms = t.Item2;
            }
            else if (operation.Target is IPropertyReferenceOperation && ((IPropertyReferenceOperation)operation.Target).IsIndexer())
            {
                var t = VisitIndexedPropertyReferenceExpression((IPropertyReferenceOperation)operation.Target, true);
                def = t.Item1;
                dependentTerms = t.Item2;
                isIndexer = true;
            }
            else if (operation.Target is IPropertyReferenceOperation)
            {
                def = VisitPropertyReferenceExpression((IPropertyReferenceOperation)operation.Target, true).Item2;
            }
            else
                def = Visit(operation.Target);

            var use = Visit(operation.Value);
            bool isSetAssignment;
            if (operation.Target is IPropertyReferenceOperation &&
                HasAccesor(((IPropertyReferenceOperation)operation.Target).Property.Name, null, out isSetAssignment, isIndexer) && isSetAssignment)
            {
                Term @this = null;
                List<Term> arguments = null;
                if (((IPropertyReferenceOperation)operation.Target).IsIndexer())
                {
                    @this = Visit(((IPropertyReferenceOperation)operation.Target).Instance);
                    var args = ((IPropertyReferenceOperation)operation.Target).Arguments;
                    arguments = new List<Term>();
                    foreach (var arg in args)
                        arguments.Add(Visit(arg));
                    if (use != null)
                        arguments.Add(use);
                }
                else
                {
                    @this = ((IPropertyReferenceOperation)operation.Target).Property.IsStatic ? null : def.DiscardLast();
                    arguments = use != null ? new List<Term>() { use } : null;
                }

                HandleInstrumentedMethod(operation, null, arguments, @this);
            }
            else
            {
                if ((operation.Target is IPropertyReferenceOperation && !operation.Target.IsInSource())
                    || (operation.Target is IPropertyReferenceOperation && ((IPropertyReferenceOperation)operation.Target).IsIndexer()))
                    CheckForSetCallbacks(def, use, dependentTerms, ((IPropertyReferenceOperation)operation.Target).Property);

                ISymbol _paramSymbol = null;
                var methodName = "";
                if (operation.Target is IPropertyReferenceOperation)
                    _paramSymbol = ((IPropertyReferenceOperation)operation.Target).Property;
                if (operation.Target is IFieldReferenceOperation)
                    _paramSymbol = ((IFieldReferenceOperation)operation.Target).Field;

                if (!Globals.properties_as_fields && (operation.Target is IPropertyReferenceOperation && ((IPropertyReferenceOperation)operation.Target).IsIndexer()))
                    _broker.HandleNonInstrumentedMethod(dependentTerms, def, new List<Term>(), null, ((IPropertySymbol)_paramSymbol).SetMethod, methodName);
                else if (def != null)
                    _broker.Assign(def, use, dependentTerms);
            }
            // Los Assignment retornan el def.
            return use;
        }

        Term VisitDeconstructionAssignmentExpression(IDeconstructionAssignmentOperation operation)
        {
            var recTerm = Visit(operation.Value);
            var i = 0;
            var elements = operation.Target is ITupleOperation ?
                ((ITupleOperation)operation.Target).Elements :
                ((ITupleOperation)((IDeclarationExpressionOperation)operation.Target).Expression).Elements;
            foreach (var e in elements)
            {
                var leftTerm = Visit(e);
                var tempTerm = recTerm.AddingField("x" + i, ISlicerSymbol.Create(e.Type, _typeArguments));
                _broker.Assign(leftTerm, tempTerm);
            }
            return recTerm;
        }
        #endregion

        #region Object creations and methods
        Term VisitUnaryOperatorExpression(IUnaryOperation operation)
        {
            Term newTerm = Visit(operation.Operand);
            // Operator overloading
            if (_traceConsumer.HasNext() &&
                ObserveNextStatement().TraceType == TraceType.EnterStaticMethod &&
                (ObserveNextStatement().CSharpSyntaxNode is OperatorDeclarationSyntax))
            {
                var declToken = ((OperatorDeclarationSyntax)ObserveNextStatement().CSharpSyntaxNode).OperatorToken.ValueText;
                string opToken = null;
                if (operation.Syntax is PrefixUnaryExpressionSyntax)
                    opToken = ((PrefixUnaryExpressionSyntax)operation.Syntax).OperatorToken.ValueText;
                else if (operation.Syntax is PostfixUnaryExpressionSyntax)
                    opToken = ((PostfixUnaryExpressionSyntax)operation.Syntax).OperatorToken.ValueText;
                else
                    throw new InvalidOperationException();

                if (declToken.Equals(opToken))
                {
                    var useTerms = new List<Term>();
                    useTerms.Add(newTerm);
                    newTerm = _termFactory.Create(operation, ISlicerSymbol.Create(operation.Type), false, TermFactory.GetFreshName());
                    HandleInstrumentedMethod(operation, newTerm, useTerms, null);
                }
            }

            return newTerm;
        }

        Term VisitBinaryOperatorExpression(IBinaryOperation operation)
        {
            Term recTermLeft = Visit(operation.LeftOperand);
            if ((Utils.IOperationStringBinaries.Contains(operation.OperatorKind)) &&
                (operation.LeftOperand.Type != null && operation.LeftOperand.Type.Name != "String") &&
                (operation.RightOperand.Type != null && operation.RightOperand.Type.Name == "String"))
            {
                var tmp = ObserveNextStatement();
                if (tmp.TraceType == TraceType.EndInvocation)
                    GetNextStatement(TraceType.EndInvocation);
                else if ((tmp.TraceType == TraceType.EnterMethod) &&
                        (((MethodDeclarationSyntax)tmp.CSharpSyntaxNode).Identifier.ValueText.Equals("ToString")))
                {
                    var returnTerm = _termFactory.Create(operation.LeftOperand,
                        ISlicerSymbol.Create(operation.Type), false, TermFactory.GetFreshName());
                    HandleInstrumentedMethod(operation.LeftOperand, returnTerm, null, recTermLeft);
                    recTermLeft = returnTerm;
                    GetNextStatement(TraceType.EndInvocation);
                }
                // TODO: ES CALLBACK
                else
                    throw new SlicerException("Funcionalidad no implementada");
            }

            Term recTermRight = null;

            if (Utils.ShortcircuitsBinaries.Contains(((BinaryExpressionSyntax)operation.Syntax).Kind()) &&
               (ObserveNextStatement().TraceType == TraceType.EnterExpression) &&
               (ObserveNextStatement().CSharpSyntaxNode.FullSpan == operation.Syntax.FullSpan))
            {
                GetNextStatement(TraceType.EnterExpression);
                recTermRight = Visit(operation.RightOperand);
            }
            else if (!Utils.ShortcircuitsBinaries.Contains(((BinaryExpressionSyntax)operation.Syntax).Kind()))
                recTermRight = Visit(operation.RightOperand);

            if ((Utils.IOperationStringBinaries.Contains(operation.OperatorKind))
                && (operation.RightOperand.Type != null && operation.RightOperand.Type.Name != "String") &&
                (operation.LeftOperand.Type != null && operation.LeftOperand.Type.Name == "String"))
            {
                var tmp = _traceConsumer.ObserveNextStatement();
                if (tmp.TraceType == TraceType.EndInvocation)
                    GetNextStatement(TraceType.EndInvocation);
                else if ((tmp.TraceType == TraceType.EnterMethod) &&
                (((MethodDeclarationSyntax)tmp.CSharpSyntaxNode).Identifier.ValueText.Equals("ToString")))
                {
                    var returnTerm = _termFactory.Create(operation.RightOperand,
                        ISlicerSymbol.Create(operation.Type), false, TermFactory.GetFreshName());
                    HandleInstrumentedMethod(operation.RightOperand, returnTerm, null, recTermRight);
                    recTermRight = returnTerm;
                    GetNextStatement(TraceType.EndInvocation);
                }
                // TODO: ES CALLBACK
                else
                {
                    //throw new SlicerException("Funcionalidad no implementada");
                    WaitForEnd((CSharpSyntaxNode)operation.RightOperand.Syntax, new List<Term>() { recTermLeft, recTermRight }, operation.RightOperand.Type, "WeirdCall", TraceType.EndInvocation, true);
                }
            }

            var useTerms = new List<Term>();
            if (recTermLeft != null)
                useTerms.Add(recTermLeft);
            if (recTermRight != null)
                useTerms.Add(recTermRight);

            var newTerm = _termFactory.Create(operation, ISlicerSymbol.Create(operation.Type), false, TermFactory.GetFreshName());

            // Operator overloading
            if (_traceConsumer.HasNext() &&
                ObserveNextStatement().TraceType == TraceType.EnterStaticMethod &&
                (ObserveNextStatement().CSharpSyntaxNode is OperatorDeclarationSyntax operationDeclarationSyntax) &&
                operationDeclarationSyntax.OperatorToken.ValueText.Equals(((BinaryExpressionSyntax)operation.Syntax).OperatorToken.ValueText))
            {
                var nextType = _semanticModelsContainer.GetBySyntaxNode(operationDeclarationSyntax).GetDeclaredSymbol(operationDeclarationSyntax.Parent) as ITypeSymbol;
                var leftOperation = operation.LeftOperand.Kind == OperationKind.Conversion ? ((IConversionOperation)operation.LeftOperand).Operand : operation.LeftOperand;
                if (TypesUtils.Compatibles(nextType, leftOperation.Type) ||
                    TypesUtils.Compatibles(leftOperation.Type, nextType))
                {
                    HandleInstrumentedMethod(operation, newTerm, useTerms, null);
                    if (_traceConsumer.HasNext() && ObserveNextStatement().TraceType == TraceType.EndInvocation)
                        GetNextStatement(TraceType.EndInvocation);
                }
                else
                    ;
            }
            // Pueden aplicarse operaciones binarias entre objetos y resultar en nuevos objetos, pero cuando es dynamic generalmente esto es bool
            // XXX: hasta ahora siempre fue así...
            else if (!newTerm.IsScalar && !newTerm.IsDynamic)
            {
                HandleNonInstrumentedMethod(operation, useTerms, null, newTerm, operation.OperatorMethod, ((BinaryExpressionSyntax)operation.Syntax).OperatorToken.ValueText);
                return newTerm;
            }

            _broker.DefUseOperation(newTerm, useTerms.ToArray());
            return newTerm;
        }

        Term VisitInvocationExpression(IInvocationOperation operation)
        {
            Term thisTerm = null;
            if (((IInvocationOperation)operation).Instance != null &&
                (((ITypeSymbol)((IInvocationOperation)operation).TargetMethod.ContainingSymbol)).TypeKind == TypeKind.Delegate &&
                ((IInvocationOperation)operation).Instance is IPropertyReferenceOperation)
                thisTerm = Visit(((IPropertyReferenceOperation)((IInvocationOperation)operation).Instance).Instance);
            else
                thisTerm = Visit(operation.Instance);

            if (operation.TargetMethod.MethodKind == MethodKind.LocalFunction &&
                !operation.TargetMethod.IsStatic &&
                thisTerm == null &&
                _thisObject != null)
                thisTerm = _termFactory.Create(operation, _thisObject.Last.Symbol, false, "this", false);

            List<Term> arguments = null;
            if (operation.Arguments.Length == operation.TargetMethod.Parameters.Length)
            {
                var argDict = new Dictionary<IArgumentOperation, Term>();
                foreach (var arg in operation.Arguments)
                {
                    var argTerm = CreateArgument(arg);
                    argDict.Add(arg, argTerm);
                }
                arguments = operation.TargetMethod.Parameters
                    .Select(x => argDict[operation.Arguments.Single(y => y.Parameter.Equals(x))]).ToList();
            }
            else
                // Default, as always, TODO: do the same with constructors, or not?
                arguments = operation.Arguments.Select(x => CreateArgument(x)).Where(x => x != null).ToList();


            var dictionary = Utils.GetTypesDictionary(operation.TargetMethod, _typeArguments);
            var newTerm = _termFactory.Create(operation, ISlicerSymbol.Create(operation.TargetMethod.ReturnType, _typeArguments), false, TermFactory.GetFreshName());

            var isException = methodsExceptions.Any(x =>
                ((_originalStatement != null && x.Item1 == _originalStatement.FileId) || (_originalStatement == null && x.Item1 == _currentFileId)) &&
                x.Item2 <= operation.Syntax.Span.Start && x.Item3 >= operation.Syntax.Span.End);
            if (IsInstrumented(operation.TargetMethod.Name, dictionary, true, operation.TargetMethod.CustomStructReceiver(), null, arguments.Count, operation.TargetMethod.IsStatic) && !isException)
            {
                HandleInstrumentedMethod(operation, newTerm, arguments, operation.TargetMethod.IsStatic ? null : thisTerm, dictionary);
                if (Globals.wrap_structs_calls || !operation.TargetMethod.CustomStructReceiver())
                { 
                    var tempGetNextStmt = GetNextStatement(TraceType.EndInvocation, false);
                    if (tempGetNextStmt == null)
                        Console.WriteLine(string.Format("ADD: {0} - {1} - {2} - {3}", _originalStatement.FileId, operation.Syntax.SpanStart, operation.Syntax.Span.End, operation.Syntax.ToString()));
                }
                if (operation.TargetMethod.ReturnsVoid)
                    _broker.DefUseOperation(newTerm);
            }
            else
                HandleNonInstrumentedMethod(operation, arguments, thisTerm, newTerm, operation.TargetMethod, null, operation.Syntax);
            return newTerm;
        }

        Term VisitObjectCreationExpression(IObjectCreationOperation operation)
        {
            // Si hacés new string(char array) es un escalar, no hay que alocarlo... hay que tratarlo como un literal que usa lo que recibe
            var newScalar = !operation.Constructor.ReceiverType.IsNotScalar();
            // XXX: Constructor de Struct sin parametros no ejecuta codigo, debe venir EndInvocation. Opción 2: es new string()
            var noExecution = (operation.Type.CustomIsStruct() && operation.Arguments.Count() == 0) || newScalar;
            var dictionary = Utils.GetTypesDictionary(operation.Constructor, _typeArguments);
            
            var newTerm = _termFactory.Create(operation, ISlicerSymbol.Create(operation.Type, _typeArguments), false, TermFactory.GetFreshName());

            List<Term> arguments = null;
            if (operation.Arguments.Length == operation.Constructor.Parameters.Length)
            {
                var argDict = new Dictionary<IArgumentOperation, Term>();
                foreach (var arg in operation.Arguments)
                {
                    var argTerm = CreateArgument(arg);
                    argDict.Add(arg, argTerm);
                }
                arguments = operation.Constructor.Parameters
                    .Select(x => argDict[operation.Arguments.Single(y => y.Parameter.Equals(x))]).ToList();
            }
            else
                // Default, as always, TODO: do the same with constructors, or not?
                arguments = operation.Arguments.Select(x => CreateArgument(x)).Where(x => x != null).ToList();

            if (newScalar)
            {
                _broker.DefUseOperation(newTerm, arguments.ToArray());
                GetNextStatement(TraceType.EndInvocation);
            }
            else
            {
                var isInstrumented = IsInstrumented(operation.Constructor.ReceiverType.Name, dictionary, objectCreationOperation: operation);
                if (isInstrumented || noExecution)
                {
                    _broker.Alloc(newTerm);
                    _broker.DefUseOperation(newTerm);
                }

                var wellHandled = true;
                if (isInstrumented)
                    wellHandled = HandleInstrumentedMethod(operation, null, arguments, newTerm, dictionary);

                if (!isInstrumented && !noExecution)
                    HandleNonInstrumentedMethod(operation, arguments, null, newTerm, operation.Constructor, null, operation.Syntax);

                if (!wellHandled)
                    isInstrumented = false;

                if (isInstrumented || noExecution)
                {
                    GetNextStatement(TraceType.EndInvocation);
                    if (operation.Type.CustomIsStruct() && operation.Arguments.Count() == 0)
                    {
                        var m = operation.Type.GetMembers().Where(x => (x is IFieldSymbol || x is IPropertySymbol) && !x.IsStatic).ToList();
                        foreach (var n in m)
                        {

                            var tTerm = newTerm.AddingField(new Field(n.Name, ISlicerSymbol.Create((n is IFieldSymbol) ? ((IFieldSymbol)n).Type : ((IPropertySymbol)n).Type, _typeArguments)));
                            tTerm.Stmt = Utils.StmtFromSyntaxNode((CSharpSyntaxNode)operation.Syntax, _instrumentationResult);
                            _broker.DefUseOperation(tTerm);
                        }
                    }
                }

                if (operation.Initializer != null)
                    foreach (var init in operation.Initializer.Initializers)
                    {
                        if (init.Kind == OperationKind.SimpleAssignment)
                        {
                            var use = Visit(((ISimpleAssignmentOperation)init).Value);
                            if (((ISimpleAssignmentOperation)init).Target.Kind == OperationKind.FieldReference)
                            {
                                var field = ((IFieldReferenceOperation)((ISimpleAssignmentOperation)init).Target).Field;
                                var def = _termFactory.Create(((ISimpleAssignmentOperation)init).Target, ISlicerSymbol.Create(field.Type), false, field.Name);
                                var completeTerm = newTerm.AddingField(def.ToString(), use.Last.Symbol);
                                completeTerm.Stmt = Utils.StmtFromSyntaxNode((CSharpSyntaxNode)((ISimpleAssignmentOperation)init).Target.Syntax, _instrumentationResult);
                                _broker.Assign(completeTerm, use);
                            }
                            else if (((ISimpleAssignmentOperation)init).Target.Kind == OperationKind.PropertyReference)
                            {
                                var property = ((IPropertyReferenceOperation)((ISimpleAssignmentOperation)init).Target).Property;

                                bool isSetAssignment;
                                if (HasAccesor(property.Name, null, out isSetAssignment))
                                {
                                    if (!isSetAssignment)
                                        throw new Exception("Deberia ser setAssginment y no es!");
                                    HandleInstrumentedMethod(operation, null, use != null ? new List<Term>() { use } : null, newTerm);
                                }
                                else
                                {
                                    var def = _termFactory.Create(((ISimpleAssignmentOperation)init).Target, ISlicerSymbol.Create(property.Type), false, property.Name);
                                    var completeTerm = newTerm.AddingField(def.ToString(), use.Last.Symbol);
                                    completeTerm.Stmt = Utils.StmtFromSyntaxNode((CSharpSyntaxNode)((ISimpleAssignmentOperation)init).Target.Syntax, _instrumentationResult);
                                    _broker.Assign(completeTerm, use);
                                }
                            }
                            else
                                throw new NotImplementedException();
                        }
                        else if (init.Kind == OperationKind.Invocation)
                        {
                            var initArgs = ((IInvocationOperation)init).Arguments.Select(x => Visit(x)).ToList();

                            if (IsInstrumented(((IInvocationOperation)init).TargetMethod.Name, dictionary, false))
                                HandleInstrumentedMethod(operation, null, initArgs, newTerm, dictionary);
                            else
                            {
                                var involvedTerms = new List<Term>(initArgs);
                                involvedTerms.Add(newTerm);
                                var returnedTerms = WaitForEnd((newTerm.Stmt.CSharpSyntaxNode), involvedTerms, ((IInvocationOperation)init).TargetMethod);
                                _broker.HandleNonInstrumentedMethod(initArgs, newTerm, returnedTerms, null, ((IInvocationOperation)init).TargetMethod);
                            }
                        }
                    }
            }

            return newTerm;
        }

        Term VisitAnonymousObjectCreationExpression(IAnonymousObjectCreationOperation operation)
        {
            var newTerm = _termFactory.Create(operation, ISlicerSymbol.CreateAnonymousSymbol(), false, TermFactory.GetFreshName());
            _broker.Alloc(newTerm);
            _broker.DefUseOperation(newTerm);

            foreach (var init in operation.Initializers)
            {
                if (init.Kind == OperationKind.SimpleAssignment)
                {
                    var use = Visit(((ISimpleAssignmentOperation)init).Value);
                    if (((ISimpleAssignmentOperation)init).Target.Kind == OperationKind.FieldReference)
                    {
                        var field = ((IFieldReferenceOperation)((ISimpleAssignmentOperation)init).Target).Field;
                        var def = _termFactory.Create(((ISimpleAssignmentOperation)init).Target, ISlicerSymbol.Create(field.Type), false, field.Name);
                        var completeTerm = newTerm.AddingField(def.ToString(), use.Last.Symbol);
                        completeTerm.Stmt = Utils.StmtFromSyntaxNode((CSharpSyntaxNode)((ISimpleAssignmentOperation)init).Target.Syntax, _instrumentationResult);
                        _broker.Assign(completeTerm, use);
                    }
                    else if (((ISimpleAssignmentOperation)init).Target.Kind == OperationKind.PropertyReference)
                    {
                        var property = ((IPropertyReferenceOperation)((ISimpleAssignmentOperation)init).Target).Property;

                        bool isSetAssignment;
                        if (HasAccesor(property.Name, null, out isSetAssignment))
                        {
                            if (!isSetAssignment)
                                throw new Exception("Deberia ser setAssginment y no es!");
                            HandleInstrumentedMethod(operation, null, use != null ? new List<Term>() { use } : null, newTerm);
                        }
                        else
                        {
                            var def = _termFactory.Create(((ISimpleAssignmentOperation)init).Target, ISlicerSymbol.Create(property.Type), false, property.Name);
                            var completeTerm = newTerm.AddingField(def.ToString(), use.Last.Symbol);
                            completeTerm.Stmt = Utils.StmtFromSyntaxNode((CSharpSyntaxNode)((ISimpleAssignmentOperation)init).Target.Syntax, _instrumentationResult);
                            _broker.Assign(completeTerm, use);
                        }
                    }
                    else
                        throw new NotImplementedException();
                }
                else
                    throw new NotImplementedException();
            }

            return newTerm;
        }

        private Term VisitTypeParameterObjectCreationExpression(ITypeParameterObjectCreationOperation operation)
        {
            var type = _typeArguments.SingleOrDefault(x => x.Key.TypeKind == TypeKind.TypeParameter && x.Key.Name == operation.Type.Name);
            if (type.Equals(default(KeyValuePair<ITypeSymbol, ITypeSymbol>)))
                ;
            var paramType = type.Value;
            var isScalar = !type.Value.IsNotScalar();
            var noExecution = operation.Type.CustomIsStruct() || isScalar;
            var isInstrumented = IsInstrumented(type.Value.Name);

            var newTerm = _termFactory.Create(operation, ISlicerSymbol.Create(operation.Type, _typeArguments), false, TermFactory.GetFreshName());

            if (isScalar)
            {
                _broker.DefUseOperation(newTerm);
                GetNextStatement(TraceType.EndInvocation);
            }
            else
            {
                _broker.Alloc(newTerm);
                _broker.DefUseOperation(newTerm);
                if (isInstrumented)
                    HandleInstrumentedMethod(operation, null, null, newTerm, _typeArguments);

                if (!isInstrumented && !noExecution)
                    HandleNonInstrumentedMethod((IOperation)null, null, newTerm, null, null);

                if (isInstrumented || noExecution)
                    GetNextStatement(TraceType.EndInvocation);
            }

            return newTerm;
        }
        #endregion

        #region Arrays
        Term VisitArrayCreationExpression(IArrayCreationOperation operation)
        {
            var uses = (operation.DimensionSizes != null) ? operation.DimensionSizes.Select(x => Visit(x)).ToList() : new List<Term>();

            var previousExpressions = new List<IOperation>();
            if (operation.Syntax is ArrayCreationExpressionSyntax && ((ArrayCreationExpressionSyntax)operation.Syntax).Initializer != null)
                previousExpressions.AddRange(((ArrayCreationExpressionSyntax)operation.Syntax).Initializer.Expressions.Select(x => GetOperation(x)).ToList());
            else if (operation.Syntax is ImplicitArrayCreationExpressionSyntax && ((ImplicitArrayCreationExpressionSyntax)operation.Syntax).Initializer != null)
                previousExpressions.AddRange(((ImplicitArrayCreationExpressionSyntax)operation.Syntax).Initializer.Expressions.Select(x => GetOperation(x)).ToList());
            // XXX: arrayCreationExpression.ElementValues.ArrayClass = Dimension
            // Se utiliza cuando tenemos params[]
            else if (operation.Initializer != null && operation.Initializer.ElementValues != null)
                operation.Initializer.ElementValues.ToList().ForEach(elementValue => previousExpressions.Add(elementValue));

            var dependentTerms = previousExpressions.Select(x => Visit(x)).ToList();

            var newTerm = _termFactory.Create(operation, ISlicerSymbol.Create(operation.Type), false, TermFactory.GetFreshName());

            _broker.Alloc(newTerm);
            _broker.DefUseOperation(newTerm, uses.ToArray());

            var otherField = new Field(Field.SIGMA_FIELD);
            otherField.Symbol = ISlicerSymbol.Create(((IArrayTypeSymbol)newTerm.Parts[0].Symbol.Symbol).ElementType);
            var otherTerm = newTerm.AddingField(otherField);
            otherTerm.Stmt = newTerm.Stmt;
            _broker.DefUseOperation(otherTerm, new Term[] { newTerm });

            foreach (var dependentTerm in dependentTerms)
            {
                otherField = new Field(Field.SIGMA_FIELD);
                otherField.Symbol = ISlicerSymbol.Create(((IArrayTypeSymbol)newTerm.Parts[0].Symbol.Symbol).ElementType);

                var termToAssign = newTerm.AddingField(otherField);
                termToAssign.Stmt = Utils.StmtFromSyntaxNode((CSharpSyntaxNode)operation.Syntax, _instrumentationResult);
                _broker.Assign(termToAssign, dependentTerm);
            }

            return newTerm;
        }

        Term VisitArrayInitializer(IArrayInitializerOperation operation)
        {
            var dependentTerms = new List<Term>();
            if (operation.ElementValues != null)
                dependentTerms.AddRange(operation.ElementValues.ToList().Select(x => Visit(x)));

            var newTerm = _termFactory.Create(operation, ISlicerSymbol.Create(operation.Parent.Type ?? operation.Parent.Parent.Type), false, TermFactory.GetFreshName());

            // XXX: Revisar
            _broker.Alloc(newTerm);
            _broker.DefUseOperation(newTerm, new Term[] { });

            var otherField = new Field(Field.SIGMA_FIELD);
            if (dependentTerms.Count == 0)
            {
                // Igual definimos sigma
                otherField.Symbol = ISlicerSymbol.Create(((IArrayTypeSymbol)operation.Parent.Type).ElementType);
                var otherTerm = newTerm.AddingField(otherField);
                otherTerm.Stmt = newTerm.Stmt;
                _broker.DefUseOperation(otherTerm, new Term[] { newTerm });
            }
            else
            {
                foreach (var dependentTerm in dependentTerms)
                {
                    otherField.Symbol = ISlicerSymbol.Create(((IArrayTypeSymbol)(operation.Parent.Type ?? operation.Parent.Parent.Type)).ElementType);
                    var otherTerm = newTerm.AddingField(otherField);
                    otherTerm.Stmt = newTerm.Stmt;
                    _broker.Assign(otherTerm, dependentTerm);
                }
            }

            return newTerm;
        }

        Term VisitArrayElementReferenceExpression(IArrayElementReferenceOperation operation)
        {
            return VisitArrayElementReferenceExpression(operation, false).Item1;
        }

        Tuple<Term, List<Term>> VisitArrayElementReferenceExpression(IArrayElementReferenceOperation operation, bool forSet)
        {
            var recTerm = Visit(operation.ArrayReference);
            var dependentTerms = operation.Indices.Select(x => Visit(x)).ToList();

            //if (Globals.use_new_annotations)
            //{
            //    if (forSet)
            //        return new Tuple<Term, List<Term>>(recTerm, dependentTerms);
            //    else
            //    {
            //        var newTerm = _termFactory.Create(operation, ISlicerSymbol.Create(operation.Type, _typeArguments));
            //        _broker.HandleNonInstrumentedMethod(dependentTerms, recTerm, new List<Term>(), newTerm, operation.Type, "this");
            //        return new Tuple<Term, List<Term>>(newTerm, null);
            //    }
            //}
            //else
            //{
            // TODO: XXXX: Reemplazar por anotaciones y oftype...
            var newTerm = recTerm.AddingField(Field.SigmaField(ISlicerSymbol.Create(operation.Type)));
            newTerm.Stmt = Utils.StmtFromSyntaxNode((CSharpSyntaxNode)operation.Syntax, _instrumentationResult);
            // Si es para SET debemos devolver lo que tiene que modificar... TODO: Si hubiera un HAVOC decente lo usaríamos :'(
            if (forSet)
                return new Tuple<Term, List<Term>>(newTerm, dependentTerms);
            else
            {
                var returnTerm = _termFactory.Create(operation, newTerm.Last.Symbol, false);
                _broker.Assign(returnTerm, newTerm, dependentTerms);
                return new Tuple<Term, List<Term>>(returnTerm, null);
            }
            //}
        }
        #endregion

        #region Control operations
        Term VisitNullCoalescingExpression(ICoalesceOperation operation)
        {
            // XXX: Se tiene que devolver el término correcto
            Term recTermLeft = Visit(operation.Value);
            Term recTermRight = null;

            if (ObserveNextStatement().TraceType == TraceType.EnterExpression)
            {
                GetNextStatement(TraceType.EnterExpression);
                recTermRight = Visit(operation.WhenNull);
            }

            // Si el de la derecha es distinto de null devolvemos ese, sino el izquierdo
            var termToAssign = recTermRight ?? recTermLeft;
            var newTerm = _termFactory.Create(operation, termToAssign.Last.Symbol, false, TermFactory.GetFreshName());
            _broker.Assign(newTerm, termToAssign);

            // XXX: Si asignamos el término derecho hay que agregar el USE del izquierdo porque este término utilizó ese para crearse
            if (recTermRight != null)
                _broker.DefUseOperation(newTerm, new Term[] { newTerm, recTermLeft });

            return newTerm;
        }

        Term VisitCoalesceAssignmentOperation(ICoalesceAssignmentOperation operation)
        {
            var left = Visit(operation.Target);
            var nextStmt = ObserveNextStatement();
            if (nextStmt.TraceType == TraceType.EnterExpression &&
                nextStmt.FileId == _originalStatement.FileId &&
                nextStmt.SpanStart <= _originalStatement.SpanStart &&
                nextStmt.SpanEnd >= _originalStatement.SpanEnd)
            {
                GetNextStatement(TraceType.EnterExpression);
                // TODO: complete left side with callback prevention

                var right = Visit(operation.Value);
                _broker.Assign(left, right);
            }
            return left;
        }

        Term VisitConditionalChoiceExpression(IConditionalOperation operation)
        {
            // 1) Leer la condición
            var conditionStmt = GetNextStatement(TraceType.SimpleStatement);
            var internalTerm = Visit(operation.Condition);
            _broker.UseOperation(
                Utils.StmtFromSyntaxNode(conditionStmt.CSharpSyntaxNode, _instrumentationResult),
                new List<Term>() { internalTerm });

            // 2) Leer la entrada
            var enterConditionStmt = conditionStmt;
            var enterConditionSyntaxNode = Utils.StmtFromSyntaxNode(enterConditionStmt.CSharpSyntaxNode, _instrumentationResult);
            _broker.EnterCondition(enterConditionSyntaxNode);
            // 3) Leer la instrucción
            var instructionStmt = GetNextStatement(TraceType.SimpleStatement);
            var recTerm = Visit(GetOperation((instructionStmt.CSharpSyntaxNode)));

            Term[] uses = new Term[] { recTerm };
            _broker.DefUseOperation(recTerm, uses);

            // 4) Exit
            _broker.ExitCondition(enterConditionSyntaxNode);

            return recTerm;
        }

        void VisitIfStatement(IConditionalOperation operation)
        {
            var internalTerm = Visit(operation.Condition);
            _broker.UseOperation(_originalStatement, new List<Term>() { internalTerm });
        }

        void VisitWhileUntilLoopStatement(IWhileLoopOperation operation)
        {
            var internalTerm = Visit(operation.Condition);
            _broker.UseOperation(_originalStatement, internalTerm != null ? new List<Term>() { internalTerm } : new List<Term>());
        }

        void VisitForOperation(IForLoopOperation operation)
        {
            var internalTerm = Visit(operation.Condition);
            _broker.UseOperation(_originalStatement, internalTerm != null ? new List<Term>() { internalTerm } : new List<Term>());
        }

        void VisitForEachLoopStatement(IForEachLoopOperation operation)
        {
            var term = Visit(operation.Collection);

            // TODO: Fallaría el override del GetEnumerator
            // Llamada implícita al GetEnumerator. Puede ser propia o no. TODO: Por ahora es solo externa
            // XXX: Si ahora cae una invocación, suponemos que es por el GetEnumerable de la collection
            Term returnHub = null;
            var returnedValues = WaitForEnd((CSharpSyntaxNode)operation.Collection.Syntax, new List<Term>() { term }, operation.Collection.Type, "GetEnumerator", TraceType.EndInvocation, true);

            IMethodSymbol methodSymbol = null;
            if (operation.Collection.Type.Name != "IEnumerable")
            {
                var typeSymbol = operation.Collection.Type.AllInterfaces.Where(x => x.Name.Contains("IEnumerable")).FirstOrDefault();
                if (typeSymbol != null)
                    methodSymbol = (IMethodSymbol)(typeSymbol.GetMembers().First());
                else
                    methodSymbol = (IMethodSymbol)((INamedTypeSymbol)operation.Collection.Type).GetMembers().First(x => x.Name == "GetEnumerator");
            }
            else
                methodSymbol = (IMethodSymbol)(operation.Collection.Type.GetMembers().First());

            returnHub = _termFactory.Create(operation.Collection,
            ISlicerSymbol.Create(methodSymbol.ReturnType, _typeArguments), false, TermFactory.GetFreshName(), false);

            _broker.CustomEvent(new List<Term>(), term, returnedValues, returnHub, "ForeachGetEnumerator");

            // Porque puede existir: porque hay un foreach dentro de otro foreach, entonces ya existe la clave del foreach de la iteración anterior y la reemplaza
            if (_foreachHubsDictionary.ContainsKey((CSharpSyntaxNode)operation.Syntax))
                _foreachHubsDictionary[(CSharpSyntaxNode)operation.Syntax] = returnHub;
            else
                _foreachHubsDictionary.Add((CSharpSyntaxNode)operation.Syntax, returnHub);
            CheckForMoveNextCallbacks(operation);
        }

        void VisitSwitchStatement(ISwitchOperation operation)
        {
            var recTerm = Visit(operation.Value);
            temporarySwitchTerm = recTerm;
            _broker.UseOperation(_originalStatement, new List<Term>() { recTerm });

            var nextStmt = ObserveNextStatement();
            if (nextStmt.TraceType == TraceType.EnterMethod && nextStmt.CSharpSyntaxNode.GetContainer().GetName().Equals("Deconstruct"))
            {
                var currentSymbol = (IMethodSymbol)_semanticModelsContainer.GetBySyntaxNode(nextStmt.CSharpSyntaxNode)
                    .GetDeclaredSymbol((nextStmt.CSharpSyntaxNode is ArrowExpressionClauseSyntax) ? nextStmt.CSharpSyntaxNode.Parent : nextStmt.CSharpSyntaxNode);
                var paramTerms = new List<Term>();
                foreach (var p in currentSymbol.Parameters)
                {
                    var tp = _termFactory.Create(operation, ISlicerSymbol.Create(p.Type));
                    tp.IsOutOrRef = p.RefKind == RefKind.Out || p.RefKind == RefKind.Ref;
                    tp.ReferencedTerm = tp;
                    _broker.DefUseOperation(tp);
                    paramTerms.Add(tp);
                }


                if (HandleInstrumentedMethod(operation, null, paramTerms, recTerm, _typeArguments))
                {
                    var enterConditionStmt = ObserveNextStatement();
                    if (enterConditionStmt.TraceType == TraceType.EnterCondition)
                    {
                        EnterCondition(_originalStatement);
                        GetNextStatement();
                        var firstStmt = ObserveNextStatement();
                        var switchSectionSyntax = firstStmt.CSharpSyntaxNode.GetCaseContainer();
                        if (((SwitchSectionSyntax)switchSectionSyntax).Labels.First() is CasePatternSwitchLabelSyntax casePatternSwitchLabelSyntax &&
                            casePatternSwitchLabelSyntax.Pattern is RecursivePatternSyntax recursivePatternSyntax)
                        {
                            var tempSubpatterns = recursivePatternSyntax.PositionalPatternClause.Subpatterns.ToList();
                            var i = 0;
                            foreach (var subpattern in tempSubpatterns)
                            {
                                var name = ((SingleVariableDesignationSyntax)((DeclarationPatternSyntax)subpattern.Pattern).Designation).Identifier.ValueText;
                                var declaringTerm = _termFactory.Create(subpattern.Pattern, paramTerms[i].Last.Symbol, false, name, false);
                                _broker.Assign(declaringTerm, paramTerms[i++]);
                            }
                        }
                    }
                    else
                        throw new NotImplementedException();
                }
            }

            try
            {
                foreach (var p in operation.Cases)
                    Visit(p);
            }
            catch (EnterSwitchException ex)
            {
                ;
            }
        }

        Term VisitSwitchExpression(ISwitchExpressionOperation operation)
        {
            Term retTerm = null;
            var valueTerm = Visit(operation.Value);
            // TODO: temporarySwitchTerm = valueTerm;

            var switchStmt = Utils.StmtFromSyntaxNode((CSharpSyntaxNode)operation.Syntax, this._instrumentationResult);
            _broker.UseOperation(switchStmt, new List<Term>() { valueTerm });

            var nextStmt = ObserveNextStatement();
            while (nextStmt.CSharpSyntaxNode is WhenClauseSyntax ||
                nextStmt.CSharpSyntaxNode is SwitchExpressionArmSyntax)
            {
                GetNextStatement();

                if (nextStmt.CSharpSyntaxNode is WhenClauseSyntax)
                {
                    var conditionOp = GetOperation(((WhenClauseSyntax)nextStmt.CSharpSyntaxNode).Condition);
                    var conditionTerm = Visit(conditionOp);

                    nextStmt = ObserveNextStatement();
                    if (nextStmt.TraceType == TraceType.EnterExpression)
                    {
                        GetNextStatement();

                        var whenStmt = Utils.StmtFromSyntaxNode((CSharpSyntaxNode)nextStmt.CSharpSyntaxNode, this._instrumentationResult);
                        _broker.UseOperation(whenStmt, new List<Term>() { conditionTerm });
                        EnterCondition(switchStmt);
                        EnterCondition(whenStmt);

                        var expOp = GetOperation(((SwitchExpressionArmSyntax)(((WhenClauseSyntax)nextStmt.CSharpSyntaxNode).Parent)).Expression);
                        retTerm = Visit(expOp);

                        ExitCondition(whenStmt);
                        ExitCondition(switchStmt);
                        nextStmt = ObserveNextStatement();
                    }
                }
                else if (nextStmt.CSharpSyntaxNode is SwitchExpressionArmSyntax)
                {
                    nextStmt = ObserveNextStatement();
                    if (nextStmt.TraceType == TraceType.EnterExpression)
                    {
                        var armOp = GetOperation(nextStmt.CSharpSyntaxNode);
                        if (armOp is ISwitchExpressionArmOperation switchExpOp && switchExpOp.Pattern != null)
                        {
                            if (switchExpOp.Pattern is IDeclarationPatternOperation)
                            {
                                var pattern = (IDeclarationPatternOperation)(switchExpOp.Pattern);
                                var matchedType = pattern.NarrowedType;
                                var varName = pattern.DeclaredSymbol.Name;
                                var newTerm = _termFactory.Create((CSharpSyntaxNode)pattern.Syntax, ISlicerSymbol.Create(matchedType, _typeArguments), false, varName, false, false);

                                _broker.Assign(newTerm, valueTerm);
                            }
                            // TODO: Repeated code
                            else if (switchExpOp.Pattern is IRecursivePatternOperation recPatternOp)
                            {
                                PatternOperationReceiver = valueTerm;
                                foreach (var propSupPat in recPatternOp.PropertySubpatterns)
                                {
                                    var member = Visit(propSupPat.Member);
                                    //usedTerms.Add(member);
                                    if (propSupPat.Pattern is IDeclarationPatternOperation)
                                    {
                                        if (!(((IDeclarationPatternOperation)propSupPat.Pattern).DeclaredSymbol is null))
                                        {
                                            var varMem = _termFactory.Create(propSupPat.Pattern, ISlicerSymbol.Create(propSupPat.Pattern.NarrowedType), false, ((IDeclarationPatternOperation)propSupPat.Pattern).DeclaredSymbol.Name, false, false);
                                            _broker.Assign(varMem, member, new List<Term>() { valueTerm });
                                        }
                                    }
                                }
                                PatternOperationReceiver = null;

                                if (recPatternOp.DeclaredSymbol != null)
                                {
                                    var declaredVariable = _termFactory.Create(recPatternOp, ISlicerSymbol.Create(recPatternOp.NarrowedType), false, recPatternOp.DeclaredSymbol.Name, false, false);
                                    _broker.Assign(declaredVariable, valueTerm, new List<Term>() { valueTerm });
                                }
                            }
                        }

                        GetNextStatement();
                        EnterCondition(switchStmt);
                        var expOp = GetOperation(((SwitchExpressionArmSyntax)nextStmt.CSharpSyntaxNode).Expression);
                        retTerm = Visit(expOp);
                        nextStmt = ObserveNextStatement();
                        ExitCondition(switchStmt);
                    }
                }
            }

            return retTerm;
        }

        void VisitSwitchCase(ISwitchCaseOperation operation)
        {
            foreach (var clause in operation.Clauses)
                Visit(clause);
        }

        void VisitCaseClause(ICaseClauseOperation operation)
        {
            if (operation is IPatternCaseClauseOperation patternCaseClauseOperation)
            {
                var uses = new List<Term>();
                DealWithPatterns(temporarySwitchTerm, patternCaseClauseOperation.Pattern, uses);

                if (patternCaseClauseOperation.Guard != null)
                {
                    var nextStmt = ObserveNextStatement();
                    if (nextStmt.CSharpSyntaxNode is WhenClauseSyntax &&
                        nextStmt.CSharpSyntaxNode == patternCaseClauseOperation.Guard.Syntax.Parent)
                    {
                        GetNextStatement();
                        var recTerm = Visit(patternCaseClauseOperation.Guard);

                        nextStmt = ObserveNextStatement();
                        if (nextStmt.TraceType == TraceType.EnterExpression &&
                            nextStmt.CSharpSyntaxNode == patternCaseClauseOperation.Guard.Syntax.Parent)
                        {
                            GetNextStatement();
                            // The next trace has to be EnterCondition (section body)
                            var switchStmt = GetNextStatement();
                            if (switchStmt.TraceType != TraceType.EnterCondition)
                                throw new SlicerException("Unexpected behavior");

                            EnterCondition(switchStmt);
                            uses.Add(recTerm);
                            var whenStmt = Utils.StmtFromSyntaxNode(
                                (CSharpSyntaxNode)patternCaseClauseOperation.Guard.Syntax.Parent, _instrumentationResult);
                            _broker.UseOperation(whenStmt, uses);
                            ExitCondition(switchStmt);
                            EnterCondition(whenStmt);
                            throw new EnterSwitchException();
                        }
                    }
                }
            }
        }

        void VisitBranchStatement(IBranchOperation operation)
        {
            _broker.Continue();
            if (operation.BranchKind == BranchKind.Continue)
            {
                var container = operation.Syntax.GetLoopContainer();
                if (container is ForEachStatementSyntax)
                    CheckForMoveNextCallbacks((IForEachLoopOperation)GetOperation((CSharpSyntaxNode)container));
            }
        }

        void VisitReturnStatement(IReturnOperation operation)
        {
            var _returnTerm = Visit(((IReturnOperation)operation).ReturnedValue);
            SetReturn(operation, _returnTerm);
        }

        void VisitArrowExpressionClause(IOperation operation)
        {
            var _returnTerm = Visit(operation);
            SetReturn(operation, _returnTerm);
        }

        void SetReturn(IOperation operation, Term _returnTerm)
        {
            if (_returnTerm != null)
            {
                var newTerm = _termFactory.Create(operation, _returnTerm.Last.Symbol, false, TermFactory.GetFreshName());
                _broker.Assign(newTerm, _returnTerm);

                // XXX: Estamos retorando un tipo complejo a través de un escalar, lo tenemos que alocar
                if (_returnComplexType && newTerm.IsScalar && !newTerm.Last.Symbol.IsNullSymbol)
                {
                    newTerm.Last.Symbol = ISlicerSymbol.CreateObjectSymbol();
                    newTerm.IsScalar = false;
                    _broker.Alloc(newTerm);
                }

                AssignRV(newTerm);
            }
            // Caso Tasks
            else if (_returnComplexType)
            {
                var type = ((IMethodSymbol)(_semanticModelsContainer.GetBySyntaxNode((CSharpSyntaxNode)_methodNode)
                .GetDeclaredSymbol((((CSharpSyntaxNode)_methodNode) is ArrowExpressionClauseSyntax) ? _methodNode.Parent : _methodNode))).ReturnType;
                var newTerm = _termFactory.Create(operation, ISlicerSymbol.Create((ITypeSymbol)type), false, TermFactory.GetFreshName(), false);
                _broker.HandleNonInstrumentedMethod(new List<Term>(), null, new List<Term>(), newTerm, type, "ctor");
                AssignRV(newTerm);
            }
            else
                _broker.DefUseOperation(_termFactory.Create(operation, ISlicerSymbol.CreateNullTypeSymbol(), false, null));

            ConsumeExitLoops();
            if (!_traceConsumer.HasNext() || _traceConsumer.ObserveNextStatement().TraceType != TraceType.EnterFinally)
            {
                _sliceCriteriaReached = _broker.SliceCriteriaReached;
                _broker.ExitMethod(_originalStatement, _returnObject, _returnExceptionTerm, null);
                _setReturn = true;
            }
            else
                _returnPostponed = true;
        }

        void VisitYieldReturnStatement(IReturnOperation operation)
        {
            if (yieldReturnValuesContainer == null)
                InitializeYieldReturnContainer((CSharpSyntaxNode)operation.Syntax);

            var recTerm = Visit(operation.ReturnedValue);
            // LAFHIS (TEMP CHECKING THIS) XXX
            //_broker.HandleNonInstrumentedMethod(new List<Term>() { recTerm }, yieldReturnValuesContainer, new List<Term>(), null, yieldReturnValuesContainer.First.Symbol.Symbol, "Add");
            _broker.HandleNonInstrumentedMethod(new List<Term>() { recTerm }, yieldReturnValuesContainer, new List<Term>(), null, yieldReturnValuesContainer.First.Symbol.Symbol, "Unknown");
        }

        void VisitYieldBreakStatement(IReturnOperation operation)
        {
            if (yieldReturnValuesContainer == null)
                InitializeYieldReturnContainer((CSharpSyntaxNode)operation.Syntax);

            AssignRV(yieldReturnValuesContainer);

            ConsumeExitLoops();
            if (!_traceConsumer.HasNext() || _traceConsumer.ObserveNextStatement().TraceType != TraceType.EnterFinally)
            {
                _sliceCriteriaReached = _broker.SliceCriteriaReached;
                _broker.ExitMethod(_originalStatement, _returnObject, _returnExceptionTerm, null);
                _setReturn = true;
            }
            else
                _returnPostponed = true;
        }
        #endregion

        #region Properties & fields
        Term VisitFieldReferenceExpression(IFieldReferenceOperation operation)
        {
            var recTerm = Visit(operation.Instance);
            Term newTerm;
            if (recTerm == null)
                newTerm = _termFactory.Create(operation, ISlicerSymbol.Create(operation.Type, _typeArguments), operation.Field.IsStatic || operation.Field.IsConst, Utils.GetRealName(operation.Field.ToString(), _typeArguments));
            else
            {
                newTerm = recTerm.AddingField(new Field(operation.Field.Name, ISlicerSymbol.Create(operation.Type, _typeArguments)));
                newTerm.Stmt = Utils.StmtFromSyntaxNode((CSharpSyntaxNode)operation.Syntax, _instrumentationResult);
            }

            //var Model = _semanticModelsContainer.GetBySyntaxNode((CSharpSyntaxNode)operation.Syntax);

            // XXX: Si no es asignación del lado izquierdo, o un struct, o ref, o un field constante entonces es wrappeado y espero el end
            //if (!((operation.Syntax.Parent is AssignmentExpressionSyntax &&
            //    ((AssignmentExpressionSyntax)operation.Syntax.Parent).Left == operation.Syntax &&
            //    ((AssignmentExpressionSyntax)operation.Syntax.Parent).OperatorToken.Kind() == SyntaxKind.EqualsToken) ||
            //    //((operation.Syntax.Parent is MemberAccessExpressionSyntax // Structs
            //    //    && (Model.GetTypeInfo(((MemberAccessExpressionSyntax)operation.Syntax.Parent).Expression).Type).CustomIsStruct())) ||

            //    // Esto es lo último que comenté
            //    //(((INamedTypeSymbol)operation.Field.ContainingSymbol).CustomIsStruct()) ||

            //        //&& operation.Instance != null && operation.Instance.Kind == OperationKind.InstanceReference) ||
            //    (operation.Syntax.Parent is ArgumentSyntax && ((ArgumentSyntax)operation.Syntax.Parent).RefOrOutKeyword.Value != null) ||
            //    (operation.Field.IsConst)))
            //    // XXX: ASUMIMOS QUE LOS FIELDS NO TIENEN CALLBACKS!
            //    if (Utils.IsEnterMethodOrConstructor(ObserveNextStatement().TraceType))
            //        throw new SlicerException("No se esperan callbacks de fields");
            //    else
            //        GetNextStatement(TraceType.EndMemberAccess);

            return newTerm;
        }

        Term VisitPropertyReferenceExpression(IPropertyReferenceOperation operation)
        {
            return VisitPropertyReferenceExpression(operation, false).Item2;
        }

        Tuple<Term, Term> VisitPropertyReferenceExpression(IPropertyReferenceOperation operation, bool forSet)
        {
            var recTerm = Visit(operation.Instance);

            Term newTerm;
            IDictionary<ITypeSymbol, ITypeSymbol> typesDictionary = null;

            // Special case, this refers to the parent
            if (operation.Parent is IPropertySubpatternOperation propertySubpatternOperation && recTerm.ToString() == "this" &&
                propertySubpatternOperation.Member == operation)
                recTerm = PatternOperationReceiver;

            bool isSetAccessor;
            var hasGetAccesor = (!forSet) && HasAccesor(operation.Property.Name, typesDictionary, out isSetAccessor) && !isSetAccessor;

            if (recTerm == null)
            {
                newTerm = _termFactory.Create(operation, ISlicerSymbol.Create(operation.Type, _typeArguments), operation.Property.IsStatic,
                    hasGetAccesor ? TermFactory.GetFreshName() : Utils.GetRealName(operation.Property.ToString(), _typeArguments));
                typesDictionary = Utils.GetTypesDictionary(operation.Property.ContainingType, _typeArguments);
            }
            // Si el recTerm es escalar estamos accediendo a cosas como string.length, cosa.IsNull, cuyo "last def" es equivalente al anterior,
            // Por lo tanto no cambia el slice devolver el término anterior.
            else if (recTerm.IsScalar)
                newTerm = recTerm;
            else
            {
                if (hasGetAccesor)
                    newTerm = _termFactory.Create(operation, ISlicerSymbol.Create(operation.Type, _typeArguments), operation.Property.IsStatic, TermFactory.GetFreshName());
                else
                {
                    newTerm = recTerm.AddingField(new Field(hasGetAccesor ? TermFactory.GetFreshName() : operation.Property.Name, ISlicerSymbol.Create(operation.Type)));
                    newTerm.Stmt = Utils.StmtFromSyntaxNode((CSharpSyntaxNode)operation.Syntax, _instrumentationResult);
                }
            }

            if (hasGetAccesor)
                HandleInstrumentedMethod(operation, newTerm, null, operation.Property.IsStatic ? null : recTerm, typesDictionary);

            var Model = _semanticModelsContainer.GetBySyntaxNode((CSharpSyntaxNode)operation.Syntax);
            var isException = structsExceptions.Any(x =>
                ((_originalStatement != null && x.Item1 == _originalStatement.FileId) || (_originalStatement == null && x.Item1 == _currentFileId)) &&
                x.Item2 < operation.Syntax.Span.Start && x.Item3 > operation.Syntax.Span.End);
            if (isException)
                ;

            if ((!forSet) && !((operation.Syntax.Parent is AssignmentExpressionSyntax &&
                ((AssignmentExpressionSyntax)operation.Syntax.Parent).Left == operation.Syntax &&
                ((AssignmentExpressionSyntax)operation.Syntax.Parent).OperatorToken.Kind() == SyntaxKind.EqualsToken) ||
                (!Globals.wrap_structs_calls && (
                operation.Property.GetMethod.ReceiverType.CustomIsStruct() && !isException)) ||

                (operation.Syntax.Parent is ArgumentSyntax && ((ArgumentSyntax)operation.Syntax.Parent).RefOrOutKeyword.Value != null)))
            {
                var nextStmt = ObserveNextStatement();
                if (newTerm.Count == 1)
                    GetNextStatement(TraceType.EndMemberAccess, false);
                else
                {
                    var returnTerms = WaitForEnd(newTerm.Stmt.CSharpSyntaxNode, new List<Term>() { newTerm }, operation.Property.GetMethod, null, TraceType.EndMemberAccess);

                    if (!Globals.properties_as_fields || returnTerms.Count > 0)
                        _broker.HandleNonInstrumentedMethod(new List<Term>(), newTerm.IsGlobal ? null : newTerm.DiscardLast(), returnTerms, newTerm, operation.Property.GetMethod);
                }
            }

            return new Tuple<Term, Term>(recTerm, newTerm);
        }

        Term VisitIndexedPropertyReferenceExpression(IPropertyReferenceOperation operation)
        {
            return VisitIndexedPropertyReferenceExpression(operation, false).Item1;
        }

        Tuple<Term, List<Term>> VisitIndexedPropertyReferenceExpression(IPropertyReferenceOperation operation, bool forSet)
        {
            // Lista
            var recTerm = Visit(operation.Instance);
            Term newTerm;

            // Si el operador está overraideado... (lo hago solo para un indexer de un argumento)
            var nextStmt = _traceConsumer.ObserveNextStatement();

            if (nextStmt.TraceType == TraceType.EnterMethod &&
                nextStmt.CSharpSyntaxNode is AccessorDeclarationSyntax &&
                ((AccessorDeclarationSyntax)nextStmt.CSharpSyntaxNode).Parent.Parent is IndexerDeclarationSyntax &&
                operation.Arguments.Count() == 1)
            {
                bool isSetAccessor;
                var hasGetAccesor = HasAccesor(operation.Property.Name, null, out isSetAccessor, true) && !isSetAccessor;

                var accesorSymbol = (IMethodSymbol)_semanticModelsContainer.GetBySyntaxNode(nextStmt.CSharpSyntaxNode).GetDeclaredSymbol(nextStmt.CSharpSyntaxNode);
                newTerm = _termFactory.Create(operation, ISlicerSymbol.Create(accesorSymbol.ReturnType, _typeArguments), false, TermFactory.GetFreshName());
                if (hasGetAccesor)
                {
                    var argumentos = new List<Term>();
                    foreach (var arg in operation.Arguments)
                    {
                        var argTerm = CreateArgument(arg);
                        argumentos.Add(argTerm);
                    }

                    var dictionary = Utils.GetTypesDictionary(accesorSymbol, _typeArguments);
                    HandleInstrumentedMethod(operation, newTerm, argumentos, accesorSymbol.IsStatic ? null : recTerm, dictionary);
                    // TODO: Si el Indexer está overraideado te puede hacer callback...
                    GetNextStatement(TraceType.EndMemberAccess, false); // TODO: False dentro de Roslyn...
                }
            }
            else
            {
                // Si se accede a un caracter de un string mediante [], ejemplo "hola"[0], pincha. Hay que devolver el literal.
                if (recTerm.IsScalar)
                {
                    newTerm = _termFactory.Create(operation, ISlicerSymbol.Create(operation.Type), false, TermFactory.GetFreshName());
                    _broker.DefUseOperation(newTerm,
                        operation.Arguments.Select(x => Visit(x)).Union(new Term[] { recTerm }).ToArray());
                    // TODO: False only with structs... (and not exceptions)
                    GetNextStatement(TraceType.EndMemberAccess, false);
                }
                else
                {
                    // No sabemos que hay adentro del indexer. Solo podemos utilizarlo y hacer una especie de Havoc readonly que apunte a todo y utilize el indexer.
                    // Una mejora es: si es string, entonces podemos crear el termino recTerm."Property"

                    var dependentTerms = new List<Term>();
                    if (operation.Arguments.Count() == 1 &&
                        operation.Arguments.Single().Value.Kind == OperationKind.Literal &&
                        operation.Arguments.Single().Value.Type.Name == "String")
                    {
                        newTerm = recTerm.AddingField(new Field(((ILiteralOperation)operation.Arguments.Single().Value).ConstantValue.Value.ToString(), ISlicerSymbol.Create(operation.Type)));
                        newTerm.Stmt = Utils.StmtFromSyntaxNode((CSharpSyntaxNode)operation.Syntax, _instrumentationResult);
                    }
                    else
                    {
                        // Términos dependientes
                        dependentTerms = operation.Arguments.Select(x => Visit(x)).ToList();
                        if (forSet)
                            return new Tuple<Term, List<Term>>(recTerm, dependentTerms);
                        else
                        {
                            newTerm = _termFactory.Create(operation, ISlicerSymbol.Create(operation.Type, _typeArguments));
                            _broker.HandleNonInstrumentedMethod(dependentTerms, recTerm, new List<Term>(), newTerm, operation.Property.GetMethod);
                        }
                    }

                    var isException = structsExceptions.Any(x =>
                        ((_originalStatement != null && x.Item1 == _originalStatement.FileId) || (_originalStatement == null && x.Item1 == _currentFileId)) &&
                        x.Item2 < operation.Syntax.Span.Start && x.Item3 > operation.Syntax.Span.End);
                    if (Globals.wrap_structs_calls || isException || !(operation.Property.GetMethod?.ReceiverType.CustomIsStruct() ?? false))
                    {
                        if (ObserveNextStatement().TraceType != TraceType.EndMemberAccess)
                        {
                            var returnTerms = WaitForEnd(recTerm.Stmt.CSharpSyntaxNode, new List<Term>() { recTerm }, operation.Property, null, TraceType.EndMemberAccess);
                            _broker.HandleNonInstrumentedMethod(dependentTerms, recTerm, returnTerms, newTerm, operation.Property);
                        }
                        else
                            GetNextStatement(TraceType.EndMemberAccess);
                    }
                }
            }
            return new Tuple<Term, List<Term>>(newTerm, null);
        }

        Term VisitConditionalAccessOperation(IConditionalAccessOperation operation)
        {
            var recTerm = Visit(operation.Operation);
            if (ObserveNextStatement(false, _typeArguments).TraceType == TraceType.ConditionalAccessIsNull)
            {
                GetNextStatement(TraceType.ConditionalAccessIsNull, true);
                var slicerSymbol = ISlicerSymbol.CreateNullTypeSymbol();
                var newTerm = _termFactory.Create(operation.Operation, slicerSymbol, false, TermFactory.GetFreshName(), true);
                _broker.DefUseOperation(newTerm, new Term[] { recTerm });
                // We have to consume one extra trace (with the instrumentation the client sends one more trace line)
                if (operation.WhenNotNull.Kind != OperationKind.FieldReference)
                {
                    var tempStmt = ObserveNextStatement();
                    if ((tempStmt.TraceType == TraceType.EndInvocation || tempStmt.TraceType == TraceType.EndMemberAccess) &&
                        tempStmt.SpanEnd == operation.Syntax.Span.End)
                        GetNextStatement();
                }
                return newTerm;
            }

            temporaryConditionalAccessTerm = recTerm;
            var whenNotNull = Visit(operation.WhenNotNull);
            return whenNotNull;
        }

        Term VisitConditionalAccessInstance(IConditionalAccessInstanceOperation operation)
        {
            var returnTerm = temporaryConditionalAccessTerm;
            temporaryConditionalAccessTerm = null;
            return returnTerm;
        }

        Term VisitEventReferenceExpression(IEventReferenceOperation operation)
        {
            var recTerm = Visit(operation.Instance);
            Term returnTerm = null;
            if (recTerm == null)
                returnTerm = _termFactory.Create(operation, ISlicerSymbol.Create(operation.Type, _typeArguments), operation.Event.IsStatic, Utils.GetRealName(operation.Event.ToString(), _typeArguments));
            else
            {
                returnTerm = recTerm.AddingField(operation.Member.Name, ISlicerSymbol.Create(operation.Type));
                returnTerm.Stmt = Utils.StmtFromSyntaxNode((CSharpSyntaxNode)operation.Syntax, _instrumentationResult);
            }
            var nextTrace = _traceConsumer.ObserveNextStatement();
            if (nextTrace.TraceType == TraceType.EndMemberAccess && nextTrace.SpanStart == operation.Syntax.Span.Start && nextTrace.SpanEnd == operation.Syntax.Span.End)
                _traceConsumer.GetNextStatement();
            return returnTerm;
        }

        Term VisitDynamicMemberReference(IDynamicMemberReferenceOperation operation)
        {
            return VisitDynamicMemberReference(operation, false);
        }

        Term VisitDynamicMemberReference(IDynamicMemberReferenceOperation operation, bool forSet)
        {
            var recTerm = Visit(operation.Instance);
            Term newTerm;
            IDictionary<ITypeSymbol, ITypeSymbol> typesDictionary = null;

            bool isSetAccessor;
            var hasGetAccesor = (!forSet) && HasAccesor(operation.MemberName, typesDictionary, out isSetAccessor) && !isSetAccessor;

            // TODO: Chequear que sucede con los dynamic types

            if (recTerm == null)
            {
                newTerm = _termFactory.Create(operation, ISlicerSymbol.Create(operation.Type, _typeArguments), operation.Type.IsStatic,
                    hasGetAccesor ? TermFactory.GetFreshName() : Utils.GetRealName(operation.MemberName, _typeArguments));
                typesDictionary = Utils.GetTypesDictionary((INamedTypeSymbol)operation.Type, _typeArguments);
            }
            // Si el recTerm es escalar estamos accediendo a cosas como string.length, cosa.IsNull, cuyo "last def" es equivalente al anterior,
            // Por lo tanto no cambia el slice devolver el término anterior.
            else if (recTerm.IsScalar)
                newTerm = recTerm;
            else
            {
                newTerm = recTerm.AddingField(new Field(hasGetAccesor ? TermFactory.GetFreshName() : operation.MemberName, ISlicerSymbol.Create(operation.Type)));
                newTerm.Stmt = Utils.StmtFromSyntaxNode((CSharpSyntaxNode)operation.Syntax, _instrumentationResult);
            }

            if (hasGetAccesor)
                HandleInstrumentedMethod(operation, newTerm, null, operation.Type.IsStatic ? null : recTerm, typesDictionary);

            var Model = _semanticModelsContainer.GetBySyntaxNode((CSharpSyntaxNode)operation.Syntax);
            if ((!forSet) && !((operation.Syntax.Parent is AssignmentExpressionSyntax &&
                ((AssignmentExpressionSyntax)operation.Syntax.Parent).Left == operation.Syntax &&
                ((AssignmentExpressionSyntax)operation.Syntax.Parent).OperatorToken.Kind() == SyntaxKind.EqualsToken) ||
                //(operation.Syntax.Parent is MemberAccessExpressionSyntax // Structs
                //&& (Model.GetTypeInfo(((MemberAccessExpressionSyntax)operation.Syntax.Parent).Expression).Type).CustomIsStruct()) ||
                (operation.Syntax.Parent is ArgumentSyntax && ((ArgumentSyntax)operation.Syntax.Parent).RefOrOutKeyword.Value != null)))
            {
                var nextStmt = ObserveNextStatement();
                if (newTerm.Count == 1)
                    GetNextStatement(TraceType.EndMemberAccess);
                else
                {
                    var memberSymbol = _semanticModelsContainer.GetBySyntaxNode((CSharpSyntaxNode)operation.Syntax).GetSymbolInfo(operation.Syntax).Symbol;
                    var returnTerms = WaitForEnd(newTerm.Stmt.CSharpSyntaxNode, new List<Term>() { newTerm }, memberSymbol, null, TraceType.EndMemberAccess);
                    // XXX: Si hubo callbacks con cosas aplicamos una operación sino simplemente retornamos el término
                    if (returnTerms.Count > 0)
                        _broker.HandleNonInstrumentedMethod(new List<Term>(), newTerm.IsGlobal ? null : newTerm.DiscardLast(), returnTerms, newTerm, operation.Type);
                }
            }

            return newTerm;
        }

        Term VisitArgument(IArgumentOperation argument)
        {
            var term = Visit(argument.Value);
            if (argument.Parameter.RefKind != RefKind.None)
            {
                term.IsRef = true;
                term.IsOutOrRef = true;
            }
            return term;
        }
        #endregion

        #region Local variables or base calls
        Term VisitInstanceReferenceExpression(IInstanceReferenceOperation operation)
        {
            return _termFactory.Create(operation, ISlicerSymbol.Create(operation.Type), false, "this", false);
        }

        Term VisitParameterReferenceExpression(IParameterReferenceOperation operation)
        {
            return _termFactory.Create(operation, ISlicerSymbol.Create(operation.Parameter.Type, _typeArguments), false, operation.Parameter.Name, false);
        }

        Term VisitLocalReferenceExpression(ILocalReferenceOperation operation)
        {
            return _termFactory.Create(operation, ISlicerSymbol.Create(operation.Local.Type, _typeArguments), false, operation.Local.Name, false);
        }

        Term VisitInterpolatedStringOperation(IInterpolatedStringOperation operation)
        {
            var uses = new List<Term>();
            foreach (var part in operation.Parts)
            {
                var use = Visit(part);
                if (use != null)
                    uses.Add(use);
            }

            var slicerSymbol = operation.Type == null ? ISlicerSymbol.CreateNullTypeSymbol() : ISlicerSymbol.Create(operation.Type);
            var newTerm = _termFactory.Create(operation, slicerSymbol, false, TermFactory.GetFreshName());
            _broker.DefUseOperation(newTerm, uses.ToArray());
            return newTerm;
        }

        Term VisitLiteralExpression(ILiteralOperation literalExpression)
        {
            var slicerSymbol = literalExpression.Type == null ? ISlicerSymbol.CreateNullTypeSymbol() : ISlicerSymbol.Create(literalExpression.Type);
            var newTerm = _termFactory.Create(literalExpression, slicerSymbol, false, TermFactory.GetFreshName());
            _broker.DefUseOperation(newTerm);
            return newTerm;
        }

        Term VisitSizeOfOperation(ISizeOfOperation operation)
        {
            var symbol = ISlicerSymbol.Create(operation.Type);
            var newTerm = _termFactory.Create(operation, symbol, false, TermFactory.GetFreshName());
            _broker.DefUseOperation(newTerm);
            return newTerm;
        }
        #endregion

        #region Visits Aux
        void DealWithPatterns(Term lastTerm, IPatternOperation pattern, List<Term> uses)
        {
            if (pattern is null)
                return;

            uses.Add(lastTerm);
            if (pattern is IDeclarationPatternOperation)
            {
                if (!(((IDeclarationPatternOperation)pattern).DeclaredSymbol is null))
                {
                    var matchedType = pattern.NarrowedType;
                    var varName = ((IDeclarationPatternOperation)pattern).DeclaredSymbol.Name;
                    var newTerm = _termFactory.Create((CSharpSyntaxNode)pattern.Syntax, ISlicerSymbol.Create(matchedType, _typeArguments), false, varName, false, false);
                    _broker.Assign(newTerm, lastTerm);
                }
            }
            else if (pattern is IRecursivePatternOperation)
            {
                var tempReceiver = PatternOperationReceiver;
                PatternOperationReceiver = lastTerm;
                foreach (var propSupPat in ((IRecursivePatternOperation)pattern).PropertySubpatterns)
                {
                    var member = Visit(propSupPat.Member);
                    DealWithPatterns(member, propSupPat.Pattern, uses);
                }
                PatternOperationReceiver = tempReceiver;

                if (((IRecursivePatternOperation)pattern).DeclaredSymbol != null)
                {
                    var declaredVariable = _termFactory.Create(pattern, ISlicerSymbol.Create(((IRecursivePatternOperation)pattern).NarrowedType), false, ((IRecursivePatternOperation)pattern).DeclaredSymbol.Name, false, false);
                    _broker.Assign(declaredVariable, lastTerm, new List<Term>() { /*newTerm*/ });
                }
            }
        }
        #endregion
    }
}
