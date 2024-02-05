using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;
using System.Collections.Generic;
using System.Linq;
using System;

namespace DynAbs
{
    class UsesVisitor
    {
        ISemanticModelsContainer _semanticModelsContainer;
        InstrumentationResult _instrumentationResult;
        TermFactory _termFactory;

        public UsesVisitor(ISemanticModelsContainer semanticModelsContainer, InstrumentationResult instrumentationResult, TermFactory termFactory)
        {
            _semanticModelsContainer = semanticModelsContainer;
            _instrumentationResult = instrumentationResult;
            _termFactory = termFactory;
        }

        public List<Term> Visit(IOperation operation)
        {
            if (operation == null)
                return new List<Term>();
            if (operation.Kind == OperationKind.IsType)
                return Visit(((IIsTypeOperation)operation).ValueOperand);
            if (operation.Kind == OperationKind.ExpressionStatement)
                return Visit(((IExpressionStatementOperation)operation).Operation);
            if (operation.Kind == OperationKind.Conversion)
                return Visit(((IConversionOperation)operation).Operand);
            if (operation.Kind == OperationKind.SimpleAssignment)
                VisitAssignmentExpression((IAssignmentOperation)operation);
            if (operation.Kind == OperationKind.Increment)
                return VisitIncrementExpression((IIncrementOrDecrementOperation)operation);
            if (operation.Kind == OperationKind.UnaryOperator)
                return VisitUnaryOperatorExpression((IUnaryOperation)operation);
            if (operation.Kind == OperationKind.BinaryOperator)
                return VisitBinaryOperatorExpression((IBinaryOperation)operation);
            if (operation.Kind == OperationKind.Conditional)
            { 
                if (((IConditionalOperation)operation).WhenTrue.Kind == OperationKind.Block)
                    VisitIfStatement((IConditionalOperation)operation);
                else
                    return VisitConditionalChoiceExpression((IConditionalOperation)operation);
            }
            if (operation.Kind == OperationKind.Loop && ((ILoopOperation)operation).LoopKind != LoopKind.ForEach)
                VisitForWhileUntilLoopStatement((IOperation)operation);
            if (operation.Kind == OperationKind.Loop && ((ILoopOperation)operation).LoopKind == LoopKind.ForEach)
                VisitForEachLoopStatement((IForEachLoopOperation)operation);
            if (operation.Kind == OperationKind.Switch)
                VisitSwitchStatement((ISwitchOperation)operation);
            if (operation.Kind == OperationKind.VariableDeclarationGroup)
                return VisitVariableDeclarationStatement((IVariableDeclarationGroupOperation)operation);
            if (operation.Kind == OperationKind.VariableDeclaration)
                return VisitVariableDeclaration((IVariableDeclarationOperation)operation);
            if (operation.Kind == OperationKind.VariableDeclarator)
                return VisitVariableDeclarator((IVariableDeclaratorOperation)operation);
            if (operation.Kind == OperationKind.ArrayCreation)
                return VisitArrayCreationExpression((IArrayCreationOperation)operation);
            if (operation.Kind == OperationKind.AnonymousObjectCreation)
                return VisitAnonymousObjectCreationExpressionSyntax((IAnonymousObjectCreationOperation)operation);
            if (operation.Kind == OperationKind.None && operation.Syntax is InitializerExpressionSyntax)
                return VisitInitializerExpressionSyntax((InitializerExpressionSyntax)operation.Syntax);
            if (operation.Kind == OperationKind.None && operation.Syntax is ElementAccessExpressionSyntax)
                return VisitElementAccessExpressionSyntax((ElementAccessExpressionSyntax)operation.Syntax);
            if (operation.Kind == OperationKind.Coalesce)
                return VisitNullCoalescingExpression((ICoalesceOperation)operation);
            if (operation.Kind == OperationKind.Return)
                return VisitReturnStatement((IReturnOperation)operation);
            if (operation.Kind == OperationKind.Invocation)
                return VisitInvocationExpression((IInvocationOperation)operation);
            if (operation.Kind == OperationKind.ObjectCreation)
                return VisitObjectCreationExpression((IObjectCreationOperation)operation);
            if (operation.Kind == OperationKind.LocalReference)
                return VisitLocalReferenceExpression((ILocalReferenceOperation)operation);
            if (operation.Kind == OperationKind.FieldReference)
                return VisitFieldReferenceExpression((IFieldReferenceOperation)operation);
            if (operation.Kind == OperationKind.PropertyReference)
                return VisitPropertyReferenceExpression((IPropertyReferenceOperation)operation);
            if (operation.Kind == OperationKind.ParameterReference)
                return VisitParameterReferenceExpression((IParameterReferenceOperation)operation);
            if (operation.Kind == OperationKind.ArrayElementReference)
                return VisitArrayElementReferenceExpression((IArrayElementReferenceOperation)operation);
            if (operation.Kind == OperationKind.InstanceReference)
                return VisitInstanceReferenceExpression((IInstanceReferenceOperation)operation);
            if (operation.Kind == OperationKind.None && operation.Syntax is QueryExpressionSyntax)
                return VisitQueryExpression((QueryExpressionSyntax)operation.Syntax);
            if (operation.Kind == OperationKind.Literal)
                return VisitLiteralExpression((ILiteralOperation)operation);
            if (operation is IBlockOperation blockOperation)
                return blockOperation.Operations.SelectMany(x => Visit(x)).ToList();
            return new List<Term>();
        }

