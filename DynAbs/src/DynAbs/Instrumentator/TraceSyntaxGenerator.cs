using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynAbs
{
    static class TraceSyntaxGenerator
    {
        public static string TraceType = "TraceSender";
        public static string NamespaceName = "DynAbs.Tracing.";

        public static StatementSyntax SimpleStatement(SyntaxNode originalNode, int sourceFileId)
        {
            var traceStr = string.Format("{4}{3}.TraceSimpleStatement({0},{1},{2});", sourceFileId, originalNode.Span.Start, originalNode.Span.End, TraceType, NamespaceName);
            return SyntaxFactory.ParseStatement(traceStr).WithTrailingTrivia(SyntaxFactory.Whitespace("\n"));
        }

        public static ExpressionSyntax SimpleExpression(SyntaxNode originalNode, int sourceFileId)
        {
            var traceStr = string.Format("{4}{3}.TraceSimpleStatement({0},{1},{2});", sourceFileId, originalNode.Span.Start, originalNode.Span.End, TraceType, NamespaceName);
            return SyntaxFactory.ParseExpression(traceStr).WithTrailingTrivia(SyntaxFactory.Whitespace("\n"));
        }

        public static ExpressionSyntax ConditionExpression(SyntaxNode originalNode, SyntaxNode condition, int sourceFileId)
        {
            // "|| true" is for telling to the compiler that the left expression will always be true
            var traceStr = string.Format("({5}{4}.TraceSimpleStatement({0},{1},{2}) || true) && ({3}) ;", sourceFileId, originalNode.Span.Start, originalNode.Span.End, condition == null ? "true" : condition.ToString(), TraceType, NamespaceName);
            return SyntaxFactory.ParseExpression(traceStr).WithTrailingTrivia(SyntaxFactory.Whitespace("\n"));
        }

        public static ExpressionSyntax ConditionExpressionWithEndingTrace(SyntaxNode originalNode, SyntaxNode condition, int sourceFileId)
        {
            // "|| true" is for telling to the compiler that the left expression will always be true
            var traceStr = string.Format("({5}{4}.TraceSimpleStatement({0},{1},{2}) || true) && ({3}) && ({5}{4}.Expression_True({0},{1},{2}) || true) ;", sourceFileId, originalNode.Span.Start, originalNode.Span.End, condition == null ? "true" : condition.ToString(), TraceType, NamespaceName);
            return SyntaxFactory.ParseExpression(traceStr).WithTrailingTrivia(SyntaxFactory.Whitespace("\n"));
        }

        public static ExpressionSyntax ConditionExpressionWithEndingTrace(SyntaxNode originalNode, int sourceFileId)
        {
            var traceStr = string.Format("({4}{3}.TraceSimpleStatement({0},{1},{2}) && {4}{3}.Expression_True({0},{1},{2})) ;", 
                sourceFileId, originalNode.Span.Start, originalNode.Span.End, TraceType, NamespaceName);
            return SyntaxFactory.ParseExpression(traceStr).WithTrailingTrivia(SyntaxFactory.Whitespace("\n"));
        }

        public static StatementSyntax EnterConditionStatement(SyntaxNode originalNode, int sourceFileId)
        {
            var traceStr = string.Format("{4}{3}.TraceEnterCondition({0},{1},{2});", sourceFileId, originalNode.Span.Start, originalNode.Span.End, TraceType, NamespaceName);
            return SyntaxFactory.ParseStatement(traceStr).WithTrailingTrivia(SyntaxFactory.Whitespace("\n"));
        }

        public static StatementSyntax ExitConditionStatement(SyntaxNode originalNode, int sourceFileId)
        {
            var traceStr = string.Format("{4}{3}.TraceExitCondition({0},{1},{2});", sourceFileId, originalNode.Span.Start, originalNode.Span.End, TraceType, NamespaceName);
            return SyntaxFactory.ParseStatement(traceStr).WithTrailingTrivia(SyntaxFactory.Whitespace("\n"));
        }

        public static ExpressionSyntax ExitConditionExpression(SyntaxNode originalNode, int sourceFileId)
        {
            var traceStr = string.Format("{4}{3}.TraceExitCondition({0},{1},{2})", sourceFileId, originalNode.Span.Start, originalNode.Span.End, TraceType, NamespaceName);
            return SyntaxFactory.ParseExpression(traceStr);
        }

        public static StatementSyntax ExitLoopStatement(SyntaxNode originalNode, int sourceFileId)
        {
            var traceStr = string.Format("{4}{3}.TraceExitLoop({0},{1},{2});", sourceFileId, originalNode.Span.Start, originalNode.Span.End, TraceType, NamespaceName);
            return SyntaxFactory.ParseStatement(traceStr).WithTrailingTrivia(SyntaxFactory.Whitespace("\n"));
        }

        public static StatementSyntax ExitLoopByExceptionStatement(SyntaxNode originalNode, int sourceFileId)
        {
            var traceStr = string.Format("{4}{3}.TraceExitLoopByException({0},{1},{2});", sourceFileId, originalNode.Span.Start, originalNode.Span.End, TraceType, NamespaceName);
            return SyntaxFactory.ParseStatement(traceStr).WithTrailingTrivia(SyntaxFactory.Whitespace("\n"));
        }

        public static StatementSyntax BreakStatement(SyntaxNode originalNode, int sourceFileId)
        {
            var traceStr = string.Format("{4}{3}.TraceBreak({0},{1},{2});", sourceFileId, originalNode.Span.Start, originalNode.Span.End, TraceType, NamespaceName);
            return SyntaxFactory.ParseStatement(traceStr).WithTrailingTrivia(SyntaxFactory.Whitespace("\n"));
        }

        public static StatementSyntax ExitUsingStatement(SyntaxNode originalNode, int sourceFileId)
        {
            var traceStr = string.Format("{4}{3}.TraceExitUsing({0},{1},{2});", sourceFileId, originalNode.Span.Start, originalNode.Span.End, TraceType, NamespaceName);
            return SyntaxFactory.ParseStatement(traceStr).WithTrailingTrivia(SyntaxFactory.Whitespace("\n"));
        }

        public static StatementSyntax EnterMethodStatement(SyntaxNode originalNode, int sourceFileId)
        {
            var traceStr = string.Format("{4}{3}.TraceEnterMethod({0},{1},{2});", sourceFileId, originalNode.Span.Start, originalNode.Span.End, TraceType, NamespaceName);
            return SyntaxFactory.ParseStatement(traceStr).WithTrailingTrivia(SyntaxFactory.Whitespace("\n"));
        }

        public static StatementSyntax ExitMethodStatement(SyntaxNode originalNode, int sourceFileId)
        {
            var traceStr = string.Format("{4}{3}.TraceExitMethod({0},{1},{2});", sourceFileId, originalNode.Span.Start, originalNode.Span.End, TraceType, NamespaceName);
            return SyntaxFactory.ParseStatement(traceStr).WithTrailingTrivia(SyntaxFactory.Whitespace("\n"));
        }

        public static ExpressionSyntax BeforeConstructorExpression(SyntaxNode originalNode, int sourceFileId)
        {
            var traceStr = string.Format("{4}{3}.TraceBeforeConstructor({0},{1},{2})", sourceFileId, originalNode.Span.Start, originalNode.Span.End, TraceType, NamespaceName);
            return SyntaxFactory.ParseExpression(traceStr);
        }

        public static StatementSyntax EnterConstructorStatement(SyntaxNode originalNode, int sourceFileId)
        {
            var traceStr = string.Format("{4}{3}.TraceEnterConstructor({0},{1},{2});", sourceFileId, originalNode.Span.Start, originalNode.Span.End, TraceType, NamespaceName);
            return SyntaxFactory.ParseStatement(traceStr).WithTrailingTrivia(SyntaxFactory.Whitespace("\n"));
        }

        public static StatementSyntax ExitConstructorStatement(SyntaxNode originalNode, int sourceFileId)
        {
            var traceStr = string.Format("{4}{3}.TraceExitConstructor({0},{1},{2});", sourceFileId, originalNode.Span.Start, originalNode.Span.End, TraceType, NamespaceName);
            return SyntaxFactory.ParseStatement(traceStr).WithTrailingTrivia(SyntaxFactory.Whitespace("\n"));
        }

        public static StatementSyntax EnterStaticMethodStatement(SyntaxNode originalNode, int sourceFileId)
        {
            var traceStr = string.Format("{4}{3}.TraceEnterStaticMethod({0},{1},{2});", sourceFileId, originalNode.Span.Start, originalNode.Span.End, TraceType, NamespaceName);
            return SyntaxFactory.ParseStatement(traceStr).WithTrailingTrivia(SyntaxFactory.Whitespace("\n"));
        }

        public static StatementSyntax ExitStaticMethodStatement(SyntaxNode originalNode, int sourceFileId)
        {
            var traceStr = string.Format("{4}{3}.TraceExitStaticMethod({0},{1},{2});", sourceFileId, originalNode.Span.Start, originalNode.Span.End, TraceType, NamespaceName);
            return SyntaxFactory.ParseStatement(traceStr).WithTrailingTrivia(SyntaxFactory.Whitespace("\n"));
        }

        public static StatementSyntax EnterCatchStatement(SyntaxNode originalNode, int sourceFileId)
        {
            var traceStr = string.Format("{4}{3}.TraceEnterCatch({0},{1},{2});", sourceFileId, originalNode.Span.Start, originalNode.Span.End, TraceType, NamespaceName);
            return SyntaxFactory.ParseStatement(traceStr).WithTrailingTrivia(SyntaxFactory.Whitespace("\n"));
        }

        public static StatementSyntax ExitCatchStatement(SyntaxNode originalNode, int sourceFileId)
        {
            var traceStr = string.Format("{4}{3}.TraceExitCatch({0},{1},{2});", sourceFileId, originalNode.Span.Start, originalNode.Span.End, TraceType, NamespaceName);
            return SyntaxFactory.ParseStatement(traceStr).WithTrailingTrivia(SyntaxFactory.Whitespace("\n"));
        }

        public static StatementSyntax EnterFinallyStatement(SyntaxNode originalNode, int sourceFileId)
        {
            var traceStr = string.Format("{4}{3}.TraceEnterFinally({0},{1},{2});", sourceFileId, originalNode.Span.Start, originalNode.Span.End, TraceType, NamespaceName);
            return SyntaxFactory.ParseStatement(traceStr).WithTrailingTrivia(SyntaxFactory.Whitespace("\n"));
        }

        public static StatementSyntax ExitFinallyStatement(SyntaxNode originalNode, int sourceFileId)
        {
            var traceStr = string.Format("{4}{3}.TraceExitFinally({0},{1},{2});", sourceFileId, originalNode.Span.Start, originalNode.Span.End, TraceType, NamespaceName);
            return SyntaxFactory.ParseStatement(traceStr).WithTrailingTrivia(SyntaxFactory.Whitespace("\n"));
        }

        public static StatementSyntax EnterFinalCatchStatement(SyntaxNode originalNode, int sourceFileId)
        {
            var traceStr = string.Format("{4}{3}.TraceEnterFinalCatch({0},{1},{2});", sourceFileId, originalNode.Span.Start, originalNode.Span.End, TraceType, NamespaceName);
            return SyntaxFactory.ParseStatement(traceStr).AddPreAndPostLine();
        }

        public static StatementSyntax EnterFinalFinallyStatement(SyntaxNode originalNode, int sourceFileId)
        {
            var traceStr = string.Format("{4}{3}.TraceEnterFinalFinally({0},{1},{2});", sourceFileId, originalNode.Span.Start, originalNode.Span.End, TraceType, NamespaceName);
            return SyntaxFactory.ParseStatement(traceStr).AddPreAndPostLine();
        }

        public static StatementSyntax EnterStaticConstructorStatement(SyntaxNode originalNode, int sourceFileId)
        {
            var traceStr = string.Format("{4}{3}.TraceEnterStaticConstructor({0},{1},{2});", sourceFileId, originalNode.Span.Start, originalNode.Span.End, TraceType, NamespaceName);
            return SyntaxFactory.ParseStatement(traceStr).WithTrailingTrivia(SyntaxFactory.Whitespace("\n"));
        }

        public static StatementSyntax ExitStaticConstructorStatement(SyntaxNode originalNode, int sourceFileId)
        {
            var traceStr = string.Format("{4}{3}.TraceExitStaticConstructor({0},{1},{2});", sourceFileId, originalNode.Span.Start, originalNode.Span.End, TraceType, NamespaceName);
            return SyntaxFactory.ParseStatement(traceStr).WithTrailingTrivia(SyntaxFactory.Whitespace("\n"));
        }

        public static StatementSyntax EndInvocation(SyntaxNode originalNode, int sourceFileId)
        {
            var traceStr = string.Format("{4}{3}.TraceEndInvocation({0},{1},{2});", sourceFileId, originalNode.Span.Start, originalNode.Span.End, TraceType, NamespaceName);
            return SyntaxFactory.ParseStatement(traceStr).WithTrailingTrivia(SyntaxFactory.Whitespace("\n"));
        }

        public static ExpressionSyntax ConditionalOperatorExpression(SyntaxNode rewritedNode, SyntaxNode originalNode, SemanticModel model, int sourceFileId)
        {
            var originalCondition = ((ConditionalExpressionSyntax)originalNode).Condition;
            var originalWhenTrue = ((ConditionalExpressionSyntax)originalNode).WhenTrue;
            var originalWhenFalse = ((ConditionalExpressionSyntax)originalNode).WhenFalse;

            var conditional = (ConditionalExpressionSyntax)rewritedNode;
            var whenTrueExp = (ExpressionSyntax)conditional.WhenTrue;
            var whenFalseExp = (ExpressionSyntax)conditional.WhenFalse;
            var type = model.GetTypeInfo(originalNode).Type;
            var traceStr = string.Format("{12}{11}.TraceConditionalOperator<{0}>({1},() => {2},{3},{4}, () => {5},{6},{7},() => {8},{9},{10})", type.ToString(), sourceFileId, conditional.Condition, originalCondition.Span.Start, originalCondition.Span.End, conditional.WhenTrue, originalWhenTrue.Span.Start, originalWhenTrue.Span.End, conditional.WhenFalse, originalWhenFalse.Span.Start, originalWhenFalse.Span.End, TraceType, NamespaceName);
            return SyntaxFactory.ParseExpression(traceStr);
        }

        public static ExpressionSyntax InvocationWrapperExpression(SyntaxNode modifiedNode, SyntaxNode originalNode, int sourceFileId)
        {
            var lambda = "() => " + modifiedNode.ToString();
            var traceStr = string.Format("{5}{4}.TraceInvocationWrapper({0},{1},{2},{3})", lambda, sourceFileId, originalNode.Span.Start, originalNode.Span.End, TraceType, NamespaceName);
            return SyntaxFactory.ParseExpression(traceStr).WithLeadingTrivia(modifiedNode.GetLeadingTrivia()).WithTrailingTrivia(modifiedNode.GetTrailingTrivia());
        }

        public static ExpressionSyntax InitializerWrapperExpression(SyntaxNode modifiedNode, SemanticModel model, SyntaxNode originalNode, int sourceFileId)
        {
            var typeInfo = model.GetTypeInfo(originalNode);
            
            // XXX TODO: This is unsound for structs, we are copying the struct instead of using references and error prone
            var lambda = "() => " + modifiedNode.ToString();
            var traceStr = string.Format("{5}{4}.TraceInitializationWrapper({0},{1},{2},{3})", lambda, sourceFileId, originalNode.Span.Start, originalNode.Span.End, TraceType, NamespaceName);
            return SyntaxFactory.ParseExpression(traceStr).WithLeadingTrivia(modifiedNode.GetLeadingTrivia()).WithTrailingTrivia(modifiedNode.GetTrailingTrivia());
        }

        public static ExpressionSyntax MemberAccessWrapperExpression(SyntaxNode modifiedNode, SemanticModel model, SyntaxNode originalNode, int sourceFileId)
        {
            var lambda = "() => " + modifiedNode.ToString();
            var traceStr = string.Format("{5}{4}.TraceMemberAccessWrapper({0},{1},{2},{3})", lambda, sourceFileId, originalNode.Span.Start, originalNode.Span.End, TraceType, NamespaceName);
            return SyntaxFactory.ParseExpression(traceStr);
        }

        public static ExpressionSyntax InitialMemberAccessWrapperExpression(SyntaxNode modifiedNode, SemanticModel model, SyntaxNode originalNode, int sourceFileId)
        {
            var lambda = "() => " + modifiedNode.ToString();
            var traceStr = string.Format("{5}{4}.TraceInitialMemberAccessWrapper({0},{1},{2},{3})", lambda, sourceFileId, originalNode.Span.Start, originalNode.Span.End, TraceType, NamespaceName);
            return SyntaxFactory.ParseExpression(traceStr);
        }

        public static ExpressionSyntax EnterExpression(SyntaxNode modifiedNode, SemanticModel model, SyntaxNode originalNode, int sourceFileId)
        {
            var lambda = "() => " + modifiedNode.ToString();
            var type = model.GetTypeInfo(originalNode).Type;
            // Problemas con los XAML, no tira el tipo
            string traceStr = "";
            if (type.ToString() == "?")
                traceStr = string.Format("{5}{4}.TraceEnterExpression({0},{1},{2},{3})",
                    lambda, sourceFileId, originalNode.Span.Start, originalNode.Span.End, TraceType, NamespaceName);
            else
                traceStr = string.Format("{6}{5}.TraceEnterExpression<{4}>({0},{1},{2},{3})",
                    lambda, sourceFileId, originalNode.Span.Start, originalNode.Span.End, type.ToString(), TraceType, NamespaceName);
            return SyntaxFactory.ParseExpression(traceStr);
        }

        /////////////////////////// 
        public static MethodDeclarationSyntax GenericInvocationWrapped(SyntaxNode modifiedNode, SemanticModel model, SyntaxNode originalNode, int sourceFileId, bool this_ref = false)
        {
            try
            {
                var operation = model.GetOperation(originalNode);
                IMethodSymbol symbolInfo = null;
                if (originalNode is InvocationExpressionSyntax)
                {
                    var tempSymbolInfo = model.GetSymbolInfo(((InvocationExpressionSyntax)originalNode).Expression).Symbol;
                    if (tempSymbolInfo != null && tempSymbolInfo is IMethodSymbol)
                        symbolInfo = (IMethodSymbol)tempSymbolInfo;
                    if (symbolInfo == null)
                        symbolInfo = Utils.GetMethodSymbolInfo(model, (InvocationExpressionSyntax)originalNode);
                }
                else if (originalNode is ObjectCreationExpressionSyntax && (operation == null || operation.Kind == OperationKind.Invalid))
                    symbolInfo = Utils.GetMethodSymbolInfo(model, (ObjectCreationExpressionSyntax)originalNode);

                if (originalNode is InvocationExpressionSyntax || originalNode is ObjectCreationExpressionSyntax)
                {
                    bool ok_iop = operation != null && operation.Kind != OperationKind.Invalid;
                    bool ok_symbol = symbolInfo != null;
                    if (!ok_iop && originalNode is ObjectCreationExpressionSyntax)
                        ;

                    BugLogging.Log(sourceFileId, (CSharpSyntaxNode)originalNode, ok_iop ? BugLogging.Behavior.WithIOperation : (ok_symbol ? BugLogging.Behavior.WithoutIOperation : BugLogging.Behavior.WithoutSymbol));
                }

                // Los fields no se wrappean, por ahora los eventos tampoco
                if (operation is IFieldReferenceOperation || operation is IEventReferenceOperation || operation == null)
                    return null;

                var name = string.Format("f_{0}_{1}_{2}", sourceFileId, originalNode.Span.Start, originalNode.Span.End);
                var is_property_access = operation is IPropertyReferenceOperation || operation is IDynamicMemberReferenceOperation;

                var returnType = symbolInfo != null ? symbolInfo.ReturnType : operation?.Type;
                var typeParameters = new HashSet<string>();
                ExtractTypeParameters(returnType, typeParameters, originalNode, true);
                // TODO ROSLYN TYPE INFERENCE
                var returnTypeName = returnType != null ? GetTypeName(returnType) : "object";

                if (returnType != null && returnType.IsValueType && returnType.NullableAnnotation == NullableAnnotation.NotAnnotated &&
                    originalNode.GetParentsUntilContainer().Any(x => x is ConditionalAccessExpressionSyntax))
                    returnTypeName = returnTypeName + "?";

                var parameterList = new SeparatedSyntaxList<ParameterSyntax>();


                #region Receiver
                var call_expression = "";
                var skip_first_param = false;
                var implicit_this = false;
                var is_unsafe = false;
                if (is_property_access)
                {
                    var instance = operation is IPropertyReferenceOperation ? ((IPropertyReferenceOperation)operation).Instance : ((IDynamicMemberReferenceOperation)operation).Instance;
                    if (instance != null)
                        ExtractTypeParameters(instance.Type, typeParameters, originalNode, false);

                    if (operation is IPropertyReferenceOperation && ((IPropertyReferenceOperation)operation).IsIndexer())
                    {
                        var thisType = GetTypeName(instance.Type);
                        var parameterName = SyntaxFactory.Identifier("this_param");
                        var thisTypeSyntax = SyntaxFactory.ParseTypeName(thisType).AddPostLine();
                        parameterList = parameterList.Add(SyntaxFactory.Parameter
                            (new SyntaxList<AttributeListSyntax>(), new SyntaxTokenList(), thisTypeSyntax, parameterName, null));
                        call_expression = "this_param[";

                        // ACOMODAR ESTO POR EL AMOR DEL SEÑOR
                        var i = 0;
                        foreach (var a in ((IPropertyReferenceOperation)operation).Arguments)
                        {
                            var p_name = "i" + (i++).ToString();
                            parameterList = parameterList.Add(SyntaxFactory.Parameter(new SyntaxList<AttributeListSyntax>(), new SyntaxTokenList(),
                                SyntaxFactory.IdentifierName(SyntaxFactory.Identifier(GetTypeName(a.Value.Type))).AddPostLine(),
                                SyntaxFactory.Identifier(p_name), null));
                            call_expression = call_expression + " " + p_name + ",";
                        }
                        call_expression = call_expression.Substring(0, call_expression.Length - 1) + "]";
                    }
                    else if (instance == null)
                        call_expression = modifiedNode.GetText().ToString();
                    else
                    {
                        if (!(originalNode is IdentifierNameSyntax))
                        {
                            var thisType = GetTypeName(instance.Type);
                            var parameterName = SyntaxFactory.Identifier("this_param");
                            var thisTypeSyntax = SyntaxFactory.ParseTypeName((instance.Type.CustomIsStruct() && this_ref ? "ref " : "") + thisType).AddPostLine();
                            parameterList = parameterList.Add(SyntaxFactory.Parameter
                                (new SyntaxList<AttributeListSyntax>(), new SyntaxTokenList(), thisTypeSyntax, parameterName, null));
                            call_expression = "this_param." + ((MemberAccessExpressionSyntax)modifiedNode).Name.GetText().ToString();
                        }
                        else
                        {
                            // Se asume el this implícito. Entonces el método no puede ser estático
                            call_expression = modifiedNode.ToString();
                            implicit_this = true;
                        }

                    }
                }
                else if (operation is IInvocationOperation &&
                    ((IInvocationOperation)operation).Instance != null &&
                    (((ITypeSymbol)((IInvocationOperation)operation).TargetMethod.ContainingSymbol)).TypeKind == TypeKind.Delegate &&
                    ((IInvocationOperation)operation).Instance is IPropertyReferenceOperation)
                {
                    var thisType = ((IPropertyReferenceOperation)((IInvocationOperation)operation).Instance).Instance.Type;
                    ExtractTypeParameters(thisType, typeParameters, originalNode, false);
                    var thisTypeName = GetTypeName(thisType);
                    var thisTypeSyntax = SyntaxFactory.ParseTypeName((thisType.CustomIsStruct() && this_ref ? "ref " : "") + thisTypeName).AddPostLine();
                    var parameterName = SyntaxFactory.Identifier("this_param");
                    var param = SyntaxFactory.Parameter(new SyntaxList<AttributeListSyntax>(), new SyntaxTokenList(), thisTypeSyntax, parameterName, null);
                    parameterList = parameterList.Add(param);
                    call_expression = "this_param." + ((IPropertyReferenceOperation)((IInvocationOperation)operation).Instance).Member.Name;
                    if (((IInvocationOperation)operation).TargetMethod.TypeArguments.Count() > 0)
                        call_expression = call_expression + "<" + string.Join(",", ((IInvocationOperation)operation).TargetMethod.TypeArguments.Select(x => GetTypeName(x))) + ">";
                }
                else if (operation is IInvocationOperation &&
                    ((IInvocationOperation)operation).Instance != null &&
                    (((ITypeSymbol)((IInvocationOperation)operation).TargetMethod.ContainingSymbol)).TypeKind == TypeKind.Delegate &&
                    ((IInvocationOperation)operation).Instance is IFieldReferenceOperation)
                {
                    if (((IFieldReferenceOperation)((IInvocationOperation)operation).Instance).Field.IsStatic)
                        call_expression = ((IFieldReferenceOperation)((IInvocationOperation)operation).Instance).Member.Name;
                    else
                    {
                        var thisType = ((IFieldReferenceOperation)((IInvocationOperation)operation).Instance).Instance.Type;
                        ExtractTypeParameters(thisType, typeParameters, originalNode, false);
                        var thisTypeName = GetTypeName(thisType);
                        var thisTypeSyntax = SyntaxFactory.ParseTypeName((thisType.CustomIsStruct() && this_ref ? "ref " : "") + thisTypeName).AddPostLine();
                        var parameterName = SyntaxFactory.Identifier("this_param");
                        var param = SyntaxFactory.Parameter(new SyntaxList<AttributeListSyntax>(), new SyntaxTokenList(), thisTypeSyntax, parameterName, null);
                        parameterList = parameterList.Add(param);
                        call_expression = "this_param." + ((IFieldReferenceOperation)((IInvocationOperation)operation).Instance).Member.Name;
                    }
                    if (((IInvocationOperation)operation).TargetMethod.TypeArguments.Count() > 0)
                        call_expression = call_expression + "<" + string.Join(",", ((IInvocationOperation)operation).TargetMethod.TypeArguments.Select(x => GetTypeName(x))) + ">";
                }
                else if (operation is IInvocationOperation && ((IInvocationOperation)operation).Instance != null)
                {
                    var thisType = ((IInvocationOperation)operation).Instance.Type;

                    ExtractTypeParameters(thisType, typeParameters, originalNode, false);

                    var thisTypeName = GetTypeName(thisType);
                    var thisTypeSyntax = SyntaxFactory.ParseTypeName((thisType.CustomIsStruct() && this_ref ? "ref " : "") + thisTypeName).AddPostLine();
                    var parameterName = SyntaxFactory.Identifier("this_param");
                    var param = SyntaxFactory.Parameter(new SyntaxList<AttributeListSyntax>(), new SyntaxTokenList(), thisTypeSyntax, parameterName, null);
                    parameterList = parameterList.Add(param);

                    call_expression = "this_param";
                    if (operation.Parent != null && operation.Parent.Kind == OperationKind.ConditionalAccess)
                    {
                        if (returnTypeName != "void" && returnTypeName.Last() != '?')
                            returnTypeName += "?";
                        call_expression += "?";
                    }
                    call_expression += "." + ((IInvocationOperation)operation).TargetMethod.Name;
                    if (((IInvocationOperation)operation).TargetMethod.TypeArguments.Count() > 0)
                        call_expression = call_expression + "<" + string.Join(",", ((IInvocationOperation)operation).TargetMethod.TypeArguments.Select(x => GetTypeName(x))) + ">";
                }
                else if (operation is IInvocationOperation)
                {
                    if (((((InvocationExpressionSyntax)originalNode).Expression is MemberAccessExpressionSyntax) &&
                        (model.GetOperation((((MemberAccessExpressionSyntax)((InvocationExpressionSyntax)originalNode).Expression).Expression).RemoveParentesisAndMore()) != null)) || (operation != null && ((IInvocationOperation)operation).TargetMethod.IsExtensionMethod))
                    {
                        var conditionalAccess = (originalNode.Parent is ConditionalAccessExpressionSyntax && originalNode == ((ConditionalAccessExpressionSyntax)originalNode.Parent).WhenNotNull);
                        var paramName = ((IInvocationOperation)operation).Arguments.First().Parameter.CustomGetName();
                        if ((((IInvocationOperation)operation).Arguments.First().Syntax is ArgumentSyntax) &&
                            (((ArgumentSyntax)((IInvocationOperation)operation).Arguments.First().Syntax).Expression is CastExpressionSyntax))
                            paramName = string.Format("(({0}){1})", 
                                ((CastExpressionSyntax)(((ArgumentSyntax)((IInvocationOperation)operation).Arguments.First().Syntax).Expression)).Type.ToString(),
                                paramName);
                        call_expression = paramName + (conditionalAccess ? "?" : "") + "." + ((IInvocationOperation)operation).TargetMethod.Name;
                        if (((IInvocationOperation)operation).TargetMethod.TypeArguments.Count() > 0)
                            call_expression = call_expression + "<" + string.Join(",", ((IInvocationOperation)operation).TargetMethod.TypeArguments.Select(x => GetTypeName(x))) + ">";
                        skip_first_param = true;
                    }
                    else
                        call_expression = ((InvocationExpressionSyntax)modifiedNode).Expression.ToString();
                }
                else if (operation is IDynamicInvocationOperation)
                {
                    // Hay veces que por ser dynamic infiere mal los tipos... (no sé porque)
                    // Esto es por si tiene THIS
                    if (symbolInfo != null && symbolInfo.ReceiverType != null)
                    {
                        var thisType = symbolInfo.ReceiverType;

                        ExtractTypeParameters(thisType, typeParameters, originalNode, false);

                        var thisTypeName = GetTypeName(thisType);
                        var thisTypeSyntax = SyntaxFactory.ParseTypeName((thisType.CustomIsStruct() && this_ref ? "ref " : "") + thisTypeName).AddPostLine();
                        var parameterName = SyntaxFactory.Identifier("this_param");
                        var param = SyntaxFactory.Parameter(new SyntaxList<AttributeListSyntax>(), new SyntaxTokenList(), thisTypeSyntax, parameterName, null);
                        parameterList = parameterList.Add(param);

                        call_expression = "this_param";
                        if (operation.Parent != null && operation.Parent.Kind == OperationKind.ConditionalAccess)
                        {
                            if (returnTypeName != "void" && returnTypeName.Last() != '?')
                                returnTypeName += "?";
                            call_expression += "?";
                        }
                        call_expression += "." + symbolInfo.Name;
                        if (symbolInfo.TypeArguments.Count() > 0)
                            call_expression = call_expression + "<" + string.Join(",", symbolInfo.TypeArguments.Select(x => GetTypeName(x))) + ">";
                    }
                    else
                    {
                        if (((IDynamicInvocationOperation)operation).Operation is IDynamicMemberReferenceOperation &&
                            ((IDynamicMemberReferenceOperation)((IDynamicInvocationOperation)operation).Operation).Instance != null)
                        {
                            var thisType = ((IDynamicMemberReferenceOperation)((IDynamicInvocationOperation)operation).Operation).Instance.Type;

                            ExtractTypeParameters(thisType, typeParameters, originalNode, false);

                            var thisTypeName = GetTypeName(thisType);
                            var thisTypeSyntax = SyntaxFactory.ParseTypeName((thisType.CustomIsStruct() && this_ref ? "ref " : "") + thisTypeName).AddPostLine();
                            var parameterName = SyntaxFactory.Identifier("this_param");
                            var param = SyntaxFactory.Parameter(new SyntaxList<AttributeListSyntax>(), new SyntaxTokenList(), thisTypeSyntax, parameterName, null);
                            parameterList = parameterList.Add(param);

                            call_expression = "this_param";
                            if (operation.Parent != null && operation.Parent.Kind == OperationKind.ConditionalAccess)
                            {
                                if (returnTypeName != "void" && returnTypeName.Last() != '?')
                                    returnTypeName += "?";
                                call_expression += "?";
                            }
                            call_expression += "." + ((IDynamicMemberReferenceOperation)((IDynamicInvocationOperation)operation).Operation).MemberName;
                            if (((IDynamicMemberReferenceOperation)((IDynamicInvocationOperation)operation).Operation).TypeArguments.Count() > 0)
                                call_expression = call_expression + "<" + string.Join(",", ((IDynamicMemberReferenceOperation)((IDynamicInvocationOperation)operation).Operation).TypeArguments.Select(x => GetTypeName(x))) + ">";
                        }
                        else
                            call_expression = ((InvocationExpressionSyntax)modifiedNode).Expression.ToString();
                    }
                }
                else if (operation is IObjectCreationOperation ||
                         operation is IDynamicObjectCreationOperation ||
                         operation is ITypeParameterObjectCreationOperation)
                    call_expression = string.Format("new {0}", returnTypeName);
                else if (operation.Kind == OperationKind.Invalid && originalNode is InvocationExpressionSyntax && symbolInfo != null)
                {
                    // Si no es estático hay que agregar la instancia...
                    if (!symbolInfo.IsStatic)
                    {
                        var thisTypeName = GetTypeName(symbolInfo.ReceiverType);
                        var thisTypeSyntax = SyntaxFactory.ParseTypeName((symbolInfo.ReceiverType.CustomIsStruct() && this_ref ? "ref " : "") + thisTypeName).AddPostLine();
                        var parameterName = SyntaxFactory.Identifier("this_param");
                        var param = SyntaxFactory.Parameter(new SyntaxList<AttributeListSyntax>(), new SyntaxTokenList(), thisTypeSyntax, parameterName, null);
                        parameterList = parameterList.Add(param);

                        call_expression = "this_param";
                        if (originalNode.Parent.Kind() == SyntaxKind.ConditionalAccessExpression)
                        {
                            if (returnTypeName != "void" && returnTypeName.Last() != '?')
                                returnTypeName += "?";
                            call_expression += "?";
                        }
                        call_expression += "." + symbolInfo.Name;
                        if (symbolInfo.TypeArguments.Count() > 0)
                            call_expression = call_expression + "<" + string.Join(",", symbolInfo.TypeArguments.Select(x => GetTypeName(x))) + ">";
                    }
                    else
                        call_expression = ((InvocationExpressionSyntax)modifiedNode).Expression.ToString();
                }
                else if (operation.Kind == OperationKind.Invalid && originalNode is ObjectCreationExpressionSyntax && symbolInfo != null)
                {
                    call_expression = string.Format("new {0}", symbolInfo.ReceiverType);
                }
                else
                    throw new SlicerException("Unexpected expression");
                #endregion

                #region Parámetros
                var arguments_list = "";
                if (operation is IInvocationOperation || operation is IObjectCreationOperation)
                {
                    var args = operation is IInvocationOperation ? ((IInvocationOperation)operation).Arguments : ((IObjectCreationOperation)operation).Arguments;
                    foreach (var a in args)
                    {
                        if (a.IsImplicit && a.ArgumentKind == ArgumentKind.DefaultValue)
                            continue;

                        var p_other_name = a.Parameter.ToMinimalDisplayParts(model, 0).Any(x => x.ToString().Trim().FirstOrDefault() == '@');
                        var p_name = p_other_name ? "@" + a.Parameter.Name : a.Parameter.Name;

                        if (a.Value.CustomGetType() != null)
                            ExtractTypeParameters(a.Value.CustomGetType(), typeParameters, originalNode, false);

                        var type_name = GetTypeName(a.Value.CustomGetType() ?? a.Parameter.Type);
                        is_unsafe = is_unsafe || type_name.Last() == '*';
                        var param = SyntaxFactory.Parameter(new SyntaxList<AttributeListSyntax>(), new SyntaxTokenList(),
                            SyntaxFactory.IdentifierName(SyntaxFactory.Identifier(type_name)).AddPostLine(),
                            SyntaxFactory.Identifier(p_name), null);

                        var modif = "";
                        if (a.Parameter.RefKind == RefKind.Ref || 
                            // Extension methods for structs
                            (operation is IInvocationOperation && ((IInvocationOperation)operation).TargetMethod.IsExtensionMethod && a == args.First() && (a.Parameter.Type.CustomIsStruct() || (a.Value.CustomGetType() != null && a.Value.CustomGetType().CustomIsStruct()))))
                        {
                            param = param.WithModifiers(SyntaxFactory.TokenList(
                                SyntaxFactory.Token(SyntaxFactory.TriviaList(), SyntaxKind.RefKeyword, SyntaxFactory.TriviaList(SyntaxFactory.Space))));
                            modif = "ref ";
                        }
                        if (a.Parameter.RefKind == RefKind.Out)
                        {
                            param = param.WithModifiers(SyntaxFactory.TokenList(
                                SyntaxFactory.Token(SyntaxFactory.TriviaList(), SyntaxKind.OutKeyword, SyntaxFactory.TriviaList(SyntaxFactory.Space))));
                            modif = "out ";
                        }
                        if (a.Parameter.IsParams)
                        {
                            param = param.WithModifiers(SyntaxFactory.TokenList(
                                SyntaxFactory.Token(SyntaxFactory.TriviaList(), SyntaxKind.ParamsKeyword, SyntaxFactory.TriviaList(SyntaxFactory.Space))));
                        }
                        parameterList = parameterList.Add(param);

                        if (skip_first_param)
                        {
                            skip_first_param = false;
                            continue;
                        }

                        if (a.Value.CustomGetType() != null && !SymbolEqualityComparer.Default.Equals(a.Value.Type, a.Value.CustomGetType()))
                            p_name =  string.Format("({0})", GetTypeName(a.Value.Type)) + p_name;
                        if (a.Syntax is ArgumentSyntax && ((ArgumentSyntax)a.Syntax).NameColon != null && (a.Parameter.RefKind != RefKind.Out))
                            modif = ((ArgumentSyntax)a.Syntax).NameColon.ToString() + " " + modif;

                        arguments_list = arguments_list + " " + modif + p_name + ",";
                    }
                    if (arguments_list.Length > 0)
                        arguments_list = arguments_list.Substring(0, arguments_list.Length - 1);
                }
                else if (operation is IDynamicInvocationOperation || operation is IDynamicObjectCreationOperation
                    || operation is ITypeParameterObjectCreationOperation || (operation is IInvalidOperation && symbolInfo != null))
                {
                    // Esto es porque está trayendo mal los tipos IOperation y lo termino sacando por symbol Info XXX
                    if (symbolInfo != null)
                    {
                        var @params = symbolInfo.Parameters;
                        var argCounter = 0;
                        foreach (var a in @params)
                        {
                            //if (a.IsImplicit && a.ArgumentKind == ArgumentKind.DefaultValue)
                            //    continue;

                            var p_other_name = a.Name[0] == '@';
                            var p_name = p_other_name ? "@" + a.Name : a.Name;
                            if (a.Type != null)
                                ExtractTypeParameters(a.Type, typeParameters, originalNode, false);

                            var type_name = GetTypeName(a.Type ?? a.Type);
                            is_unsafe = is_unsafe || type_name.Last() == '*';
                            var param = SyntaxFactory.Parameter(new SyntaxList<AttributeListSyntax>(), new SyntaxTokenList(),
                                SyntaxFactory.IdentifierName(SyntaxFactory.Identifier(type_name)).AddPostLine(),
                                SyntaxFactory.Identifier(p_name), null);

                            var modif = "";
                            if (a.RefKind == RefKind.Ref)
                            {
                                param = param.WithModifiers(SyntaxFactory.TokenList(
                                    SyntaxFactory.Token(SyntaxFactory.TriviaList(), SyntaxKind.RefKeyword, SyntaxFactory.TriviaList(SyntaxFactory.Space))));
                                modif = "ref ";
                            }
                            if (a.RefKind == RefKind.Out)
                            {
                                param = param.WithModifiers(SyntaxFactory.TokenList(
                                    SyntaxFactory.Token(SyntaxFactory.TriviaList(), SyntaxKind.OutKeyword, SyntaxFactory.TriviaList(SyntaxFactory.Space))));
                                modif = "out ";
                            }
                            if (a.IsParams)
                            {
                                param = param.WithModifiers(SyntaxFactory.TokenList(
                                    SyntaxFactory.Token(SyntaxFactory.TriviaList(), SyntaxKind.ParamsKeyword, SyntaxFactory.TriviaList(SyntaxFactory.Space))));
                            }
                            parameterList = parameterList.Add(param);

                            if (skip_first_param)
                            {
                                skip_first_param = false;
                                continue;
                            }
                            if (argCounter < ((InvocationExpressionSyntax)originalNode).ArgumentList.Arguments.Count && 
                                ((InvocationExpressionSyntax)originalNode).ArgumentList.Arguments[argCounter].NameColon != null && (a.RefKind != RefKind.Out))
                                modif = modif + ((InvocationExpressionSyntax)originalNode).ArgumentList.Arguments[argCounter].NameColon.ToString();
                            arguments_list = arguments_list + " " + modif + p_name + ",";
                            argCounter++;
                        }
                        if (arguments_list.Length > 0)
                            arguments_list = arguments_list.Substring(0, arguments_list.Length - 1);
                    }
                    else if (!(operation is ITypeParameterObjectCreationOperation))
                    { 
                        var args = operation is IDynamicInvocationOperation ? ((IDynamicInvocationOperation)operation).Arguments : ((IDynamicObjectCreationOperation)operation).Arguments;
                        var i = 0;
                        foreach (var a in args)
                        {
                            var p_name = "i" + (i++).ToString();
                            ExtractTypeParameters(a.Type, typeParameters, originalNode, false);
                            parameterList = parameterList.Add(SyntaxFactory.Parameter(new SyntaxList<AttributeListSyntax>(), new SyntaxTokenList(),
                                SyntaxFactory.IdentifierName(SyntaxFactory.Identifier(GetTypeName(a.Type))).AddPostLine(),
                                SyntaxFactory.Identifier(p_name), null));
                            arguments_list = arguments_list + " " + p_name + ",";
                        }
                        if (arguments_list.Length > 0)
                            arguments_list = arguments_list.Substring(0, arguments_list.Length - 1);
                    }
                }
                #endregion

                #region Construcción del método
                // Adding container type parameters
                var containerSymbol = model.GetDeclaredSymbol(originalNode.GetContainer());
                if (containerSymbol != null && containerSymbol is IMethodSymbol)
                    foreach (var tp in ((IMethodSymbol)containerSymbol).TypeParameters)
                        typeParameters.Add(tp.Name.ToString());

                var container = originalNode.GetContainer();
                var willBeStatic = (!implicit_this && (originalNode.StaticContainer() && (originalNode.GetContainer() is ClassDeclarationSyntax || originalNode.GetContainer() is StructDeclarationSyntax) || originalNode.GetContainerOrConstructorInitializerSyntax() is ConstructorInitializerSyntax)) ||
                        container is ConstructorDeclarationSyntax;
                if (!willBeStatic && (typeParameters.Count > 0))
                {
                    var to_exclude = originalNode.GetTypeParametersOfContainer();
                    if (to_exclude != null)
                        foreach (var exc in to_exclude)
                            typeParameters.Remove(exc);
                }

                if (typeParameters.Count > 0)
                    name = name + "<" + string.Join(", ", typeParameters) + ">";
                var newMethod = SyntaxFactory.MethodDeclaration(SyntaxFactory.ParseTypeName(returnTypeName == "void" ? "int" : returnTypeName).AddPostLine(), name);
                if (is_unsafe)
                    newMethod = newMethod.WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxFactory.TriviaList(), SyntaxKind.UnsafeKeyword, SyntaxFactory.TriviaList(SyntaxFactory.Space))));
                
                if (willBeStatic)
                    newMethod = newMethod.WithModifiers(SyntaxFactory.TokenList(
                        newMethod.Modifiers.Union(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxFactory.TriviaList(), 
                        SyntaxKind.StaticKeyword, SyntaxFactory.TriviaList(SyntaxFactory.Space))))));
                if (typeParameters.Count > 0)
                { 
                    var t = originalNode.GetContainer();
                    var clauses = new SyntaxList<TypeParameterConstraintClauseSyntax>();
                    if (t is MethodDeclarationSyntax)
                        clauses = ((MethodDeclarationSyntax)t).ConstraintClauses;
                    t = t.Parent.GetContainer();
                    if (t is ClassDeclarationSyntax)
                        clauses = clauses.AddRange(((ClassDeclarationSyntax)t).ConstraintClauses);
                    if (t is StructDeclarationSyntax)
                        clauses = clauses.AddRange(((StructDeclarationSyntax)t).ConstraintClauses);
                    newMethod = newMethod.WithConstraintClauses(new SyntaxList<TypeParameterConstraintClauseSyntax>(clauses.Where(x => typeParameters.Contains(x.Name.GetText().ToString().Trim()))));
                }
                newMethod = newMethod.WithParameterList(SyntaxFactory.ParameterList(parameterList));
                #endregion

                #region Statements
                // 3) Invocaciones reales y envío de traza
                var returnSomething = (returnTypeName != "void");
                var ret_expr = (returnSomething ? "var return_v = " : "") + call_expression +
                    (is_property_access ? ";" : (string.Format("({0});", arguments_list)));

                SyntaxList<StatementSyntax> stmtList = new SyntaxList<StatementSyntax>();
                stmtList = stmtList.Add(SyntaxFactory.ParseStatement(ret_expr).AddPreLine());
                stmtList = stmtList.Add(SyntaxFactory.ParseStatement(string.Format("{0}{1}.{5}({2}, {3}, {4});",
                    NamespaceName, TraceType, sourceFileId, originalNode.Span.Start, originalNode.Span.End,
                    is_property_access ? "TraceEndMemberAccess" : "TraceEndInvocation")).AddPreLine());
                stmtList = stmtList.Add(SyntaxFactory.ParseStatement(returnSomething ? "return return_v;" : "return 0;").AddPreAndPostLine());
                newMethod = newMethod.WithBody(SyntaxFactory.Block(stmtList).AddPreAndPostLine()).AddPreAndPostLine();
                #endregion

                //if (newMethod.ToString().Contains("ref useSiteDiagnostics:"))
                //    ;

                return newMethod;
            }
            catch (Exception ex)
            {
                return null;
                //throw;
            }
        }

        public static MethodDeclarationSyntax EnterExpressionInvocationWrapped(SyntaxNode modifiedNode, SemanticModel model, SyntaxNode originalNode, int sourceFileId)
        {
            try
            {
                // 1) Relacionado con el tipo y el nombre del método nuevo
                var operation = model.GetOperation(originalNode);
                var returnType = GetTypeName(operation.Type);
                var name = string.Format("f_{0}_{1}_{2}", sourceFileId, originalNode.Span.Start, originalNode.Span.End);

                var newMethod = SyntaxFactory.MethodDeclaration(SyntaxFactory.IdentifierName(SyntaxFactory.Identifier(returnType)).AddPostLine(), name);
                newMethod = newMethod.WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxFactory.TriviaList(), SyntaxKind.StaticKeyword, SyntaxFactory.TriviaList(SyntaxFactory.Space))));

                var parameterList = new SeparatedSyntaxList<ParameterSyntax>();
                // Si es invocación y no es estática, tiene "this". OK
                var thisType = returnType;
                parameterList = parameterList.Add(SyntaxFactory.Parameter
                    (new SyntaxList<AttributeListSyntax>(), new SyntaxTokenList(),
                    SyntaxFactory.IdentifierName(SyntaxFactory.Identifier(thisType)).AddPostLine(), SyntaxFactory.Identifier("a"), null));
                newMethod = newMethod.WithParameterList(SyntaxFactory.ParameterList(parameterList));

                // 3) Invocaciones reales y envío de traza
                SyntaxList<StatementSyntax> stmtList = new SyntaxList<StatementSyntax>();
                stmtList = stmtList.Add(SyntaxFactory.ParseStatement(string.Format("{0}{1}.{5}({2}, {3}, {4});",
                    NamespaceName, TraceType, sourceFileId, originalNode.Span.Start, originalNode.Span.End, "TraceEnterExpression")).AddPreLine());
                stmtList = stmtList.Add(SyntaxFactory.ParseStatement("return a;").AddPreLine());
                newMethod = newMethod.WithBody(SyntaxFactory.Block(stmtList).AddPreAndPostLine()).AddPreAndPostLine();
                return newMethod;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public static BinaryExpressionSyntax EnterExpressionInvocationWrapped_V2(ExpressionSyntax left, ExpressionSyntax right, SemanticModel model, BinaryExpressionSyntax originalNode, int sourceFileId)
        {
            try
            {
                // In each case we apply different modifications:
                // X ?? Y ==> X ?? (F_NULL ?? Y)
                // X && Y ==> X && (F_TRUE && Y)
                // X || Y ==> X || (F_FALSE || Y)

                var op = "";
                var kind = originalNode.Kind();
                switch (kind)
                {
                    case SyntaxKind.CoalesceExpression:
                        op = string.Format("Expression_Null<{0}>", model.GetOperation(originalNode.Left.RemoveParentesisAndMore()).Type.ToString());
                        break;
                    case SyntaxKind.LogicalOrExpression:
                        op = "Expression_False";
                        break;
                    case SyntaxKind.LogicalAndExpression:
                        op = "Expression_True";
                        break;
                    default:
                        throw new NotImplementedException();
                }
                var exp = SyntaxFactory.ParseExpression(string.Format("{5}{4}.{3}({0}, {1}, {2})", sourceFileId, originalNode.Span.Start, originalNode.Span.End, op, TraceType, NamespaceName));
                var newRight = SyntaxFactory.ParenthesizedExpression(SyntaxFactory.BinaryExpression(kind, exp, right));
                var newBinary = SyntaxFactory.BinaryExpression(kind, left, newRight);
                return newBinary;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public static ConditionalExpressionSyntax ConditionalOperatorExpression_V2(ConditionalExpressionSyntax rewritedNode, ConditionalExpressionSyntax originalNode, SemanticModel model, int sourceFileId)
        {
            // BEFORE: a ? b : c
            // AFTER: ((F)||((a ^ T) || (F)) ? b : c)

            // CALL F1, F2 y F3:
            var call_f1 = SyntaxFactory.ParseExpression(string.Format("{4}{3}.Conditional_F1({0}, {1}, {2})", sourceFileId, originalNode.Condition.Span.Start, originalNode.Condition.Span.End, TraceType, NamespaceName));
            var call_f2 = SyntaxFactory.ParseExpression(string.Format("{4}{3}.Conditional_F2({0}, {1}, {2})", sourceFileId, originalNode.WhenTrue.Span.Start, originalNode.WhenTrue.Span.End, TraceType, NamespaceName));
            var call_f3 = SyntaxFactory.ParseExpression(string.Format("{4}{3}.Conditional_F3({0}, {1}, {2})", sourceFileId, originalNode.WhenFalse.Span.Start, originalNode.WhenFalse.Span.End, TraceType, NamespaceName));

            var expresion_a_T = SyntaxFactory.ParenthesizedExpression(SyntaxFactory.BinaryExpression(SyntaxKind.LogicalAndExpression, rewritedNode.Condition, call_f2));
            var expresion_T_or_F = SyntaxFactory.ParenthesizedExpression(SyntaxFactory.BinaryExpression(SyntaxKind.LogicalOrExpression, expresion_a_T, call_f3));
            var expresion_F_or_other = SyntaxFactory.ParenthesizedExpression(SyntaxFactory.BinaryExpression(SyntaxKind.LogicalOrExpression, call_f1, expresion_T_or_F));

            return SyntaxFactory.ConditionalExpression(expresion_F_or_other, rewritedNode.WhenTrue, rewritedNode.WhenFalse);
        }

        public static SyntaxNode ConditionalAccessCheckNull(ConditionalAccessExpressionSyntax originalNode, SyntaxNode modifiedExpression, int sourceFileId)
        {
            return SyntaxFactory.ParseExpression(string.Format("{5}{4}.TraceConditionalAccessExpression({0}, {1}, {2}, {3})",
                modifiedExpression.ToString(), sourceFileId, originalNode.Span.Start, originalNode.Span.End, TraceType, NamespaceName));
        }

        public static Tuple<ExpressionSyntax, MethodDeclarationSyntax> ConditionalAccessExpression(ConditionalAccessExpressionSyntax rewritedNode, ConditionalAccessExpressionSyntax originalNode, SemanticModel model, int sourceFileId)
        {
            // BEFORE: a?.b
            // AFTER: function(a)?.b

            //var t =  (ConditionalAccessExpressionSyntax)SyntaxFactory.ParseExpression(string.Format("{6}{5}.TraceConditionalAccessExpression({0}, {2}, {3}, {4})?{1}",
            //    rewritedNode.Expression.ToString(), rewritedNode.WhenNotNull.ToString(),
            //    sourceFileId, originalNode.Span.Start, originalNode.Span.End, TraceType, NamespaceName));

            AccessType a = AccessType.Member;
            var operation = model.GetOperation(originalNode.WhenNotNull);
            var traceConditionalAccessExpression = string.Format("{6}{5}.TraceConditionalAccessExpression({0}, {2}, {3}, {4})?{1}",
                    rewritedNode.Expression.ToString(), rewritedNode.WhenNotNull.ToString(),
                    sourceFileId, originalNode.Span.Start, originalNode.Span.End, TraceType, NamespaceName);
            // TODO XXX This is not good. First: its awful. Second: it has to be recursive
            if (operation.Kind == OperationKind.PropertyReference ||
                operation.Kind == OperationKind.ArrayElementReference ||
               (operation.Kind == OperationKind.ConditionalAccess /* XXX CHEQUEAR && ((IConditionalAccessOperation)operation).Operation.Kind == OperationKind.PropertyReference*/)
               // TODO: default when we can't get the operation
               || operation.Kind == OperationKind.Invalid)
            { 
                var method = ExpressionWrapped(originalNode, model, sourceFileId, a, null);
                var exp = SyntaxFactory.ParseExpression(method.Identifier.ValueText + "(" + traceConditionalAccessExpression + ")");
                return new Tuple<ExpressionSyntax, MethodDeclarationSyntax>(exp, method);
            }
            else if (operation.Kind == OperationKind.FieldReference)
                return new Tuple<ExpressionSyntax, MethodDeclarationSyntax>(SyntaxFactory.ParseExpression(traceConditionalAccessExpression), null);
            else
                throw new NotSupportedException("SLICER");
        }

        public static Tuple<ExpressionSyntax, MethodDeclarationSyntax> ConditionalAccessExpressionReceiver(ConditionalAccessExpressionSyntax rewritedNode, ConditionalAccessExpressionSyntax originalNode, SemanticModel model, int sourceFileId)
        {
            // BEFORE: a?.b
            // AFTER: function(a)?.b

            //var t =  (ConditionalAccessExpressionSyntax)SyntaxFactory.ParseExpression(string.Format("{6}{5}.TraceConditionalAccessExpression({0}, {2}, {3}, {4})?{1}",
            //    rewritedNode.Expression.ToString(), rewritedNode.WhenNotNull.ToString(),
            //    sourceFileId, originalNode.Span.Start, originalNode.Span.End, TraceType, NamespaceName));

            AccessType a = AccessType.Member;
            var operation = model.GetOperation(originalNode.WhenNotNull);
            var traceConditionalAccessExpression = string.Format("{5}{4}.TraceConditionalAccessExpression({0}, {1}, {2}, {3})",
                    rewritedNode.Expression.ToString(), sourceFileId, originalNode.Span.Start, originalNode.Span.End, TraceType, NamespaceName);
            // TODO XXX This is not good. First: its awful. Second: it has to be recursive
            if (operation.Kind == OperationKind.PropertyReference ||
                operation.Kind == OperationKind.ArrayElementReference ||
               (operation.Kind == OperationKind.ConditionalAccess /* XXX CHEQUEAR && ((IConditionalAccessOperation)operation).Operation.Kind == OperationKind.PropertyReference*/)
               // TODO: default when we can't get the operation
               || operation.Kind == OperationKind.Invalid)
            {
                var method = ExpressionWrapped(originalNode, model, sourceFileId, a, null);
                var exp = SyntaxFactory.ParseExpression(method.Identifier.ValueText + "(" + traceConditionalAccessExpression + ")");
                return new Tuple<ExpressionSyntax, MethodDeclarationSyntax>(exp, method);
            }
            else if (operation.Kind == OperationKind.FieldReference)
                return new Tuple<ExpressionSyntax, MethodDeclarationSyntax>(SyntaxFactory.ParseExpression(traceConditionalAccessExpression), null);
            else
                throw new NotSupportedException("SLICER");
        }

        public static MethodDeclarationSyntax ExpressionWrapped(SyntaxNode originalNode, SemanticModel model, int sourceFileId, AccessType accessType, SyntaxNode refenceToNode = null)
        {
            var operation = model.GetOperation(originalNode);
            if (operation == null)
            {
                if (originalNode is ArgumentSyntax)
                {
                    operation = model.GetOperation(((ArgumentSyntax)originalNode).Expression.RemoveParentesisAndMore());
                    if (operation == null)
                        ;

                    // The most usual scenario is for avoiding nullable warning expressions!
                    // if (operation == null && ((ArgumentSyntax)originalNode).Expression is PostfixUnaryExpressionSyntax && ((PostfixUnaryExpressionSyntax)((ArgumentSyntax)originalNode).Expression).OperatorToken.ValueText == "!")
                    //    operation = model.GetOperation(((PostfixUnaryExpressionSyntax)((ArgumentSyntax)originalNode).Expression).Operand);
                }
                else
                    ;
            }

            if (operation == null)
                ;

            ITypeSymbol returnType;
            if (operation is IArgumentOperation)
            {
                operation = ((IArgumentOperation)operation).Value;
                returnType = operation.Type ?? ((IArgumentOperation)operation).Parameter.Type;
            }
            else
                returnType = operation.Type;

            if (refenceToNode == null)
                refenceToNode = originalNode;

            var traceMethod = "";
            var add_to_name = "";
            switch (accessType)
            {
                case AccessType.Invocation:
                    traceMethod = "TraceEndInvocation";
                    add_to_name = "I";
                    break;
                case AccessType.Member:
                    traceMethod = "TraceEndMemberAccess";
                    add_to_name = "M";
                    break;
                case AccessType.Base:
                    traceMethod = "TraceBaseCall";
                    add_to_name = "C";
                    break;
            }

            var name = string.Format("f_{0}_{1}_{2}_{3}", sourceFileId, originalNode.Span.Start, originalNode.Span.End, add_to_name);


            var typeParameters = new HashSet<string>();
            ExtractTypeParameters(returnType, typeParameters, originalNode, false);
            var returnTypeName = GetTypeName(returnType);
            var parameterList = new SeparatedSyntaxList<ParameterSyntax>();
            parameterList = parameterList.Add(SyntaxFactory.Parameter(
                new SyntaxList<AttributeListSyntax>(), new SyntaxTokenList(),
                SyntaxFactory.ParseTypeName(returnTypeName).AddPostLine(),
                SyntaxFactory.Identifier("i"), null));
            
            var willBeStatic = ((originalNode.GetContainer() is ClassDeclarationSyntax || originalNode.GetContainer() is StructDeclarationSyntax) && originalNode.StaticContainer()) || add_to_name == "C";
            if (!willBeStatic && (typeParameters.Count > 0))
            {
                var to_exclude = originalNode.GetTypeParametersOfContainer();
                if (to_exclude != null)
                    foreach (var exc in to_exclude)
                        typeParameters.Remove(exc);
            }

            if (typeParameters.Count > 0)
                name = name + "<" + string.Join(", ", typeParameters) + ">";

            var newMethod = SyntaxFactory.MethodDeclaration(SyntaxFactory.ParseTypeName(returnTypeName).AddPostLine(), name);
            if (willBeStatic)
                newMethod = newMethod.WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxFactory.TriviaList(), SyntaxKind.StaticKeyword, SyntaxFactory.TriviaList(SyntaxFactory.Space))));
            newMethod = newMethod.WithParameterList(SyntaxFactory.ParameterList(parameterList));

            if (typeParameters.Count > 0)
            {
                var t = originalNode.GetContainer();
                var clauses = new SyntaxList<TypeParameterConstraintClauseSyntax>();
                if (t is MethodDeclarationSyntax)
                    clauses = ((MethodDeclarationSyntax)t).ConstraintClauses;
                t = t.Parent.GetContainer();
                if (t is ClassDeclarationSyntax)
                    clauses = clauses.AddRange(((ClassDeclarationSyntax)t).ConstraintClauses);
                if (t is StructDeclarationSyntax)
                    clauses = clauses.AddRange(((StructDeclarationSyntax)t).ConstraintClauses);
                newMethod = newMethod.WithConstraintClauses(new SyntaxList<TypeParameterConstraintClauseSyntax>(clauses.Where(x => typeParameters.Contains(x.Name.GetText().ToString().Trim()))));
            }

            SyntaxList<StatementSyntax> stmtList = new SyntaxList<StatementSyntax>();
            stmtList = stmtList.Add(SyntaxFactory.ParseStatement("var return_v = i;").AddPreLine());
            stmtList = stmtList.Add(SyntaxFactory.ParseStatement(string.Format("{0}{1}.{5}({2}, {3}, {4});",
                NamespaceName, TraceType, sourceFileId, refenceToNode.Span.Start, refenceToNode.Span.End, traceMethod)).AddPreLine());
            stmtList = stmtList.Add(SyntaxFactory.ParseStatement("return return_v;").AddPreAndPostLine());
            newMethod = newMethod.WithBody(SyntaxFactory.Block(stmtList).AddPreAndPostLine()).AddPreAndPostLine();
            return newMethod;
        }

        static void ExtractTypeParameters(ITypeSymbol symbol, ISet<string> types_set, SyntaxNode originalNode, bool is_returned_type)
        {
            if (symbol != null && symbol is ITypeSymbol && (!is_returned_type || !(originalNode is IdentifierNameSyntax)))
            {
                if (((ITypeSymbol)symbol).Kind == SymbolKind.ArrayType &&
                    ((IArrayTypeSymbol)symbol).ElementType.Kind == SymbolKind.TypeParameter)
                    types_set.Add(GetTypeName(((IArrayTypeSymbol)symbol).ElementType));
                if (((ITypeSymbol)symbol).Kind == SymbolKind.TypeParameter)
                    types_set.Add(GetTypeName(((ITypeParameterSymbol)symbol)).Replace("?", ""));
                if (((ITypeSymbol)symbol).Kind == SymbolKind.NamedType &&
                    ((INamedTypeSymbol)symbol).TypeArguments.Count() > 0 &&
                    ((INamedTypeSymbol)symbol).TypeArguments.Any(x => x.Kind == SymbolKind.TypeParameter))
                    foreach (var typeArg in ((INamedTypeSymbol)symbol).TypeArguments)
                    {
                        if (typeArg.Kind == SymbolKind.TypeParameter)
                            types_set.Add(GetTypeName(typeArg));
                        else
                            ExtractTypeParameters(typeArg, types_set, originalNode, is_returned_type);
                    }
                        
            }
        }

        static string GetTypeName(ITypeSymbol type)
        {
            if (type == null)
                ;

            // TODO ROSLYN: For avoiding the "?" type in parameters
            if (type.ToString() == "?")
                return "object";

            if (type.IsAnonymousType)
                return "dynamic";

            var t = type.ToString();
            if (t.Contains("anonymous type"))
            {
                var f_char = t.IndexOf("anonymous type");
                var i_char = f_char;
                var cant_b = 0;
                var n_char = t[f_char];
                while (n_char != '>' || cant_b == 0)
                {
                    f_char++;
                    n_char = t[f_char];
                    if (n_char == '<')
                        cant_b++;
                    if (n_char == '>')
                        cant_b--;
                }
                t = t.Substring(0, i_char - 1) + "dynamic" + t.Substring(f_char + 1);
            }

            // If the last is * we let it there...
            // We just want to remove [*,*]
            return t.Replace("*", "") + (t.Last() == '*' ? new string('*', Utils.CountAsterix(t)) : "");
        } 

        public enum AccessType
        {
            Invocation = 1,
            Member = 2,
            Base = 3
        }
    }
}