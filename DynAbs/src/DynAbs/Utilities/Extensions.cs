using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DynAbs
{
    public static class Extensions
    {
        public static bool IsIndexer(this IPropertyReferenceOperation operation)
        {
            return operation.Arguments.Count() > 0;
        }

        public static bool IsInSource(this IOperation operation)
        {
            if (operation.Kind == OperationKind.ObjectCreation)
                return (((IObjectCreationOperation)operation).Constructor.Locations.All(x => x.IsInSource));
            if (operation.Kind == OperationKind.Invocation)
                if (((IInvocationOperation)operation).TargetMethod.MethodKind == MethodKind.DelegateInvoke)
                    return false;
                else
                    return (((IInvocationOperation)operation).TargetMethod.Locations.All(x => x.IsInSource));
            if (operation.Kind == OperationKind.PropertyReference)
                return (((IPropertyReferenceOperation)operation).Property.Locations.All(x => x.IsInSource));

            return true;
        }

        public static bool IsNotScalar(this ITypeSymbol typeSymbol, bool stringAsScalar = true, bool structsAsScalar = false)
        {
            if ((typeSymbol.TypeKind == TypeKind.Struct || typeSymbol.IsValueType))
                return !structsAsScalar && !Utils.HasDefaultValue(typeSymbol);
            return (typeSymbol.IsReferenceType || typeSymbol.Kind == SymbolKind.TypeParameter) && (!stringAsScalar || !typeSymbol.Name.ToLower().Equals("string"));
        }

        public static bool CustomIsStruct(this ITypeSymbol typeSymbol) 
        {
            return typeSymbol.TypeKind == TypeKind.Struct && !Utils.HasDefaultValue(typeSymbol);
        }

        public static bool CustomStructReceiver(this IMethodSymbol methodSymbol)
        {
            return methodSymbol.ReceiverType.CustomIsStruct() || 
                (methodSymbol.IsExtensionMethod &&
                ((methodSymbol.Parameters.Count() > 0 &&
                methodSymbol.Parameters.First().Type.CustomIsStruct()) || 
                (methodSymbol.ReceiverType.CustomIsStruct())));
        }

        public static bool CustomStructReceiver(this IOperation operation)
        {
            return operation is IPropertyReferenceOperation && ((IPropertyReferenceOperation)operation).Property.GetMethod.ReceiverType.CustomIsStruct();
        }

        public static ITypeSymbol CustomGetType(this IOperation operation)
        {
            if (operation is IConversionOperation)
                return ((IConversionOperation)operation).Operand.Type;
            return operation.Type;
        }

        public static int GetParametersCount(this CSharpSyntaxNode node)
        {
            var parametersCount = 0;
            if (node is ConstructorDeclarationSyntax)
            {
                var constructor = node as ConstructorDeclarationSyntax;
                parametersCount = constructor.ParameterList.Parameters.Count;
            }
            if (node is MethodDeclarationSyntax)
            {
                MethodDeclarationSyntax method = node as MethodDeclarationSyntax;
                parametersCount = method.ParameterList.Parameters.Count;
            }
            return parametersCount;
        }

        public static Tuple<int, int> GetParametersRangeCount(this CSharpSyntaxNode node)
        {
            var parametersInit = 0;
            var parametersEnd = 0;
            if (node is ConstructorDeclarationSyntax)
            {
                var constructor = node as ConstructorDeclarationSyntax;
                parametersInit = constructor.ParameterList.Parameters.Where(x => x.Default == null).Count();
                parametersEnd = constructor.ParameterList.Parameters.Count;
            }
            if (node is MethodDeclarationSyntax)
            {
                MethodDeclarationSyntax method = node as MethodDeclarationSyntax;
                parametersInit = method.ParameterList.Parameters.Where(x => x.Default == null).Count();
                parametersEnd = method.ParameterList.Parameters.Count;
            }
            if (node is LocalFunctionStatementSyntax)
            {
                LocalFunctionStatementSyntax localFunction = node as LocalFunctionStatementSyntax;
                parametersInit = localFunction.ParameterList.Parameters.Where(x => x.Default == null).Count();
                parametersEnd = localFunction.ParameterList.Parameters.Count;
            }
            return new Tuple<int, int>(parametersInit, parametersEnd);
        }

        public static TypeSyntax ReturnTypeSyntax(this CSharpSyntaxNode syntaxNode)
        {
            if (syntaxNode is MethodDeclarationSyntax)
                return ((MethodDeclarationSyntax)syntaxNode).ReturnType;
            if (syntaxNode is ArrowExpressionClauseSyntax && syntaxNode.Parent is MethodDeclarationSyntax)
                return ((MethodDeclarationSyntax)syntaxNode.Parent).ReturnType;
            if (syntaxNode is LocalFunctionStatementSyntax)
                return ((LocalFunctionStatementSyntax)syntaxNode).ReturnType;
            if (syntaxNode is ArrowExpressionClauseSyntax && syntaxNode.Parent is LocalFunctionStatementSyntax)
                return ((LocalFunctionStatementSyntax)syntaxNode.Parent).ReturnType;
            if (syntaxNode is OperatorDeclarationSyntax)
                return ((OperatorDeclarationSyntax)syntaxNode).ReturnType;
            if (syntaxNode is ConversionOperatorDeclarationSyntax)
                return ((ConversionOperatorDeclarationSyntax)syntaxNode).Type;
            if (syntaxNode is ArrowExpressionClauseSyntax && syntaxNode.Parent is ConversionOperatorDeclarationSyntax)
                return ((ConversionOperatorDeclarationSyntax)syntaxNode.Parent).Type;
            if (syntaxNode is ArrowExpressionClauseSyntax && syntaxNode.Parent is PropertyDeclarationSyntax)
                return ((PropertyDeclarationSyntax)syntaxNode.Parent).Type;
            if (syntaxNode is ArrowExpressionClauseSyntax && syntaxNode.Parent is AccessorDeclarationSyntax
                && syntaxNode.Parent.Parent.Parent is PropertyDeclarationSyntax propertyDeclarationSyntax)
                return propertyDeclarationSyntax.Type;
            if (syntaxNode is AccessorDeclarationSyntax &&
                ((AccessorDeclarationSyntax)syntaxNode).Keyword.ValueText.Equals("set", StringComparison.OrdinalIgnoreCase))
                return null;
            else if (syntaxNode is ArrowExpressionClauseSyntax && syntaxNode.Parent is AccessorDeclarationSyntax &&
                ((AccessorDeclarationSyntax)syntaxNode.Parent).Keyword.ValueText.Equals("set", StringComparison.OrdinalIgnoreCase))
                return null;
            if (syntaxNode is AccessorDeclarationSyntax &&
                ((AccessorDeclarationSyntax)syntaxNode).Keyword.ValueText.Equals("get", StringComparison.OrdinalIgnoreCase) &&
                ((AccessorDeclarationSyntax)syntaxNode).Parent.Parent is IndexerDeclarationSyntax)
                return ((IndexerDeclarationSyntax)(((AccessorDeclarationSyntax)syntaxNode).Parent.Parent)).Type;
            if (syntaxNode is AccessorDeclarationSyntax &&
                ((AccessorDeclarationSyntax)syntaxNode).Keyword.ValueText.Equals("get", StringComparison.OrdinalIgnoreCase) &&
                ((AccessorDeclarationSyntax)syntaxNode).Parent.Parent is PropertyDeclarationSyntax)
                return ((PropertyDeclarationSyntax)(((AccessorDeclarationSyntax)syntaxNode).Parent.Parent)).Type;
            if (syntaxNode is ConstructorDeclarationSyntax)
                return null;
            if (syntaxNode is ClassDeclarationSyntax || syntaxNode is StructDeclarationSyntax)
                return null;
            throw new SlicerException("Slicer: Unexpected expression");
        }

        public static bool CustomIsStatic(this VariableDeclaratorSyntax node)
        {
            return ((FieldDeclarationSyntax)node.Parent.Parent).Modifiers.Any(x => x.ToString() == "static" || x.ToString() == "const");
        }

        public static string CustomGetName(this MethodDeclarationSyntax node)
        {
            return node.Identifier.ValueText.Contains('<') ?
                node.Identifier.ValueText.Substring(0, node.Identifier.ValueText.IndexOf('<')) : node.Identifier.ValueText;
        }

        public static string CustomGetName(this IParameterSymbol parameter)
        {
            if (parameter.Name == "this")
                return "@this";
            return parameter.Name;
        }

        public static ISet<string> GetTypeParameters(this MethodDeclarationSyntax node)
        {
            if (node.TypeParameterList == null || node.TypeParameterList.Parameters == null)
                return new HashSet<string>();

            var allTypes = new HashSet<string>(
                node.TypeParameterList.Parameters.Select(x => x.Identifier.ValueText));
            foreach (var param in node.ParameterList.Parameters)
                if (param.Type is GenericNameSyntax && ((GenericNameSyntax)param.Type).TypeArgumentList.Arguments.Count > 0)
                    FillTypeParameters((GenericNameSyntax)param.Type, allTypes);
            return allTypes;
        }

        public static void FillTypeParameters(GenericNameSyntax node, ISet<string> set)
        {
            var stack = new Stack<GenericNameSyntax>();
            stack.Push(node);
            while (stack.Count > 0)
            {
                var genericNameSyntax = stack.Pop();
                foreach (var typeArg in genericNameSyntax.TypeArgumentList.Arguments)
                {
                    if (typeArg is IdentifierNameSyntax)
                        set.Add(((IdentifierNameSyntax)typeArg).Identifier.ValueText);
                    else if (typeArg is GenericNameSyntax)
                        stack.Push((GenericNameSyntax)typeArg);
                }
            }
        }

        public static T AddSpace<T>(this T node, int c = 1) where T : CSharpSyntaxNode
        {
            SyntaxTriviaList t = node.GetLeadingTrivia();
            for (var i = 0; i < c; i++)
                t = t.Add(SyntaxFactory.Whitespace(" "));
            return node.WithLeadingTrivia(t);
        }

        public static T AddTabs<T>(this T node, int c = 2) where T : CSharpSyntaxNode
        {
            SyntaxTriviaList t = node.GetLeadingTrivia();
            for (var i = 0; i < c; i++)
                t = t.Add(SyntaxFactory.Tab);
            return node.WithLeadingTrivia(t);
        }

        public static SyntaxToken AddTabs(this SyntaxToken node, int c = 2)
        {
            SyntaxTriviaList t = node.LeadingTrivia;
            for (var i = 0; i < c; i++)
                t = t.Add(SyntaxFactory.Tab);
            return node.WithLeadingTrivia(t);
        }

        public static T AddPostLine<T>(this T node) where T : CSharpSyntaxNode
        {
            SyntaxTriviaList t = node.GetTrailingTrivia();
            t = t.Add(SyntaxFactory.Whitespace("\n"));
            return node.WithTrailingTrivia(t);
        }

        public static SyntaxToken AddPostLine(this SyntaxToken node) 
        {
            SyntaxTriviaList t = node.TrailingTrivia;
            t = t.Add(SyntaxFactory.Whitespace("\n"));
            return node.WithTrailingTrivia(t);
        }

        public static T AddPreLine<T>(this T node) where T : CSharpSyntaxNode
        {
            SyntaxTriviaList t = node.GetLeadingTrivia();
            t = t.Add(SyntaxFactory.Whitespace("\n"));
            return node.WithLeadingTrivia(t);
        }

        public static T AddPreAndPostLine<T>(this T node) where T : CSharpSyntaxNode
        {
            return node.AddPreLine().AddPostLine();
        }

        public static BlockSyntax AddTabs(this BlockSyntax node)
        {
            node = node.WithOpenBraceToken(SyntaxFactory.Token(SyntaxKind.OpenBraceToken).AddTabs());
            var t = new SyntaxList<StatementSyntax>();
            foreach (var s in node.Statements)
                t = t.Add(s.AddTabs(3));
            node = node.WithCloseBraceToken(SyntaxFactory.Token(SyntaxKind.CloseBraceToken).AddTabs());
            return node.WithStatements(t);
        }

        public static SyntaxNode GetWhenContainer(this SyntaxNode node)
        {
            var p_node = node;
            while (!(p_node is WhenClauseSyntax ||
                     p_node == null))
            {
                p_node = p_node.Parent;
            }
            return p_node;
        }

        public static SyntaxNode GetCaseContainer(this SyntaxNode node)
        {
            var p_node = node;
            while (!(p_node is SwitchSectionSyntax ||
                     p_node == null))
            {
                p_node = p_node.Parent;
            }
            return p_node;
        }

        public static SyntaxNode GetContainerOrConstructorInitializerSyntax(this SyntaxNode node)
        {
            var p_node = node;
            while (!(p_node is MethodDeclarationSyntax ||
                     p_node is LocalFunctionStatementSyntax ||
                     p_node is ConstructorDeclarationSyntax ||
                     p_node is PropertyDeclarationSyntax ||
                     //p_node is FieldDeclarationSyntax ||
                     p_node is EventDeclarationSyntax ||
                     p_node is IndexerDeclarationSyntax ||
                     p_node is DestructorDeclarationSyntax ||
                     p_node is OperatorDeclarationSyntax ||
                     p_node is ConversionOperatorDeclarationSyntax ||
                     p_node is ClassDeclarationSyntax ||
                     p_node is StructDeclarationSyntax ||
                     // CONSTRUCTOR INITIALIZER SYNTAX (for using static inside these structures)
                     p_node is ConstructorInitializerSyntax ||
                     p_node == null))
            {
                p_node = p_node.Parent;
            }

            if (p_node == null)
                Console.WriteLine("");
            return p_node;
        }

        public static SyntaxNode GetContainer(this SyntaxNode node)
        {
            var p_node = node;
            while (!(p_node is MethodDeclarationSyntax ||
                     p_node is LocalFunctionStatementSyntax ||
                     p_node is ConstructorDeclarationSyntax || 
                     p_node is PropertyDeclarationSyntax ||
                     //p_node is FieldDeclarationSyntax ||
                     p_node is EventDeclarationSyntax ||
                     p_node is IndexerDeclarationSyntax ||
                     p_node is DestructorDeclarationSyntax ||
                     p_node is OperatorDeclarationSyntax ||
                     p_node is ConversionOperatorDeclarationSyntax ||
                     p_node is ClassDeclarationSyntax ||
                     p_node is StructDeclarationSyntax ||
                     p_node == null))
            {
                p_node = p_node.Parent;
            }

            if (p_node == null)
                Console.WriteLine("");
            return p_node;
        }

        public static SyntaxNode GetStatementContainer(this SyntaxNode node)
        {
            var p_node = node;
            while (!(p_node is StatementSyntax ||
                     p_node == null))
            {
                p_node = p_node.Parent;
            }

            if (p_node == null)
                return node;
            return p_node;
        }

        public static SyntaxNode GetContainerClass(this SyntaxNode node)
        {
            var p_node = node;
            while (!(p_node is ClassDeclarationSyntax ||
                     p_node is StructDeclarationSyntax ||
                     p_node is RecordDeclarationSyntax ||
                     p_node == null))
            {
                p_node = p_node.Parent;
            }

            return p_node;
        }

        public static IEnumerable<string> GetTypeParametersOfContainer(this SyntaxNode node)
        {
            List<string> returnList = null;
            var containerClassOrStruct = node.GetContainerClass();
            if (containerClassOrStruct is ClassDeclarationSyntax)
                returnList = ((ClassDeclarationSyntax)containerClassOrStruct).TypeParameterList?.Parameters.Select(x => x.Identifier.ValueText).ToList();
            if (containerClassOrStruct is StructDeclarationSyntax)
                returnList = ((StructDeclarationSyntax)containerClassOrStruct).TypeParameterList?.Parameters.Select(x => x.Identifier.ValueText).ToList();

            var container = node.GetContainer();
            if (container is MethodDeclarationSyntax)
            {
                var methodTypeParameters = ((MethodDeclarationSyntax)container).GetTypeParameters();
                if (methodTypeParameters.Count > 0)
                {
                    returnList ??= new List<string>();
                    returnList.AddRange(methodTypeParameters);
                }
            }

            if (containerClassOrStruct != null && containerClassOrStruct.Parent != null)
            {
                var recList = containerClassOrStruct.Parent.GetTypeParametersOfContainer();
                if (returnList != null && recList != null)
                    returnList.AddRange(recList);
                else if (recList != null)
                    return recList;
            }
            return returnList;
        }

        public static bool StaticContainer(this SyntaxNode node)
        {
            var container = node;
            while (!(container is MethodDeclarationSyntax ||
                     container is LocalFunctionStatementSyntax ||
                     container is ConstructorDeclarationSyntax ||
                     container is PropertyDeclarationSyntax ||
                     container is FieldDeclarationSyntax ||
                     container is PropertyDeclarationSyntax ||
                     container is EventDeclarationSyntax ||
                     container is IndexerDeclarationSyntax ||
                     container is DestructorDeclarationSyntax ||
                     container is OperatorDeclarationSyntax ||
                     container is ConversionOperatorDeclarationSyntax ||
                     container is ConstructorInitializerSyntax ||
                     container == null))
            {
                // Initializers in containers are static
                if ((container.Parent is PropertyDeclarationSyntax) && (((PropertyDeclarationSyntax)container.Parent).Initializer != null) && ((PropertyDeclarationSyntax)container.Parent).Initializer == container)
                    return true;

                container = container.Parent;
            }

            if (container is MethodDeclarationSyntax)
                return (((MethodDeclarationSyntax)container).Modifiers.Select(x => x.ValueText).Contains(SyntaxFactory.Token(SyntaxKind.StaticKeyword).ValueText));
            if (container is LocalFunctionStatementSyntax)
                return (((LocalFunctionStatementSyntax)container).Modifiers.Select(x => x.ValueText).Contains(SyntaxFactory.Token(SyntaxKind.StaticKeyword).ValueText));
            if (container is ConstructorDeclarationSyntax)
                return !(((ConstructorDeclarationSyntax)container).Initializer?.ThisOrBaseKeyword != null);
            if (container is FieldDeclarationSyntax)
                return (((FieldDeclarationSyntax)container).Modifiers.Select(x => x.ValueText).Contains(SyntaxFactory.Token(SyntaxKind.StaticKeyword).ValueText));
            if (container is PropertyDeclarationSyntax)
                return (((PropertyDeclarationSyntax)container).Modifiers.Select(x => x.ValueText).Contains(SyntaxFactory.Token(SyntaxKind.StaticKeyword).ValueText));
            if (container is EventDeclarationSyntax)
                return (((EventDeclarationSyntax)container).Modifiers.Select(x => x.ValueText).Contains(SyntaxFactory.Token(SyntaxKind.StaticKeyword).ValueText));
            if (container is IndexerDeclarationSyntax)
                return (((IndexerDeclarationSyntax)container).Modifiers.Select(x => x.ValueText).Contains(SyntaxFactory.Token(SyntaxKind.StaticKeyword).ValueText));
            if (container is DestructorDeclarationSyntax)
                return false;
            if (container is OperatorDeclarationSyntax)
                return (((OperatorDeclarationSyntax)container).Modifiers.Select(x => x.ValueText).Contains(SyntaxFactory.Token(SyntaxKind.StaticKeyword).ValueText));
            if (container is ConversionOperatorDeclarationSyntax)
                return true;
            if (container is ConstructorInitializerSyntax)
                return true;
            throw new NotImplementedException();
        }

        public static bool IsStatic(this SyntaxNode container)
        {
            if (container is ClassDeclarationSyntax)
                return (((ClassDeclarationSyntax)container).Modifiers.Select(x => x.ValueText).Contains(SyntaxFactory.Token(SyntaxKind.StaticKeyword).ValueText));
            if (container is StructDeclarationSyntax)
                return (((StructDeclarationSyntax)container).Modifiers.Select(x => x.ValueText).Contains(SyntaxFactory.Token(SyntaxKind.StaticKeyword).ValueText));
            if (container is MethodDeclarationSyntax)
                return (((MethodDeclarationSyntax)container).Modifiers.Select(x => x.ValueText).Contains(SyntaxFactory.Token(SyntaxKind.StaticKeyword).ValueText));
            if (container is LocalFunctionStatementSyntax)
                return (((LocalFunctionStatementSyntax)container).Modifiers.Select(x => x.ValueText).Contains(SyntaxFactory.Token(SyntaxKind.StaticKeyword).ValueText));
            if (container is ConstructorDeclarationSyntax)
                return !(((ConstructorDeclarationSyntax)container).Initializer?.ThisOrBaseKeyword != null);
            if (container is FieldDeclarationSyntax)
                return (((FieldDeclarationSyntax)container).Modifiers.Select(x => x.ValueText).Contains(SyntaxFactory.Token(SyntaxKind.StaticKeyword).ValueText));
            if (container is PropertyDeclarationSyntax)
                return (((PropertyDeclarationSyntax)container).Modifiers.Select(x => x.ValueText).Contains(SyntaxFactory.Token(SyntaxKind.StaticKeyword).ValueText));
            // This should be always false
            if (container is EventDeclarationSyntax)
                return (((EventDeclarationSyntax)container).Modifiers.Select(x => x.ValueText).Contains(SyntaxFactory.Token(SyntaxKind.StaticKeyword).ValueText));
            if (container is IndexerDeclarationSyntax)
                return (((IndexerDeclarationSyntax)container).Modifiers.Select(x => x.ValueText).Contains(SyntaxFactory.Token(SyntaxKind.StaticKeyword).ValueText));
            if (container is DestructorDeclarationSyntax)
                return false;
            if (container is OperatorDeclarationSyntax)
                return (((OperatorDeclarationSyntax)container).Modifiers.Select(x => x.ValueText).Contains(SyntaxFactory.Token(SyntaxKind.StaticKeyword).ValueText));
            if (container is ConversionOperatorDeclarationSyntax)
                return true;
            if (container is ConstructorInitializerSyntax)
                return true;
            return false;
        }

        public static SyntaxNode RemoveParentesisAndMore(this SyntaxNode s)
        {
            if (s is ParenthesizedExpressionSyntax)
                return RemoveParentesisAndMore(((ParenthesizedExpressionSyntax)s).Expression);
            if (s is PostfixUnaryExpressionSyntax && ((PostfixUnaryExpressionSyntax)(s)).OperatorToken.ValueText == "!")
                return RemoveParentesisAndMore(((PostfixUnaryExpressionSyntax)s).Operand);
            return s;
        }

        public static string GetName(this SyntaxNode container)
        {
            if (container is MethodDeclarationSyntax)
                return ((MethodDeclarationSyntax)container).Identifier.ValueText;
            if (container is LocalFunctionStatementSyntax)
                return ((LocalFunctionStatementSyntax)container).Identifier.ValueText;
            if (container is ConstructorDeclarationSyntax)
                return ((ConstructorDeclarationSyntax)container).Identifier.ValueText;
            if (container is FieldDeclarationSyntax)
                return ((FieldDeclarationSyntax)container).Declaration.Variables.First().Identifier.ValueText;
            if (container is PropertyDeclarationSyntax)
                return ((PropertyDeclarationSyntax)container).Identifier.ValueText;
            if (container is EventDeclarationSyntax)
                return ((EventDeclarationSyntax)container).Identifier.ValueText;
            if (container is IndexerDeclarationSyntax)
                return "[]";
            if (container is DestructorDeclarationSyntax)
                return "Destructor";
            if (container is OperatorDeclarationSyntax)
                return ((OperatorDeclarationSyntax)container).OperatorToken.ValueText;
            if (container is ConversionOperatorDeclarationSyntax)
                return ((ConversionOperatorDeclarationSyntax)container).Type.ToString();
            if (container is ConstructorInitializerSyntax)
                throw new NotImplementedException();

            if (container is ClassDeclarationSyntax)
                return ((ClassDeclarationSyntax)container).Identifier.ValueText;
            if (container is StructDeclarationSyntax)
                return ((StructDeclarationSyntax)container).Identifier.ValueText;
            if (container is RecordDeclarationSyntax)
                return ((RecordDeclarationSyntax)container).Identifier.ValueText;
            if (container is NamespaceDeclarationSyntax)
                return ((NamespaceDeclarationSyntax)container).Name.ToString();

            throw new NotImplementedException();
        }

        public static List<string> GetAllContainers(this SyntaxNode node)
        {
            var retList = new List<string>();
            var p_node = node;
            while (p_node != null)
            {
                while (!(p_node is MethodDeclarationSyntax ||
                         p_node is LocalFunctionStatementSyntax ||
                         p_node is ConstructorDeclarationSyntax ||
                         p_node is PropertyDeclarationSyntax ||
                         //p_node is FieldDeclarationSyntax ||
                         p_node is EventDeclarationSyntax ||
                         p_node is IndexerDeclarationSyntax ||
                         p_node is DestructorDeclarationSyntax ||
                         p_node is OperatorDeclarationSyntax ||
                         p_node is ConversionOperatorDeclarationSyntax ||
                         p_node is ClassDeclarationSyntax ||
                         p_node is StructDeclarationSyntax ||
                         p_node is NamespaceDeclarationSyntax ||
                         p_node == null))
                {
                    p_node = p_node.Parent;
                }
                if (p_node != null)
                {
                    retList.Add(p_node.GetName());
                    p_node = p_node.Parent;
                }
            }

            retList.Reverse();
            return retList;
        }

        public static List<SyntaxNode> GetParentsUntilContainer(this SyntaxNode node)
        {
            var retList = new List<SyntaxNode>();
            var p_node = node;
            while (!(p_node is MethodDeclarationSyntax ||
                     p_node is LocalFunctionStatementSyntax ||
                     p_node is ConstructorDeclarationSyntax ||
                     p_node is PropertyDeclarationSyntax ||
                     //p_node is FieldDeclarationSyntax ||
                     p_node is EventDeclarationSyntax ||
                     p_node is IndexerDeclarationSyntax ||
                     p_node is DestructorDeclarationSyntax ||
                     p_node is OperatorDeclarationSyntax ||
                     p_node is ConversionOperatorDeclarationSyntax ||
                     p_node is ClassDeclarationSyntax ||
                     p_node is StructDeclarationSyntax ||
                     p_node == null))
            {
                retList.Add(p_node);
                p_node = p_node.Parent;
            }
            return retList;
        }

        public static SyntaxNode GetLoopContainer(this SyntaxNode node)
        {
            var p_node = node;
            while (!(p_node is ForEachStatementSyntax ||
                     p_node is ForStatementSyntax ||
                     p_node is WhileStatementSyntax ||
                     p_node is DoStatementSyntax ||
                     p_node == null))
            {
                p_node = p_node.Parent;
            }
            return p_node;
        }
    }
}