        List<Term> VisitVariableDeclarationStatement(IVariableDeclarationGroupOperation operation)
        {
            var result = new List<Term>();
            foreach (var v in operation.Declarations)
                result.AddRange(Visit(v));
            return result;
        }

        List<Term> VisitVariableDeclaration(IVariableDeclarationOperation operation)
        {
            var result = new List<Term>();
            foreach (var d in operation.Declarators)
                result.AddRange(Visit(d));
            return result;
        }

        List<Term> VisitVariableDeclarator(IVariableDeclaratorOperation operation)
        {
            return Visit(operation.Initializer);
        }

        List<Term> VisitAssignmentExpression(IAssignmentOperation operation)
        {
            var result = new List<Term>();
            result.AddRange(Visit(operation.Target));
            result.AddRange(Visit(operation.Value));
            return result;
        }

        List<Term> VisitIncrementExpression(IIncrementOrDecrementOperation operation)
        {
            return Visit(operation.Target);
        }

        List<Term> VisitUnaryOperatorExpression(IUnaryOperation operation)
        {
            return Visit(operation.Operand);
        }

        List<Term> VisitBinaryOperatorExpression(IBinaryOperation operation)
        {
            var result = new List<Term>();
            result.AddRange(Visit(operation.LeftOperand));
            result.AddRange(Visit(operation.RightOperand));
            return result;
        }

        List<Term> VisitIfStatement(IConditionalOperation operation)
        {
            return Visit(operation.Condition);
        }

        List<Term> VisitConditionalChoiceExpression(IConditionalOperation operation)
        {
            var result = new List<Term>();
            result.AddRange(Visit(operation.Condition));
            result.AddRange(Visit(operation.WhenTrue));
            result.AddRange(Visit(operation.WhenFalse));
            return result;
        }

        List<Term> VisitForWhileUntilLoopStatement(IOperation operation)
        {
            throw new NotImplementedException();
        }

        List<Term> VisitForEachLoopStatement(IForEachLoopOperation operation)
        {
            var result = new List<Term>();

            result.AddRange(Visit(operation.Collection));
            result.AddRange(Visit(operation.Body));
            
            return result;
        }

        List<Term> VisitSwitchStatement(ISwitchOperation operation)
        {
            return Visit(operation.Value);
        }

        List<Term> VisitArrayCreationExpression(IArrayCreationOperation operation)
        {
            var dependentTerms = new List<Term>();

            var previousExpressions = new List<IOperation>();

            if (operation.Syntax is ArrayCreationExpressionSyntax && ((ArrayCreationExpressionSyntax)operation.Syntax).Initializer != null)
                previousExpressions = ((ArrayCreationExpressionSyntax)operation.Syntax).Initializer.Expressions.Select(x => GetOperation(x)).ToList();
            else if (operation.Syntax is ImplicitArrayCreationExpressionSyntax && ((ImplicitArrayCreationExpressionSyntax)operation.Syntax).Initializer != null)
                previousExpressions = ((ImplicitArrayCreationExpressionSyntax)operation.Syntax).Initializer.Expressions.Select(x => GetOperation(x)).ToList();
            // Note: arrayCreationExpression.ElementValues.ArrayClass = Dimension
            // It is used when we have params[]
            else if (operation.Initializer != null && operation.Initializer.ElementValues != null)
                operation.Initializer.ElementValues.ToList().ForEach(elementValue => previousExpressions.Add(elementValue));
            previousExpressions.ToList().ForEach(x => dependentTerms.AddRange(Visit(x)));
            return dependentTerms;
        }

        List<Term> VisitAnonymousObjectCreationExpressionSyntax(IAnonymousObjectCreationOperation operation)
        {
            var dependentTerms = new List<Term>();
            foreach (var init in operation.Initializers)
                dependentTerms.AddRange(Visit(init));
            return dependentTerms;
        }

        List<Term> VisitInitializerExpressionSyntax(InitializerExpressionSyntax operation)
        {
            var dependentTerms = new List<Term>();
            if (operation.Expressions != null)
                operation.Expressions.ToList().ForEach(x =>
                    dependentTerms.AddRange(Visit(_semanticModelsContainer.GetBySyntaxNode(x).GetOperation(x))));
            return dependentTerms;
        }

        List<Term> VisitElementAccessExpressionSyntax(ElementAccessExpressionSyntax operation)
        {
            var p_semanticModel = _semanticModelsContainer.GetBySyntaxNode(operation.Expression);
            var p_operation = p_semanticModel.GetOperation(operation.Expression);
            var recTerm = Visit(p_operation);

            var dependentTerms = new List<Term>();
            dependentTerms.AddRange(recTerm);
            operation.ArgumentList.Arguments.ToList().ForEach(x => dependentTerms.AddRange(
                Visit(p_semanticModel.GetOperation(x.Expression))));
            return dependentTerms;
        }

        List<Term> VisitNullCoalescingExpression(ICoalesceOperation operation)
        {
            var result = new List<Term>();
            result.AddRange(Visit(operation.Value));
            result.AddRange(Visit(operation.WhenNull));
            return result;
        }

        List<Term> VisitReturnStatement(IReturnOperation operation)
        {
            return Visit(((IReturnOperation)operation).ReturnedValue);
        }

        List<Term> VisitInvocationExpression(IInvocationOperation operation)
        {
            var result = new List<Term>();
            result.AddRange(Visit(operation.Instance));
            result.AddRange(operation.Arguments
                                 .Select(x => Visit(x.Value))
                                 .SelectMany(x => x));
            return result;
        }

        List<Term> VisitObjectCreationExpression(IObjectCreationOperation operation)
        {
            var dependentTerms = new List<Term>();
            if (((ObjectCreationExpressionSyntax)operation.Syntax).Initializer != null)
                ((ObjectCreationExpressionSyntax)operation.Syntax).Initializer.Expressions
                    .Where(x => !(x is AssignmentExpressionSyntax)).ToList()
                    .ForEach(x => dependentTerms.AddRange(Visit(_semanticModelsContainer.GetBySyntaxNode(x).GetOperation(x))));
            
            dependentTerms.AddRange(operation.Arguments.Select(x => Visit(x.Value)).SelectMany(x => x));
            
            if (((ObjectCreationExpressionSyntax)operation.Syntax).Initializer != null)
            {
                var expressions = ((ObjectCreationExpressionSyntax)operation.Syntax).Initializer.Expressions.Where(x => x is AssignmentExpressionSyntax);
                foreach (var expression in expressions)
                {
                    var ioperation_expression = _semanticModelsContainer.GetBySyntaxNode(expression).GetOperation(expression);
                    dependentTerms.AddRange(Visit(((IAssignmentOperation)ioperation_expression).Value));
                }
            }
            return dependentTerms;
        }

        List<Term> VisitLocalReferenceExpression(ILocalReferenceOperation operation)
        {
            return new List<Term>() { _termFactory.Create(operation, ISlicerSymbol.Create(operation.Local.Type), false, operation.Local.Name) };
        }

        List<Term> VisitFieldReferenceExpression(IFieldReferenceOperation operation)
        {
            Term newTerm = null;
            List<Term> recTerm = Visit(operation.Instance);
            if (operation.Instance == null || (recTerm == null || recTerm.Count == 0))
                newTerm = _termFactory.Create(operation, ISlicerSymbol.Create(operation.Field.Type), operation.Field.IsStatic || operation.Field.IsConst, operation.Field.ToString());
            else
            {
                newTerm = recTerm.First().AddingField(operation.Field.Name, ISlicerSymbol.Create(operation.Field.Type));
                newTerm.Stmt = Utils.StmtFromSyntaxNode((CSharpSyntaxNode)operation.Syntax, _instrumentationResult);
            }
            return new List<Term>() { newTerm };
        }

        List<Term> VisitPropertyReferenceExpression(IPropertyReferenceOperation operation)
        {
            var recTerm = Visit(operation.Instance);
            Term newTerm = null;
           
            if (recTerm == null || recTerm.Count == 0)
            {
                // Hay un caso particular feo. Lo que hay detrás es una variable libre declarada en la query! pero el visit da null.. lpm
                if (operation.Instance != null && operation.Instance.Kind == OperationKind.None && operation.Instance.Syntax is IdentifierNameSyntax)
                    newTerm = _termFactory.Create(operation, ISlicerSymbol.Create(operation.Type), operation.Property.IsStatic, 
                        ((IdentifierNameSyntax)operation.Instance.Syntax).Identifier.ValueText + "." + operation.Property.Name);
                else if (!operation.Property.IsIndexer) // Si es v[0] y v pasado por parámetro el nombre de la property es this // XXX VER BIEN PORQUE TENDRÍA QUE AGREGARSE "v"
                    newTerm = _termFactory.Create(operation, ISlicerSymbol.Create(operation.Type), operation.Property.IsStatic, operation.Property.Name);
            }
            else if (recTerm.First().IsScalar)
                newTerm = recTerm.First();
            else
            {
                newTerm = recTerm.First().AddingField(new Field(operation.Property.Name, ISlicerSymbol.Create(operation.Type)));
                newTerm.Stmt = Utils.StmtFromSyntaxNode((CSharpSyntaxNode)operation.Syntax, _instrumentationResult);
            }
            
            if (newTerm != null)
                return new List<Term>() { newTerm };
            return new List<Term>();
        }

        List<Term> VisitParameterReferenceExpression(IParameterReferenceOperation operation)
        {
            if (operation.Parameter.Type.IsAnonymousType)
                return new List<Term>() { };
            return new List<Term>() { _termFactory.Create(operation, ISlicerSymbol.Create(operation.Parameter.Type), false, operation.Parameter.Name) };
        }

        List<Term> VisitArrayElementReferenceExpression(IArrayElementReferenceOperation operation)
        {
            var result = new List<Term>();
            result.AddRange(Visit(operation.ArrayReference));
            operation.Indices.ToList().ForEach(x => result.AddRange(Visit(x)));
            return result;
        }

        List<Term> VisitInstanceReferenceExpression(IInstanceReferenceOperation operation)
        {
            return new List<Term>() { _termFactory.Create(operation, ISlicerSymbol.Create(operation.Type), false, "this") };
        }

        List<Term> VisitQueryExpression(QueryExpressionSyntax operation)
        {
            // Las variables que se definen dentro de la query (los elementos de las listas)
            // Sirve para filtrar las variables externas de las internas
            var listIdentifiers = new List<string>();
            // Las variables que vamos a ligar a la operation
            var dependentTerms = new HashSet<Term>();
            // Instanciamos el visitor que no consume traza
            //var usesVisitor = new UsesVisitor(_semanticModelsContainer, _instrumentationResult, _termFactory);

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

            // 1. Arrancamos obteniendo las variables declaradas en el FROM (identifiers)
            listIdentifiers.Add(operation.FromClause.Identifier.Text);

            // 2. Luego obtenemos nuestras variables externas
            Visit(GetOperation(operation.FromClause.Expression))
                .ToList()
                .ForEach(x => dependentTerms.Add(x));

            // 3. De las cláusulas obtenemos los términos que importan
            foreach (var clause in operation.Body.Clauses)
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

                    Visit(GetOperation(((JoinClauseSyntax)clause).InExpression))
                               .ToList()
                               .ForEach(x => dependentTerms.Add(x));

                    Visit(GetOperation(((JoinClauseSyntax)clause).LeftExpression))
                               .Where(x => !listIdentifiers.Contains(x.First.ToString()))
                               .ToList()
                               .ForEach(x => dependentTerms.Add(x));

                    Visit(GetOperation(((JoinClauseSyntax)clause).RightExpression))
                               .Where(x => !listIdentifiers.Contains(x.First.ToString()))
                               .ToList()
                               .ForEach(x => dependentTerms.Add(x));
                }
                if (clause is WhereClauseSyntax)
                {
                    // EJEMPLO DE WHERE: where otraCasa.cantVentanas < variableAuxiliar3
                    // Lo único que importa es la expresión

                    Visit(GetOperation(((WhereClauseSyntax)clause).Condition))
                               .Where(x => !listIdentifiers.Contains(x.First.ToString()))
                               .ToList()
                               .ForEach(x => dependentTerms.Add(x));
                }
            }

            // 2. Obtenemos del body las variables utilizadas
            // TODO: FALTA EL GROUPCLAUSESYNTAX
            if (operation.Body.SelectOrGroup is SelectClauseSyntax)
                Visit(GetOperation(((SelectClauseSyntax)operation.Body.SelectOrGroup).Expression))
                    .Where(x => !listIdentifiers.Contains(x.First.ToString()))
                    .ToList()
                    .ForEach(x => dependentTerms.Add(x));

            return dependentTerms.ToList();
        }

        List<Term> VisitLiteralExpression(ILiteralOperation literalExpression)
        {
            return new List<Term>() { };
        }

        IOperation GetOperation(CSharpSyntaxNode syntaxNode)
        {
            var p_semanticModel = _semanticModelsContainer.GetBySyntaxNode(syntaxNode);
            var p_operation = p_semanticModel.GetOperation(syntaxNode);
            return p_operation;
        }
    }
}
