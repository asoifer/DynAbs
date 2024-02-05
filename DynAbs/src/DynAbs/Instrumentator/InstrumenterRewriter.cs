using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynAbs
{
    public class InstrumenterRewriter : CSharpSyntaxRewriter
    {
        #region Properties and constructor
        SemanticModel Model { get; set; }
        CSharpCompilation Compilation { get; set; }
        private Dictionary<string, int> pathToIdDict;
        public int sourceFileId { get; set; }
        public Dictionary<string, ISet<string>> excludedClassesMethods { get; set; }
        public ISet<string> allowedClasses { get; set; }
        public ISet<string> alreadyVisitedClasses;
        public Stack<Dictionary<int, MethodDeclarationSyntax>> wrapperMethods = new Stack<Dictionary<int, MethodDeclarationSyntax>>();
        bool insideBaseCall = false;
        Func<string, bool> analizingFile;

        public static Dictionary<string, List<string>> notSupportedExpressions = new Dictionary<string, List<string>>();
        public static Dictionary<string, List<string>> skippedMethods = new Dictionary<string, List<string>>();
        public static Dictionary<string, List<string>> infoExpressions = new Dictionary<string, List<string>>();
        public static Dictionary<string, Dictionary<string, string>> methodsToComplete = new Dictionary<string, Dictionary<string, string>>();

        // Usados para instrumentar funciones con parametros ref
        //private ISet<Tuple<string,string>> methodRefOrOutParams = new HashSet<Tuple<string, string>>();

        MethodsTempInfo currentMethodInfo = new MethodsTempInfo();
        const string listYieldReturn = "listYield";

        class MethodsTempInfo
        {
            public IList<StatementSyntax> outParamsInline = new List<StatementSyntax>();
            public ISet<string> outParams = new HashSet<string>();
            public int tmpVariableNumber = 0;
            public StatementSyntax returnYieldReturnStmt = null;

            public MethodsTempInfo()
            {
                outParamsInline = new List<StatementSyntax>();
                outParams = new HashSet<string>();
                tmpVariableNumber = 0;
                returnYieldReturnStmt = null;
            }

            public MethodsTempInfo(MethodsTempInfo methodsTempInfo)
            {
                this.outParamsInline = methodsTempInfo.outParamsInline;
                this.outParams = methodsTempInfo.outParams;
                this.tmpVariableNumber = methodsTempInfo.tmpVariableNumber;
                this.returnYieldReturnStmt = methodsTempInfo.returnYieldReturnStmt;
            }
        }

        public InstrumenterRewriter(int fileId, CSharpCompilation compilation, SyntaxTree tree, Dictionary<string, int> pathToIdDict, Dictionary<string, ISet<string>> excluded, ISet<string> allowedClasses, Func<string, bool> analizingFile)
        {
            Model = compilation.GetSemanticModel(tree);
            Compilation = compilation;
            sourceFileId = fileId;
            this.pathToIdDict = pathToIdDict;
            excludedClassesMethods = excluded;
            this.allowedClasses = allowedClasses;
            alreadyVisitedClasses = new HashSet<string>();
            this.analizingFile = analizingFile;
        }
        #endregion

        #region Visits
        public override SyntaxNode VisitClassDeclaration(ClassDeclarationSyntax originalClassNode)
        {
            if ((excludedClassesMethods.ContainsKey(originalClassNode.Identifier.ValueText) && excludedClassesMethods[originalClassNode.Identifier.ValueText] == null) && (!allowedClasses.Contains(originalClassNode.Identifier.ValueText)))
                return originalClassNode;

            wrapperMethods.Push(new Dictionary<int, MethodDeclarationSyntax>());
            ClassDeclarationSyntax modifiedClassNode = (ClassDeclarationSyntax)base.VisitClassDeclaration(originalClassNode);
            INamedTypeSymbol classSymbol = Model.GetDeclaredSymbol(originalClassNode);
            // Fields
            /* Si un field se inicializa fuera del constructor, se deja solo la declaracion y luego
             * se inicializa en el constructor. */
            SeparatedSyntaxList<StatementSyntax> instanceFieldsSyntaxlist = new SeparatedSyntaxList<StatementSyntax>();
            SeparatedSyntaxList<StatementSyntax> staticFieldsSyntaxlist = new SeparatedSyntaxList<StatementSyntax>();
            IEnumerable<ISymbol> fieldMembers = classSymbol.GetMembers().Where(x => x is IFieldSymbol);
            foreach (var iterationMember in fieldMembers)
            {
                // Si tiene symbol asociado entonces es autoproperty
                ISymbol member = ((IFieldSymbol)iterationMember).AssociatedSymbol != null ?
                    ((IFieldSymbol)iterationMember).AssociatedSymbol : iterationMember;

                var syntaxRef = member.DeclaringSyntaxReferences.FirstOrDefault();
                if (syntaxRef == null) continue;

                bool isStatic, isNullableOrReference, isConst;
                ITypeSymbol typeSymbol;
                var originalField = syntaxRef.SyntaxTree.GetRoot().FindNode(syntaxRef.Span);
                EqualsValueClauseSyntax originalInitializer;
                SyntaxToken originalIdentifier;
                TypeSyntax parentType;
                if (member is IFieldSymbol)
                {
                    isStatic = ((IFieldSymbol)member).IsStatic;
                    isConst = ((IFieldSymbol)member).IsConst;
                    isNullableOrReference = Utils.IsNullableOrReference((IFieldSymbol)member, false, true);
                    typeSymbol = ((IFieldSymbol)member).Type;
                    originalInitializer = ((VariableDeclaratorSyntax)originalField).Initializer;
                    originalIdentifier = ((VariableDeclaratorSyntax)originalField).Identifier;
                    parentType = ((VariableDeclarationSyntax)originalField.Parent).Type;
                }
                else // IPropertySymbol
                {
                    isStatic = ((IPropertySymbol)member).IsStatic;
                    isConst = false;
                    isNullableOrReference = Utils.IsNullableOrReference((IPropertySymbol)member, false, true);
                    typeSymbol = ((IPropertySymbol)member).Type;
                    originalInitializer = ((PropertyDeclarationSyntax)originalField).Initializer;
                    originalIdentifier = ((PropertyDeclarationSyntax)originalField).Identifier;
                    parentType = ((PropertyDeclarationSyntax)originalField).Type;
                }

                // Para continuar: 
                // TODO: Los structs SI tienen default value, está aclarado en Utils.cs. Sin embargo puede pinchar esta implementación con definiciones recursivas.
                if (originalInitializer == null && typeSymbol.Kind != SymbolKind.TypeParameter && !isNullableOrReference && !Utils.HasDefaultValue(typeSymbol))
                    continue;

                /* Puede suceder que exista una clase partial, que tiene dos constructores, por lo que
                 * si existe un field inicializado fuera de los constructores hay que iniciarlo en
                 * ambos, por eso es necesario utilizar el modelo semantico para acceder al pedazo de
                 * sintaxis original, que puede no estar en el archivo que estamos instrumentando. 
                 * No salteamos las inicializaciones de fields nulas porque esta puede ser utilizada en 
                 * el programa sin haber sido inicializada previamente. */
                if (!pathToIdDict.ContainsKey(syntaxRef.SyntaxTree.FilePath))
                {
                    Console.WriteLine(System.IO.Path.GetFileName(originalClassNode.SyntaxTree.FilePath));
                    wrapperMethods.Pop();
                    return originalClassNode;
                }
                int tracedFileId = pathToIdDict[syntaxRef.SyntaxTree.FilePath];
                var actualSourceFileId = sourceFileId;
                var actualModel = Model;
                sourceFileId = tracedFileId;
                Model = Compilation.GetSemanticModel(syntaxRef.SyntaxTree);

                var traceInstr = TraceSyntaxGenerator.SimpleStatement(originalField, tracedFileId);
                if (isStatic)
                {
                    staticFieldsSyntaxlist = staticFieldsSyntaxlist.Add(traceInstr);
                    if (!isConst)
                    {
                        /* Las constantes pueden ser números, valores booleanos, cadenas o una referencia nula.
                         * Por lo tanto, para const solo dejamos la traza y no agregamos la inicializacion, ya
                         * que es stmt simple, que no ejecuta codigo no esperado. */
                        var initializer = originalInitializer != null ?
                            originalInitializer.Value as InitializerExpressionSyntax : (InitializerExpressionSyntax)null;
                        var modifiedOriginalInitializer = (EqualsValueClauseSyntax)Visit(originalInitializer);

                        string newFieldDeclaration = null;
                        if (initializer != null && initializer.IsKind(SyntaxKind.ArrayInitializerExpression))
                            newFieldDeclaration = originalIdentifier.ToString() + " = " + "new " + parentType.ToString() + Visit(originalInitializer.Value).ToString() + ";";
                        else if (modifiedOriginalInitializer != null)
                            newFieldDeclaration = originalIdentifier.ToString() + " = " + modifiedOriginalInitializer.Value.ToString() + ";";
                        if (newFieldDeclaration != null)
                            staticFieldsSyntaxlist = staticFieldsSyntaxlist.Add(SyntaxFactory.ParseStatement(newFieldDeclaration));
                    }
                }
                else
                {
                    instanceFieldsSyntaxlist = instanceFieldsSyntaxlist.Add(traceInstr);
                    var initializer = originalInitializer != null ?
                            originalInitializer.Value as InitializerExpressionSyntax : (InitializerExpressionSyntax)null;
                    var modifiedOriginalInitializer = (EqualsValueClauseSyntax)Visit(originalInitializer);

                    string newFieldDeclaration = null;
                    if (initializer != null && initializer.IsKind(SyntaxKind.ArrayInitializerExpression))
                        newFieldDeclaration = "this." + originalIdentifier.ToString() + " = " + "new " + parentType.ToString() + Visit(originalInitializer.Value).ToString() + ";";
                    else if (modifiedOriginalInitializer != null)
                        newFieldDeclaration = "this." + originalIdentifier.ToString() + " = " + modifiedOriginalInitializer.Value.ToString() + ";";
                    if (newFieldDeclaration != null)
                        instanceFieldsSyntaxlist = instanceFieldsSyntaxlist.Add(SyntaxFactory.ParseStatement(newFieldDeclaration));
                }
                sourceFileId = actualSourceFileId;
                Model = actualModel;
            }

            var methods = classSymbol.GetMembers().Where(x => x is IMethodSymbol).Select(x => (IMethodSymbol)x);
            bool staticConstructorMissing = HasStaticConstructor(methods);
            bool instanceConstructorMissing = HasInstanceConstructor(methods);

            // Modificacion de constructores
            /* Si existen, se les agrega la inicializacion de fields */
            var modifiedConstructors = new HashSet<ConstructorDeclarationSyntax>();
            foreach (var member in modifiedClassNode.Members.Where(x => x is ConstructorDeclarationSyntax))
            {
                var modifiedConstructor = (ConstructorDeclarationSyntax)member;
                SyntaxList<StatementSyntax> stmtList = new SyntaxList<StatementSyntax>();
                BlockSyntax constructorBlock = modifiedConstructor.Body;

                if (!(constructorBlock.Statements[0] is TryStatementSyntax))
                    continue;

                if (IsStatic(modifiedConstructor))
                {
                    // Solo se agrega la inicialización si no tiene initializers
                    // El 0 es el TryStatement
                    var tryStmt = (TryStatementSyntax)constructorBlock.Statements[0];
                    var newTryBlock = new SyntaxList<StatementSyntax>();
                    newTryBlock = newTryBlock.Add(tryStmt.Block.Statements[0]);
                    newTryBlock = newTryBlock.AddRange(staticFieldsSyntaxlist);
                    for (int i = 1; i < tryStmt.Block.Statements.Count; ++i)
                        newTryBlock = newTryBlock.Add(tryStmt.Block.Statements[i]);

                    tryStmt = tryStmt.WithBlock(SyntaxFactory.Block(newTryBlock));
                    stmtList = stmtList.Add(tryStmt);
                }
                else
                {
                    instanceConstructorMissing = false;
                    if (modifiedConstructor.Initializer == null || !modifiedConstructor.Initializer.ThisOrBaseKeyword.Text.ToString().Equals("this"))
                    {
                        // Solo se agrega la inicialización si no tiene initializers
                        // El 0 es el TryStatement
                        var tryStmt = (TryStatementSyntax)constructorBlock.Statements[0];
                        var newTryBlock = new SyntaxList<StatementSyntax>();
                        newTryBlock = newTryBlock.Add(tryStmt.Block.Statements[0]);
                        newTryBlock = newTryBlock.AddRange(instanceFieldsSyntaxlist);
                        for (int i = 1; i < tryStmt.Block.Statements.Count; ++i)
                            newTryBlock = newTryBlock.Add(tryStmt.Block.Statements[i]);

                        tryStmt = tryStmt.WithBlock(SyntaxFactory.Block(newTryBlock));
                        stmtList = stmtList.Add(tryStmt);
                    }
                    else
                        stmtList = stmtList.AddRange(constructorBlock.Statements);
                }

                constructorBlock = constructorBlock.WithStatements(stmtList);
                modifiedConstructors.Add(modifiedConstructor.WithBody(constructorBlock));
            }
            foreach (var modifiedConstructor in modifiedConstructors)
            {
                var constructors = modifiedClassNode.Members.Where(x => x is ConstructorDeclarationSyntax).Select(x => (ConstructorDeclarationSyntax)x);
                var constructor = constructors.Single(x => x.ParameterList.ToString() == modifiedConstructor.ParameterList.ToString() && IsStatic(x) == IsStatic(modifiedConstructor));
                modifiedClassNode = modifiedClassNode.ReplaceNode(constructor, modifiedConstructor);
            }

            // Agregado de constructores
            /* Solo se agregan constructores si es la primera clase partial (es decir, solo
             * agregamos constructor una vez en todas las partial de una clase) y si no existen
             * previamente */
            if (InsideFirstOfPartials(originalClassNode, classSymbol))
            {
                /* No hay que agregar constructor de instancia si la clase es estatica */
                if (instanceConstructorMissing && !classSymbol.IsStatic)
                {
                    SyntaxList<StatementSyntax> stmtList = new SyntaxList<StatementSyntax>();
                    ConstructorDeclarationSyntax newConstructor = SyntaxFactory.ConstructorDeclaration(SyntaxFactory.Identifier(originalClassNode.Identifier.ToString())).WithModifiers(
                        SyntaxFactory.TokenList(
                            SyntaxFactory.Token(
                                SyntaxFactory.TriviaList(), SyntaxKind.PublicKeyword, SyntaxFactory.TriviaList(
                                    SyntaxFactory.Space))));
                    stmtList = stmtList.Add(TraceSyntaxGenerator.EnterConstructorStatement(originalClassNode, sourceFileId).AddPreLine());
                    stmtList = stmtList.AddRange(instanceFieldsSyntaxlist);
                    stmtList = stmtList.Add(TraceSyntaxGenerator.ExitConstructorStatement(originalClassNode, sourceFileId));
                    stmtList = stmtList.Add(TraceSyntaxGenerator.EnterFinalFinallyStatement(originalClassNode, sourceFileId));
                    newConstructor = newConstructor.WithBody(SyntaxFactory.Block(stmtList).AddPreAndPostLine()).AddPreAndPostLine();
                    modifiedClassNode = modifiedClassNode.WithMembers(modifiedClassNode.Members.Add(newConstructor));
                }
                if (staticConstructorMissing)
                {
                    SyntaxList<StatementSyntax> stmtList = new SyntaxList<StatementSyntax>();
                    ConstructorDeclarationSyntax newConstructor = SyntaxFactory.ConstructorDeclaration(SyntaxFactory.Identifier(originalClassNode.Identifier.ToString())).WithModifiers(
                        SyntaxFactory.TokenList(
                            SyntaxFactory.Token(
                                SyntaxFactory.TriviaList(), SyntaxKind.StaticKeyword, SyntaxFactory.TriviaList(
                                    SyntaxFactory.Space))));
                    stmtList = stmtList.Add(TraceSyntaxGenerator.EnterStaticConstructorStatement(originalClassNode, sourceFileId).AddPreLine());
                    stmtList = stmtList.AddRange(staticFieldsSyntaxlist);
                    stmtList = stmtList.Add(TraceSyntaxGenerator.ExitStaticConstructorStatement(originalClassNode, sourceFileId));
                    stmtList = stmtList.Add(TraceSyntaxGenerator.EnterFinalFinallyStatement(originalClassNode, sourceFileId));
                    newConstructor = newConstructor.WithBody(SyntaxFactory.Block(stmtList).AddPreAndPostLine()).AddPreAndPostLine();
                    modifiedClassNode = modifiedClassNode.WithMembers(modifiedClassNode.Members.Add(newConstructor));
                }

                if (!classSymbol.IsStatic)
                {
                    var boolType = SyntaxFactory.ParseTypeName("int").WithTrailingTrivia(SyntaxFactory.Space);
                    var equals = SyntaxFactory.EqualsValueClause(TraceSyntaxGenerator.BeforeConstructorExpression(originalClassNode, sourceFileId));
                    var declarator = SyntaxFactory.VariableDeclarator("___ignore_me___").WithInitializer(equals);
                    var list = new SeparatedSyntaxList<VariableDeclaratorSyntax>().Add(declarator);
                    var declaration = SyntaxFactory.VariableDeclaration(boolType).WithVariables(list);
                    var ignore_me_field = SyntaxFactory.FieldDeclaration(declaration);
                    ignore_me_field = ignore_me_field.AddPostLine().AddTabs(2);

                    modifiedClassNode = modifiedClassNode.WithMembers(modifiedClassNode.Members.Add(ignore_me_field));
                }
            }
            alreadyVisitedClasses.Add(classSymbol.ContainingNamespace + "." + classSymbol.Name);

            var newMethods = wrapperMethods.Pop();
            var modifiedClassStr = modifiedClassNode.ToString();
            modifiedClassNode = modifiedClassNode.WithMembers(modifiedClassNode.Members.AddRange(newMethods.Values.Where(x => x.Identifier.ValueText.StartsWith("f_" + sourceFileId.ToString() + "_") && modifiedClassStr.Contains(x.CustomGetName()))));
            
            return modifiedClassNode
                .WithoutTrivia()
                .WithCloseBraceToken(SyntaxFactory.Token(SyntaxKind.CloseBraceToken).AddPostLine())
                .WithOpenBraceToken(SyntaxFactory.Token(SyntaxKind.OpenBraceToken));
        }

        public override SyntaxNode VisitStructDeclaration(StructDeclarationSyntax node)
        {
            /* Notar que este codigo es identico al de VisitClass, solo que no contiene ni el agregado del Before, ni el manejo de fields
             * y constructores de instancia, ya que en los struct si hay un constructor explicito, tiene que tener parametros y debe
             * inicializar todos los fields y ademas, no puede haber inicializaciones de fields fuera de constructores. */

            if ((excludedClassesMethods.ContainsKey(node.Identifier.ValueText) && excludedClassesMethods[node.Identifier.ValueText] == null) &&
                (!allowedClasses.Contains(node.Identifier.ValueText)))
                return node;

            wrapperMethods.Push(new Dictionary<int, MethodDeclarationSyntax>());
            StructDeclarationSyntax modifiedStruct = (StructDeclarationSyntax)base.VisitStructDeclaration(node);
            INamedTypeSymbol structSymbol = Model.GetDeclaredSymbol(node);

            SeparatedSyntaxList<StatementSyntax> staticFieldsSyntaxlist = new SeparatedSyntaxList<StatementSyntax>();
            IEnumerable<ISymbol> fieldMembers = structSymbol.GetMembers().Where(x => x is IFieldSymbol);
            foreach (var member in fieldMembers)
            {
                IFieldSymbol field = (IFieldSymbol)member;
                var syntaxRef = field.DeclaringSyntaxReferences.FirstOrDefault();
                if (syntaxRef == null) continue;

                VariableDeclaratorSyntax originalField = (VariableDeclaratorSyntax)syntaxRef.SyntaxTree.GetRoot().FindNode(syntaxRef.Span);
                if (originalField.Initializer == null && (!field.IsStatic || !Utils.IsNullableOrReference(field))) continue;

                int tracedFileId = pathToIdDict[syntaxRef.SyntaxTree.FilePath];

                var traceInstr = TraceSyntaxGenerator.SimpleStatement(originalField, tracedFileId);
                if (field.IsStatic)
                {
                    staticFieldsSyntaxlist = staticFieldsSyntaxlist.Add(traceInstr);
                    if (!field.IsConst)
                    {
                        if (originalField.Initializer != null)
                        {
                            var temp = sourceFileId;
                            sourceFileId = tracedFileId;
                            var actualModel = Model;
                            Model = Compilation.GetSemanticModel(syntaxRef.SyntaxTree);
                            SyntaxNode newSN = Visit(originalField.Initializer.Value);
                            Model = actualModel;
                            sourceFileId = temp;
                            staticFieldsSyntaxlist = staticFieldsSyntaxlist.Add(SyntaxFactory.ParseStatement(originalField.Identifier.ToString() + " = " + newSN.ToString() + ";"));
                        }
                        else if (Model.GetDeclaredSymbol(originalField) != null && Model.GetDeclaredSymbol(originalField).ContainingType.TypeKind == TypeKind.Struct)
                            staticFieldsSyntaxlist = staticFieldsSyntaxlist.Add(SyntaxFactory.ParseStatement(originalField.Identifier.ToString() + " = default(" + Model.GetDeclaredSymbol(originalField).ContainingType.Name + ");"));
                        else
                            staticFieldsSyntaxlist = staticFieldsSyntaxlist.Add(SyntaxFactory.ParseStatement(originalField.Identifier.ToString() + " = null;"));
                    }
                }
            }

            var methods = structSymbol.GetMembers().Where(x => x is IMethodSymbol).Select(x => (IMethodSymbol)x);
            bool staticConstructorMissing = HasStaticConstructor(methods);

            foreach (var member in modifiedStruct.Members)
            {
                if (member is ConstructorDeclarationSyntax)
                {
                    var modifiedConstructor = (ConstructorDeclarationSyntax)member;
                    if (modifiedConstructor.Modifiers.Select(x => x.ValueText).Contains(SyntaxFactory.Token(SyntaxKind.StaticKeyword).ValueText))
                    {
                        SyntaxList<StatementSyntax> stmtList = new SyntaxList<StatementSyntax>();
                        BlockSyntax constructorBlock = modifiedConstructor.Body;

                        staticConstructorMissing = false;
                        stmtList = stmtList.Add(constructorBlock.Statements[0]);
                        stmtList = stmtList.AddRange(staticFieldsSyntaxlist);
                        for (int i = 1; i < constructorBlock.Statements.Count; ++i)
                            stmtList = stmtList.Add(constructorBlock.Statements[i]);
                        constructorBlock = constructorBlock.WithStatements(stmtList);
                        modifiedStruct = modifiedStruct.ReplaceNode(modifiedConstructor, modifiedConstructor.WithBody(constructorBlock));
                    }
                }
            }

            if (InsideFirstOfPartials(node, structSymbol))
            {
                if (staticConstructorMissing)
                {
                    SyntaxList<StatementSyntax> stmtList = new SyntaxList<StatementSyntax>();
                    ConstructorDeclarationSyntax newConstructor = SyntaxFactory.ConstructorDeclaration(SyntaxFactory.Identifier(node.Identifier.ToString())).WithModifiers(
                        SyntaxFactory.TokenList(
                            SyntaxFactory.Token(
                                SyntaxFactory.TriviaList(), SyntaxKind.StaticKeyword, SyntaxFactory.TriviaList(
                                    SyntaxFactory.Space))));
                    stmtList = stmtList.Add(TraceSyntaxGenerator.EnterStaticConstructorStatement(node, sourceFileId));
                    stmtList = stmtList.AddRange(staticFieldsSyntaxlist);
                    stmtList = stmtList.Add(TraceSyntaxGenerator.ExitStaticConstructorStatement(node, sourceFileId));
                    stmtList = stmtList.Add(TraceSyntaxGenerator.EnterFinalFinallyStatement(node, sourceFileId));
                    newConstructor = newConstructor.WithBody(SyntaxFactory.Block(stmtList)).WithTrailingTrivia(SyntaxFactory.Whitespace("\n"));
                    modifiedStruct = modifiedStruct.WithMembers(modifiedStruct.Members.Add(newConstructor));
                }
            }
            alreadyVisitedClasses.Add(structSymbol.ContainingNamespace + "." + structSymbol.Name);

            var newMethods = wrapperMethods.Pop();
            modifiedStruct = modifiedStruct.WithMembers(modifiedStruct.Members.AddRange(newMethods.Values));
            return modifiedStruct.WithoutTrivia().AddPreAndPostLine();
        }

        public override SyntaxNode VisitMethodDeclaration(MethodDeclarationSyntax node)
        {
            if (IgnoreMethod(node))
                return node.WithoutTrivia().AddPreAndPostLine();

            //#region Roslyn ignore methods
            //// Custom list for LanguageParser.cs
            //List<string> methodsListUt = new List<string>();
            //methodsListUt.Add("private void ParseNamespaceBody(ref SyntaxToken openBrace, ref NamespaceBodyBuilder body, ref SyntaxListBuilder initialBadNodes, SyntaxKind parentKind)");
            //methodsListUt.Add("internal AttributeArgumentListSyntax ParseAttributeArgumentList()");
            //methodsListUt.Add("internal static DeclarationModifiers GetModifier(SyntaxKind kind, SyntaxKind contextualKind)");
            //methodsListUt.Add("private MemberDeclarationSyntax ParseMemberDeclarationOrStatementCore(SyntaxKind parentKind)");
            //methodsListUt.Add("private MemberDeclarationSyntax ParseMemberDeclarationCore(SyntaxKind parentKind)");
            //methodsListUt.Add("private void ParseParameterList(");
            //methodsListUt.Add("private VariableDeclaratorSyntax ParseVariableDeclarator(");
            //methodsListUt.Add("private void ParseEnumMemberDeclarations(");
            //methodsListUt.Add("private ScanTypeFlags ScanType(ParseTypeMode mode, out SyntaxToken lastTokenOfType)");
            //methodsListUt.Add("private TypeSyntax ParseTypeCore(ParseTypeMode mode)");
            //methodsListUt.Add("private void ParseForStatementExpressionList(ref SyntaxToken startToken, SeparatedSyntaxListBuilder<ExpressionSyntax> list)");
            //methodsListUt.Add("private ExpressionSyntax ParsePostFixExpression(ExpressionSyntax expr)");
            //methodsListUt.Add("private void ParseArgumentList(");
            //methodsListUt.Add("private void ParseAnonymousTypeMemberInitializers(ref SyntaxToken openBrace, ref SeparatedSyntaxListBuilder<AnonymousObjectMemberDeclaratorSyntax> list)");
            //methodsListUt.Add("private void ParseObjectOrCollectionInitializerMembers(ref SyntaxToken startToken, SeparatedSyntaxListBuilder<ExpressionSyntax> list, out bool isObjectInitializer)");
            //methodsListUt.Add("private void ParseExpressionsForComplexElementInitializer(ref SyntaxToken openBrace, SeparatedSyntaxListBuilder<ExpressionSyntax> list, out DiagnosticInfo closeBraceError)");
            //methodsListUt.Add("private InitializerExpressionSyntax ParseArrayInitializer()");
            //methodsListUt.Add("private ParameterListSyntax ParseLambdaParameterList()");

            //// SourceNamespaceSymbol_Completion.cs
            //methodsListUt.Add("internal override void ForceComplete(SourceLocation locationOpt, CancellationToken cancellationToken)");

            //if (methodsListUt.Any(x => node.ToString().Contains(x)))
            //    return node.WithoutTrivia().AddPreAndPostLine();
            //#endregion

            bool wrappedMethodsAdded = false;

            try
            {
                IList<StatementSyntax> beforeAssignments = new List<StatementSyntax>();
                IList<StatementSyntax> afterAssignments = new List<StatementSyntax>();
                var containsYieldReturn = node.DescendantNodes().Any(x => x is YieldStatementSyntax && x.GetContainer() == node);

                CleanRefAndOutParameters();
                if (containsYieldReturn)
                {
                    var internalType = ((INamedTypeSymbol)Model.GetDeclaredSymbol(node).ReturnType).TypeArguments.Count() > 0 ?
                        Utils.GetTypeName((ITypeSymbol)((INamedTypeSymbol)Model.GetDeclaredSymbol(node).ReturnType).TypeArguments.First()) : "object";
                    var initYieldReturnStmt = SyntaxFactory.ParseStatement("var " + listYieldReturn + "= new List<" + internalType + ">();").AddPreAndPostLine();
                    var getEnumerator = ((INamedTypeSymbol)Model.GetDeclaredSymbol(node).ReturnType).Name == "IEnumerator" ? ".GetEnumerator()" : "";
                    currentMethodInfo.returnYieldReturnStmt = SyntaxFactory.ParseStatement("return " + listYieldReturn + getEnumerator + ";").AddPreAndPostLine();
                    beforeAssignments.Add(initYieldReturnStmt);
                }

                //if (node.Body != null)
                //{
                    wrapperMethods.Push(new Dictionary<int, MethodDeclarationSyntax>());
                    wrappedMethodsAdded = true;
                //}

                MethodDeclarationSyntax modifiedMethod = (MethodDeclarationSyntax)base.VisitMethodDeclaration(node);

                var newMethods = wrapperMethods.Pop();
                wrappedMethodsAdded = false;

                SyntaxNode returnNode = null;
                BlockSyntax modifiedBlock = modifiedMethod.Body;
                if (modifiedBlock == null)
                {
                    if (modifiedMethod.ExpressionBody != null)
                    {
                        var enter_prop = TraceSyntaxGenerator.EnterMethodStatement(node.ExpressionBody, sourceFileId);
                        var stmt_stmt = TraceSyntaxGenerator.SimpleStatement(node.ExpressionBody.Expression, sourceFileId);
                        var ret_stmt = SyntaxFactory.ParseStatement(
                            (node.ReturnType.WithoutTrivia().GetText().ToString().Trim() != "void"
                            && !(node.ExpressionBody.Expression is ThrowExpressionSyntax) ? "return " : "") +
                            modifiedMethod.ExpressionBody.Expression.ToString() + ";");
                        var exit_prop = TraceSyntaxGenerator.ExitMethodStatement(node.ExpressionBody, sourceFileId);

                        var new_block = WrapBlock(SyntaxFactory.Block(enter_prop, stmt_stmt, ret_stmt, exit_prop), node.ExpressionBody,
                            node.ReturnType.WithoutTrivia().GetText().ToString().Trim() != "void");

                        new_block = new_block.AddStatements(newMethods.Values.Select(x => ConvertoToLocal(x)).ToArray());

                        var newMethod = SyntaxFactory.MethodDeclaration(node.ReturnType, node.Identifier.ValueText);
                        newMethod = newMethod.WithBody(new_block);
                        newMethod = newMethod.WithAttributeLists(node.AttributeLists);
                        newMethod = newMethod.WithModifiers(node.Modifiers);
                        newMethod = newMethod.WithParameterList(node.ParameterList);
                        newMethod = newMethod.WithTypeParameterList(node.TypeParameterList);
                        newMethod = newMethod.WithConstraintClauses(node.ConstraintClauses);
                        if (node.ExplicitInterfaceSpecifier != null)
                            newMethod = newMethod.WithExplicitInterfaceSpecifier(node.ExplicitInterfaceSpecifier);
                        returnNode = newMethod.WithoutTrivia().AddPreAndPostLine();
                    }
                    else
                        returnNode = modifiedMethod.WithoutTrivia().AddPreAndPostLine();
                }
                else 
                { 
                    if (node.Modifiers.Select(x => x.ValueText).Contains(SyntaxFactory.Token(SyntaxKind.StaticKeyword).ValueText))
                    {
                        modifiedBlock = ReplaceRefOrOutParameters(node.Body, modifiedBlock, afterAssignments/*, members*/);
                        // Lo del principio
                        modifiedBlock = modifiedBlock.WithStatements(modifiedBlock.Statements.Insert(0, TraceSyntaxGenerator.EnterStaticMethodStatement(node, sourceFileId)));
                        modifiedBlock = modifiedBlock.WithStatements(modifiedBlock.Statements.InsertRange(1, beforeAssignments));
                        modifiedBlock = modifiedBlock.WithStatements(modifiedBlock.Statements.InsertRange(1, currentMethodInfo.outParamsInline));
                        // Lo del final
                        modifiedBlock = modifiedBlock.AddStatements(afterAssignments.ToArray());
                        modifiedBlock = modifiedBlock.AddStatements(new StatementSyntax[] { TraceSyntaxGenerator.ExitStaticMethodStatement(node, sourceFileId) });
                    }
                    else
                    {
                        modifiedBlock = ReplaceRefOrOutParameters(node.Body, modifiedBlock, afterAssignments/*, members*/);
                        // Lo del principio
                        modifiedBlock = modifiedBlock.WithStatements(modifiedBlock.Statements.Insert(0, TraceSyntaxGenerator.EnterMethodStatement(node, sourceFileId)));
                        modifiedBlock = modifiedBlock.WithStatements(modifiedBlock.Statements.InsertRange(1, beforeAssignments));
                        modifiedBlock = modifiedBlock.WithStatements(modifiedBlock.Statements.InsertRange(1, currentMethodInfo.outParamsInline));
                        // Lo del final
                        modifiedBlock = modifiedBlock.AddStatements(afterAssignments.ToArray());
                        modifiedBlock = modifiedBlock.AddStatements(new StatementSyntax[] { TraceSyntaxGenerator.ExitMethodStatement(node, sourceFileId) });
                    }

                    if (containsYieldReturn)
                        modifiedBlock = modifiedBlock.AddStatements(currentMethodInfo.returnYieldReturnStmt);

                    modifiedBlock = modifiedBlock.AddStatements(newMethods.Values.Select(x => ConvertoToLocal(x)).ToArray());

                    var modified = modifiedMethod.WithBody(WrapBlock(modifiedBlock, node, node.ReturnType.WithoutTrivia().GetText().ToString().Trim() != "void"));

                    CleanRefAndOutParameters();

                    returnNode = modified.WithoutTrivia().AddPreAndPostLine();
                }

                //var fileName = node.SyntaxTree.FilePath;
                //var firstLine = node.ToString().Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries).First();
                //if (InstrumenterRewriter.methodsToComplete.ContainsKey(fileName) &&
                //    InstrumenterRewriter.methodsToComplete[fileName].ContainsKey(firstLine))
                //    InstrumenterRewriter.methodsToComplete[fileName][firstLine] = returnNode.ToString();

                return returnNode;
            }
            catch (Exception ex)
            {
                if (wrappedMethodsAdded)
                    wrapperMethods.Pop();
                //return node;
            }

            return node;
        }

        public override SyntaxNode VisitLocalFunctionStatement(LocalFunctionStatementSyntax node)
        {
            bool wrappedMethodsAdded = false;
            var previousMethodInfo = new MethodsTempInfo(currentMethodInfo);

            try
            {
                LocalFunctionStatementSyntax localFunction = null;
                IList<StatementSyntax> beforeAssignments = new List<StatementSyntax>();
                IList<StatementSyntax> afterAssignments = new List<StatementSyntax>();
                var containsYieldReturn = node.DescendantNodes().Any(x => x is YieldStatementSyntax);
                
                CleanRefAndOutParameters();
                if (containsYieldReturn)
                {
                    var internalType = ((INamedTypeSymbol)((IMethodSymbol)Model.GetDeclaredSymbol(node)).ReturnType).TypeArguments.Count() > 0 ?
                        Utils.GetTypeName((ITypeSymbol)((INamedTypeSymbol)((IMethodSymbol)Model.GetDeclaredSymbol(node)).ReturnType).TypeArguments.First()) : "object";
                    var initYieldReturnStmt = SyntaxFactory.ParseStatement("var " + listYieldReturn + "= new List<" + internalType + ">();").AddPreAndPostLine();
                    var getEnumerator = ((INamedTypeSymbol)((IMethodSymbol)Model.GetDeclaredSymbol(node)).ReturnType).Name == "IEnumerator" ? ".GetEnumerator()" : "";
                    currentMethodInfo.returnYieldReturnStmt = SyntaxFactory.ParseStatement("return " + listYieldReturn + getEnumerator + ";").AddPreAndPostLine();
                    beforeAssignments.Add(initYieldReturnStmt);
                }

                if (node.Body != null)
                {
                    wrapperMethods.Push(new Dictionary<int, MethodDeclarationSyntax>());
                    wrappedMethodsAdded = true;
                }

                LocalFunctionStatementSyntax modified = (LocalFunctionStatementSyntax)base.VisitLocalFunctionStatement(node);
                BlockSyntax modifiedBlock = modified.Body;
                if (modifiedBlock == null)
                {
                    if (modified.ExpressionBody != null)
                    {
                        var enter_prop = TraceSyntaxGenerator.EnterMethodStatement(node.ExpressionBody, sourceFileId);
                        var stmt_stmt = TraceSyntaxGenerator.SimpleStatement(node.ExpressionBody.Expression, sourceFileId);
                        var ret_stmt = SyntaxFactory.ParseStatement(
                            (node.ReturnType.WithoutTrivia().GetText().ToString().Trim() != "void"
                            && !(node.ExpressionBody.Expression is ThrowExpressionSyntax) ? "return " : "") +
                            modified.ExpressionBody.Expression.ToString() + ";");
                        var exit_prop = TraceSyntaxGenerator.ExitMethodStatement(node.ExpressionBody, sourceFileId);

                        var new_block = WrapBlock(SyntaxFactory.Block(enter_prop, stmt_stmt, ret_stmt, exit_prop), node.ExpressionBody,
                            node.ReturnType.WithoutTrivia().GetText().ToString().Trim() != "void");

                        localFunction = SyntaxFactory.LocalFunctionStatement(node.ReturnType, node.Identifier.ValueText);
                        localFunction = localFunction.WithBody(new_block);
                        localFunction = localFunction.WithAttributeLists(node.AttributeLists);
                        localFunction = localFunction.WithModifiers(node.Modifiers);
                        localFunction = localFunction.WithParameterList(node.ParameterList);
                        localFunction = localFunction.WithTypeParameterList(node.TypeParameterList);
                        localFunction = localFunction.WithoutTrivia().AddPreAndPostLine();
                    }
                    else
                        localFunction = modified.WithoutTrivia().AddPreAndPostLine();
                }
                else
                {
                    if (node.Modifiers.Select(x => x.ValueText).Contains(SyntaxFactory.Token(SyntaxKind.StaticKeyword).ValueText))
                    {
                        modifiedBlock = ReplaceRefOrOutParameters(node.Body, modifiedBlock, afterAssignments);
                        // Lo del principio
                        modifiedBlock = modifiedBlock.WithStatements(modifiedBlock.Statements.Insert(0, TraceSyntaxGenerator.EnterStaticMethodStatement(node, sourceFileId)));
                        modifiedBlock = modifiedBlock.WithStatements(modifiedBlock.Statements.InsertRange(1, beforeAssignments));
                        modifiedBlock = modifiedBlock.WithStatements(modifiedBlock.Statements.InsertRange(1, currentMethodInfo.outParamsInline));
                        // Lo del final
                        modifiedBlock = modifiedBlock.AddStatements(afterAssignments.ToArray());
                        modifiedBlock = modifiedBlock.AddStatements(new StatementSyntax[] { TraceSyntaxGenerator.ExitStaticMethodStatement(node, sourceFileId) });
                    }
                    else
                    {
                        modifiedBlock = ReplaceRefOrOutParameters(node.Body, modifiedBlock, afterAssignments);
                        // Lo del principio
                        modifiedBlock = modifiedBlock.WithStatements(modifiedBlock.Statements.Insert(0, TraceSyntaxGenerator.EnterMethodStatement(node, sourceFileId)));
                        modifiedBlock = modifiedBlock.WithStatements(modifiedBlock.Statements.InsertRange(1, beforeAssignments));
                        modifiedBlock = modifiedBlock.WithStatements(modifiedBlock.Statements.InsertRange(1, currentMethodInfo.outParamsInline));
                        // Lo del final
                        modifiedBlock = modifiedBlock.AddStatements(afterAssignments.ToArray());
                        modifiedBlock = modifiedBlock.AddStatements(new StatementSyntax[] { TraceSyntaxGenerator.ExitMethodStatement(node, sourceFileId) });
                    }

                    if (containsYieldReturn)
                        modifiedBlock = modifiedBlock.AddStatements(currentMethodInfo.returnYieldReturnStmt);

                    var newMethods = wrapperMethods.Pop();
                    wrappedMethodsAdded = false;
                    modifiedBlock = modifiedBlock.AddStatements(newMethods.Values.Select(x => ConvertoToLocal(x)).ToArray());

                    CleanRefAndOutParameters();
                    localFunction = node.WithBody(WrapBlock(modifiedBlock, node, node.ReturnType.WithoutTrivia().GetText().ToString().Trim() != "void"));
                    localFunction = localFunction.WithoutTrivia().AddPreAndPostLine();
                }

                return localFunction;
            }
            catch (Exception ex)
            {
                if (wrappedMethodsAdded)
                    wrapperMethods.Pop();
                return node;
            }
            finally
            {
                currentMethodInfo = previousMethodInfo;
            }
        }

        public override SyntaxNode VisitAccessorDeclaration(AccessorDeclarationSyntax node)
        {
            IList<StatementSyntax> beforeAssignments = new List<StatementSyntax>();
            IList<StatementSyntax> afterAssignments = new List<StatementSyntax>();

            var containsYieldReturn = node.DescendantNodes().Any(x => x is YieldStatementSyntax);
            var isStructAccess = node.Parent.Parent.Parent is StructDeclarationSyntax;

            if (containsYieldReturn)
            {
                var internalType = ((INamedTypeSymbol)Model.GetDeclaredSymbol(node).ReturnType).TypeArguments.Count() > 0 ?
                    Utils.GetTypeName((ITypeSymbol)((INamedTypeSymbol)Model.GetDeclaredSymbol(node).ReturnType).TypeArguments.First()) : "object";
                var initYieldReturnStmt = SyntaxFactory.ParseStatement("var " + listYieldReturn + "= new List<" + internalType + ">();").AddPreAndPostLine();
                var getEnumerator = ((INamedTypeSymbol)Model.GetDeclaredSymbol(node).ReturnType).Name == "IEnumerator" ? ".GetEnumerator()" : "";
                currentMethodInfo.returnYieldReturnStmt = SyntaxFactory.ParseStatement("return " + listYieldReturn + getEnumerator + ";").AddPreAndPostLine();
                beforeAssignments.Add(initYieldReturnStmt);
            }

            if (node.Body != null)
                wrapperMethods.Push(new Dictionary<int, MethodDeclarationSyntax>());

            AccessorDeclarationSyntax newNode = (AccessorDeclarationSyntax)base.VisitAccessorDeclaration(node);
            BlockSyntax block = newNode.Body;
            if (block == null)
            {
                // Si tiene expression body, lo armamos como return
                if (node.ExpressionBody != null)
                {
                    var enter_prop = TraceSyntaxGenerator.EnterMethodStatement(node.ExpressionBody, sourceFileId);
                    var stmt_stmt = TraceSyntaxGenerator.SimpleStatement(node.ExpressionBody.Expression, sourceFileId);
                    var ret_stmt = SyntaxFactory.ParseStatement(
                        (node.Keyword.WithoutTrivia().ValueText == "get" &&
                        !(node.ExpressionBody.Expression is ThrowExpressionSyntax)
                        ? "return " : "")
                        + newNode.ExpressionBody.Expression.ToString() + ";");
                    var exit_prop = TraceSyntaxGenerator.ExitMethodStatement(node.ExpressionBody, sourceFileId);

                    var new_block = SyntaxFactory.Block(enter_prop, stmt_stmt, ret_stmt, exit_prop);
                    var newAccessor = SyntaxFactory.AccessorDeclaration(node.Kind(), new_block);
                    newAccessor = newAccessor.WithAttributeLists(node.AttributeLists);
                    newAccessor = newAccessor.WithModifiers(node.Modifiers);
                    return newAccessor;
                }
                else
                    return newNode;
            }
            
            bool isStatic = 
                (node.Parent.Parent is PropertyDeclarationSyntax &&
                (((PropertyDeclarationSyntax)node.Parent.Parent).Modifiers.Select(x => x.ValueText).Contains(SyntaxFactory.Token(SyntaxKind.StaticKeyword).ValueText))) ||
                (node.Parent.Parent is IndexerDeclarationSyntax &&
                (((IndexerDeclarationSyntax)node.Parent.Parent).Modifiers.Select(x => x.ValueText).Contains(SyntaxFactory.Token(SyntaxKind.StaticKeyword).ValueText)));

            if (isStatic)
            { 
                block = block.WithStatements(block.Statements.Insert(0, TraceSyntaxGenerator.EnterStaticMethodStatement(node, sourceFileId)));
                if (isStructAccess)
                {
                    block = ReplaceRefOrOutParameters(node.Body, block, afterAssignments);
                    block = block.WithStatements(block.Statements.InsertRange(1, beforeAssignments));
                    block = block.WithStatements(block.Statements.InsertRange(1, currentMethodInfo.outParamsInline));
                    block = block.AddStatements(afterAssignments.ToArray());
                }
                else
                {
                    block = block.WithStatements(block.Statements.InsertRange(1, beforeAssignments));
                    block = block.WithStatements(block.Statements.InsertRange(1, currentMethodInfo.outParamsInline));
                }
                block = block.AddStatements(new StatementSyntax[] { TraceSyntaxGenerator.ExitStaticMethodStatement(node, sourceFileId) });
            }
            else
            {
                block = block.WithStatements(block.Statements.Insert(0, TraceSyntaxGenerator.EnterMethodStatement(node, sourceFileId)));
                if (isStructAccess)
                {
                    block = ReplaceRefOrOutParameters(node.Body, block, afterAssignments);
                    block = block.WithStatements(block.Statements.InsertRange(1, beforeAssignments));
                    block = block.WithStatements(block.Statements.InsertRange(1, currentMethodInfo.outParamsInline));
                    block = block.AddStatements(afterAssignments.ToArray());
                }
                else
                {
                    block = block.WithStatements(block.Statements.InsertRange(1, beforeAssignments));
                    block = block.WithStatements(block.Statements.InsertRange(1, currentMethodInfo.outParamsInline));
                }
                block = block.AddStatements(new StatementSyntax[] { TraceSyntaxGenerator.ExitMethodStatement(node, sourceFileId) });
            }

            if (containsYieldReturn)
                block = block.AddStatements(currentMethodInfo.returnYieldReturnStmt);

            var newMethods = wrapperMethods.Pop();
            block = block.AddStatements(newMethods.Values.Select(x => ConvertoToLocal(x)).ToArray());

            // Acá se puede llegar por dentro de la property o no... si lo agrega por la property no lo debemos agregar por acá
            // (PD: el otro es IndexerDeclarationSyntax)
            AccessorDeclarationSyntax modified;
            if (!(node.Parent.Parent is PropertyDeclarationSyntax))
                modified = newNode.WithBody(WrapBlock(block, node, node.Keyword.WithoutTrivia().ValueText == "get"));
            else
                modified = newNode.WithBody(block);

            CleanRefAndOutParameters();

            return modified.WithoutTrivia().AddPreAndPostLine();
        }
        
        public override SyntaxNode VisitConstructorDeclaration(ConstructorDeclarationSyntax node)
        {
            try 
            { 
                IList <StatementSyntax> beforeAssignments = new List<StatementSyntax>();
                IList<StatementSyntax> afterAssignments = new List<StatementSyntax>();

                CleanRefAndOutParameters();

                if (node.Body != null)
                    wrapperMethods.Push(new Dictionary<int, MethodDeclarationSyntax>());

                insideBaseCall = true;
                var modifiedConstructor = (ConstructorDeclarationSyntax)base.VisitConstructorDeclaration(node);
                if (node.Body != null)
                {
                    BlockSyntax modifiedBlock;
                    if (modifiedConstructor.Modifiers.Select(x => x.ValueText).Contains(SyntaxFactory.Token(SyntaxKind.StaticKeyword).ValueText))
                    {
                        modifiedBlock = ReplaceRefOrOutParameters(node.Body, modifiedConstructor.Body, afterAssignments/*, null*/);
                        modifiedBlock = modifiedBlock.WithStatements(modifiedBlock.Statements.Insert(0, TraceSyntaxGenerator.EnterStaticConstructorStatement(node, sourceFileId)));
                        modifiedBlock = modifiedBlock.WithStatements(modifiedBlock.Statements.InsertRange(1, beforeAssignments));
                        modifiedBlock = modifiedBlock.WithStatements(modifiedBlock.Statements.InsertRange(1, currentMethodInfo.outParamsInline));
                        modifiedBlock = modifiedBlock.AddStatements(afterAssignments.ToArray());
                        modifiedBlock = modifiedBlock.AddStatements(new StatementSyntax[] { TraceSyntaxGenerator.ExitStaticConstructorStatement(node, sourceFileId) });
                    }
                    else
                    {
                        modifiedBlock = ReplaceRefOrOutParameters(node.Body, modifiedConstructor.Body, afterAssignments/*, null*/);
                        modifiedBlock = modifiedBlock.WithStatements(modifiedBlock.Statements.Insert(0, TraceSyntaxGenerator.EnterConstructorStatement(node, sourceFileId)));
                        modifiedBlock = modifiedBlock.WithStatements(modifiedBlock.Statements.InsertRange(1, beforeAssignments));
                        modifiedBlock = modifiedBlock.WithStatements(modifiedBlock.Statements.InsertRange(1, currentMethodInfo.outParamsInline));
                        modifiedBlock = modifiedBlock.AddStatements(afterAssignments.ToArray());
                        modifiedBlock = modifiedBlock.AddStatements(new StatementSyntax[] { TraceSyntaxGenerator.ExitConstructorStatement(node, sourceFileId) });
                    }
                    modifiedConstructor = modifiedConstructor.WithBody(modifiedBlock);
                }
                else
                {
                    var expressionTraceStmt = TraceSyntaxGenerator.SimpleStatement(node.ExpressionBody.Expression, sourceFileId);
                    var visitedExpressionBody = SyntaxFactory.ExpressionStatement((ExpressionSyntax)Visit(node.ExpressionBody.Expression));

                    var newStmtList = new SyntaxList<StatementSyntax>();
                    newStmtList = newStmtList.Add(TraceSyntaxGenerator.EnterConstructorStatement(node, sourceFileId).AddPreLine());
                    newStmtList = newStmtList.Add(expressionTraceStmt);
                    newStmtList = newStmtList.Add(visitedExpressionBody);
                    newStmtList = newStmtList.Add(TraceSyntaxGenerator.ExitConstructorStatement(node, sourceFileId));
                    var newBlock = SyntaxFactory.Block(newStmtList);
                    modifiedConstructor = SyntaxFactory.ConstructorDeclaration(node.AttributeLists, node.Modifiers, node.Identifier, node.ParameterList, node.Initializer, newBlock);
                }

                CleanRefAndOutParameters();

                // Si tiene llamada al base le wrappeamos el 1er argumento si es que lo tiene
                if (modifiedConstructor.Initializer != null && modifiedConstructor.Initializer is ConstructorInitializerSyntax &&
                    ((ConstructorInitializerSyntax)modifiedConstructor.Initializer).ArgumentList != null && 
                    ((ConstructorInitializerSyntax)modifiedConstructor.Initializer).ArgumentList.Arguments.Count > 0)
                {
                    var spanHash = Utils.GetHashCodeFromFileAndSpan(sourceFileId, node.Initializer.Span);
                    MethodDeclarationSyntax methodDecl = null;
                    // Wrapper methods couldn't be local functions on base calls
                    var localScopeForWrappedMethods = GetWrappedMethodsDictionary();
                    if (localScopeForWrappedMethods.ContainsKey(spanHash))
                        methodDecl = localScopeForWrappedMethods[spanHash];
                    if (methodDecl == null)
                    {
                        methodDecl = TraceSyntaxGenerator.ExpressionWrapped(((ConstructorInitializerSyntax)node.Initializer).ArgumentList.Arguments.First(), Model, sourceFileId, TraceSyntaxGenerator.AccessType.Base, node);
                        localScopeForWrappedMethods.Add(spanHash, methodDecl);
                    }

                    var iArg = ((ConstructorInitializerSyntax)modifiedConstructor.Initializer).ArgumentList.Arguments.First();
                    var modifiedExpression = SyntaxFactory.ParseExpression(string.Format("{0}({1}) ", methodDecl.Identifier.ValueText, iArg.Expression));
                    ArgumentSyntax argS = null;
                    if (iArg.NameColon != null)
                        argS = SyntaxFactory.Argument(iArg.NameColon, iArg.RefKindKeyword, modifiedExpression);
                    else
                        argS = SyntaxFactory.Argument(modifiedExpression);

                    // Construimos otro constructor initializer syntax con el 1er argumento wrapeado
                    modifiedConstructor = modifiedConstructor.WithInitializer(SyntaxFactory.ConstructorInitializer
                        (((ConstructorInitializerSyntax)modifiedConstructor.Initializer).Kind(), 
                        ((ConstructorInitializerSyntax)modifiedConstructor.Initializer).ArgumentList.WithArguments(
                            SyntaxFactory.SeparatedList<ArgumentSyntax>(
                                    ((ConstructorInitializerSyntax)modifiedConstructor.Initializer).ArgumentList.Arguments.Select(x => 
                                    x == ((ConstructorInitializerSyntax)modifiedConstructor.Initializer).ArgumentList.Arguments.First() ?
                                    argS : x)))));
                }

                if (node.Body != null)
                { 
                    var newMethods = wrapperMethods.Pop();
                    modifiedConstructor = modifiedConstructor.WithBody(modifiedConstructor.Body.AddStatements(newMethods.Values.Select(x => ConvertoToLocal(x)).ToArray()));
                }

                insideBaseCall = false;
                return modifiedConstructor
                    .WithBody(WrapBlock(modifiedConstructor.Body, node, false))
                    .WithoutTrivia().AddPreAndPostLine();
            }
            catch(GoToStatementException ex)
            {
                return node;
            }
        }

        public override SyntaxNode VisitConversionOperatorDeclaration(ConversionOperatorDeclarationSyntax node)
        {
            if (node.Body != null)
                wrapperMethods.Push(new Dictionary<int, MethodDeclarationSyntax>());

            var modifiedNode = (ConversionOperatorDeclarationSyntax)base.VisitConversionOperatorDeclaration(node);

            if (modifiedNode.Body != null)
            { 
                var modifiedBlock = modifiedNode.Body;
                modifiedBlock = modifiedBlock.WithStatements(modifiedBlock.Statements.Insert(0, TraceSyntaxGenerator.EnterStaticMethodStatement(node, sourceFileId)));
                modifiedBlock = modifiedBlock.AddStatements(new StatementSyntax[] { TraceSyntaxGenerator.ExitStaticMethodStatement(node, sourceFileId) });

                var newMethods = wrapperMethods.Pop();
                modifiedBlock = modifiedBlock.AddStatements(newMethods.Values.Select(x => ConvertoToLocal(x)).ToArray());

                modifiedNode = modifiedNode.WithBody(WrapBlock(modifiedBlock, node, false));

                return modifiedNode;
            }
            // Si tiene expression body, lo armamos como return
            else if (modifiedNode.ExpressionBody != null)
            {
                var enter_prop = TraceSyntaxGenerator.EnterStaticMethodStatement(node.ExpressionBody, sourceFileId);
                var stmt_stmt = TraceSyntaxGenerator.SimpleStatement(node.ExpressionBody.Expression, sourceFileId);
                var ret_stmt = SyntaxFactory.ParseStatement("return " + modifiedNode.ExpressionBody.Expression.ToString() + ";");
                var exit_prop = TraceSyntaxGenerator.ExitStaticMethodStatement(node.ExpressionBody, sourceFileId);

                var new_block = WrapBlock(SyntaxFactory.Block(enter_prop, stmt_stmt, ret_stmt, exit_prop), node.ExpressionBody, true);
                var conversion = SyntaxFactory.ConversionOperatorDeclaration(
                    modifiedNode.AttributeLists, node.Modifiers, node.ImplicitOrExplicitKeyword, 
                    node.Type, node.ParameterList, new_block, null);
                conversion = conversion.WithOperatorKeyword(node.OperatorKeyword);
                return conversion.WithoutTrivia().AddPreAndPostLine();
            }
            throw new SlicerException("Unexpected behavior (SlicerException)");
        }

        public override SyntaxNode VisitOperatorDeclaration(OperatorDeclarationSyntax node)
        {
            if (node.Body != null)
                wrapperMethods.Push(new Dictionary<int, MethodDeclarationSyntax>());

            var newNode = (OperatorDeclarationSyntax)base.VisitOperatorDeclaration(node);
            BlockSyntax block = newNode.Body;
            if (block == null) 
                return newNode;

            block = block.WithStatements(block.Statements.Insert(0, TraceSyntaxGenerator.EnterStaticMethodStatement(node, sourceFileId)));
            block = block.WithStatements(block.Statements.Add(TraceSyntaxGenerator.ExitStaticMethodStatement(node, sourceFileId)));
            var newMethods = wrapperMethods.Pop();
            block = block.AddStatements(newMethods.Values.Select(x => ConvertoToLocal(x)).ToArray());

            newNode = node.WithBody(WrapBlock(block, node, false)).WithoutTrivia().AddPreAndPostLine();

            return newNode;
        }

        public override SyntaxNode VisitBlock(BlockSyntax block)
        {
            SyntaxList<StatementSyntax> instrumentedStmts = new SyntaxList<StatementSyntax>();
            SyntaxList<StatementSyntax> originalStmts = block.Statements;

            foreach (var statementOriginal in originalStmts)
            {
                var statementInstrumented = (StatementSyntax)Visit(statementOriginal);
                if (statementInstrumented is BlockSyntax && !(statementOriginal is DoStatementSyntax) && !(statementOriginal is BlockSyntax))
                {
                    var newBlock = statementInstrumented as BlockSyntax;
                    foreach (var stmt in newBlock.Statements)
                        instrumentedStmts = instrumentedStmts.Add(stmt);
                }
                else
                    instrumentedStmts = instrumentedStmts.Add(statementInstrumented);

            }

            block = block.WithOpenBraceToken(base.VisitToken(block.OpenBraceToken));
            block = block.WithCloseBraceToken(base.VisitToken(block.CloseBraceToken));

            return block.WithStatements(instrumentedStmts);
        }

        public override SyntaxNode VisitBinaryExpression(BinaryExpressionSyntax node)
        {
            if (Utils.ShortcircuitsBinaries.Contains(node.Kind()))
            {
                var leftExpression = (ExpressionSyntax)base.Visit(node.Left);
                var rightExpression = (ExpressionSyntax)base.Visit(node.Right);

                return TraceSyntaxGenerator.EnterExpressionInvocationWrapped_V2(leftExpression, rightExpression, Model, node, sourceFileId);
            }

            var operation = Model.GetOperation(node);
            var modifiedExpression = (BinaryExpressionSyntax)base.VisitBinaryExpression(node);

            if (operation is IBinaryOperation && Utils.IOperationStringBinaries.Contains(((IBinaryOperation)operation).OperatorKind)
                && ((IBinaryOperation)operation).LeftOperand.Type != null && ((IBinaryOperation)operation).RightOperand.Type != null)
            {
                var binaryOperation = (IBinaryOperation)operation;
                // Si uno es string y el otro no va a haber un "ToString()" automático que mejor wrappear.
                // TODOX: puede cambiar el orden de ejecución LAZY a HARD, 
                // ver "OverrideToStringTest" que primero manda el último end member access antes de la entrada al método
                // Wrappeamos a los que no son string
                if (binaryOperation.LeftOperand.Type.Name == "String" && binaryOperation.RightOperand.Type.Name != "String")
                {
                    var toStringExpr = SyntaxFactory.ParseExpression("(" + modifiedExpression.Right.ToString() + ").ToString()");
                    modifiedExpression = modifiedExpression.WithRight(toStringExpr);
                    modifiedExpression = modifiedExpression.WithRight(
                            TraceSyntaxGenerator.InvocationWrapperExpression(modifiedExpression.Right, node.Right, sourceFileId));
                }
                if (binaryOperation.RightOperand.Type.Name == "String" && binaryOperation.LeftOperand.Type.Name != "String")
                {
                    var toStringExpr = SyntaxFactory.ParseExpression("(" + modifiedExpression.Left.ToString() + ").ToString()");
                    modifiedExpression = modifiedExpression.WithLeft(toStringExpr);
                    modifiedExpression = modifiedExpression.WithLeft(
                        TraceSyntaxGenerator.InvocationWrapperExpression(modifiedExpression.Left, node.Left, sourceFileId));
                }
            }

            return modifiedExpression;
        }

        public override SyntaxNode VisitVariableDeclarator(VariableDeclaratorSyntax node)
        {
            var modifiedDeclarator = (VariableDeclaratorSyntax)base.VisitVariableDeclarator(node);
            var fieldNode = (FieldDeclarationSyntax)node.Ancestors().SingleOrDefault(x => x is FieldDeclarationSyntax);

            if (fieldNode != null)
            {
                if (!fieldNode.Modifiers.Select(x => x.ValueText).Contains(SyntaxFactory.Token(SyntaxKind.ConstKeyword).ValueText))
                {
                    return modifiedDeclarator.WithInitializer(null);
                }
            }
            return modifiedDeclarator.WithoutTrivia().AddPreAndPostLine();
        }

        public override SyntaxNode VisitAssignmentExpression(AssignmentExpressionSyntax node)
        {
            // Si es el += y en el lado izquierdo es una property 
            // agregamos un wrapper al lado derecho aviazando que terminamos la lectura del lado izquierdo
            // Tiene que pasar:
            // O BIEN ES UNA PROPERTY NO CONSTANTE
            // O BIEN ES UN IDENTIFIER PROPERTY O FIELD PERO NO CONSTANTE NI EVENTO
            // TODO: ACOMODAR BIEN ESTO PORQUE ES CÓDIGO REPETIDO DEL VISIT IDENTIFIER Y DEL VISIT MEMBERACCESS
            if (
                // 1. Distinto del IGUAL
                node.OperatorToken.Kind() != SyntaxKind.EqualsToken &&
                // 2.A. El nodo de la izquierda es un MemberAccessExpressionSyntax (Properties o Fields)
                ((node.Left is MemberAccessExpressionSyntax &&
                // A.3. Si es MemberAccessExpressionSyntax no pueden ser ni Fields constantes ni Eventos
                // Entonces o bien no tiene symbolo (no sucede en esos casos
                (Model.GetSymbolInfo(node.Left).Symbol == null ||
                // O bien no se da que:
                // X: Sea Field
                !((Model.GetSymbolInfo(node.Left).Symbol is IFieldSymbol && ((IFieldSymbol)Model.GetSymbolInfo(node.Left).Symbol).IsConst) ||
                (Model.GetSymbolInfo(node.Left).Symbol is IEventSymbol)))) 
                // 2.B. La otra opción es que no sea un MemberAccessExpressionSyntax pero sea un identifier que es property del this
                || (Model.GetSymbolInfo(node.Left).Symbol != null && !(Model.GetSymbolInfo(node.Left).Symbol is IEventSymbol) &&
                (
                (Model.GetSymbolInfo(node.Left).Symbol.Kind == SymbolKind.Field && !((IFieldSymbol)Model.GetSymbolInfo(node.Left).Symbol).IsConst)
                || (Model.GetSymbolInfo(node.Left).Symbol.Kind == SymbolKind.Property &&
                !(node.Left.Parent.Parent is AnonymousObjectMemberDeclaratorSyntax &&
                    node.Left.Parent is NameEqualsSyntax && ((NameEqualsSyntax)node.Left.Parent).Name == node.Left))))))
            {
                var left = Visit(node.Left);
                var right = Visit(node.Right);

                //if (node.Left.GetText().ToString().Trim() == "hg.phi0")
                //    ;

                try
                { 
                    if ((Model.GetSymbolInfo(right)).CandidateSymbols.Count() > 0 &&
                        ((Model.GetSymbolInfo(right))).CandidateSymbols.First().Kind == SymbolKind.Method)
                        return SyntaxFactory.AssignmentExpression(node.Kind(), (ExpressionSyntax)left, node.OperatorToken, (ExpressionSyntax)right);
                }
                catch(Exception ex)
                {

                }
                ExpressionSyntax exp_right = null;
                if (Model.GetOperation(node.Left)?.Kind == OperationKind.FieldReference)
                    exp_right = (ExpressionSyntax)right; 
                else
                    exp_right = TraceSyntaxGenerator.InitialMemberAccessWrapperExpression(right, Model, node.Left, sourceFileId);
                return SyntaxFactory.AssignmentExpression(node.Kind(), (ExpressionSyntax)left, node.OperatorToken, (ExpressionSyntax)exp_right);
            }
            else
                return ((AssignmentExpressionSyntax)base.VisitAssignmentExpression(node));
        }

        public override SyntaxNode VisitSwitchSection(SwitchSectionSyntax node)
        {
            SwitchSectionSyntax newSection = (SwitchSectionSyntax)base.VisitSwitchSection(node);

            var list = new SyntaxList<StatementSyntax>();
            SyntaxList<StatementSyntax> originalStmts = node.Statements;
            SyntaxList<StatementSyntax> alreadyInstrumentedStmts = newSection.Statements;

            var positionStatement = 0;
            foreach (var stmtOriginal in originalStmts)
            {
                var stmtAlreadyInstrumentedInternally = alreadyInstrumentedStmts[positionStatement++];

                if (!(stmtOriginal is BlockSyntax) && stmtAlreadyInstrumentedInternally is BlockSyntax)
                {
                    /* Si el stmt es un bloque, el visit retorna un bloque, si es otro stmt
                     * retorna tambien un bloque (por el agregado del Trace). */
                    var block = (BlockSyntax)stmtAlreadyInstrumentedInternally;
                    list = list.AddRange(block.Statements); 
                }
                else
                {
                    list = list.Add(stmtAlreadyInstrumentedInternally);
                }
            }

            var syntaxListLabel = new SyntaxList<SwitchLabelSyntax>();
            for (var i = 0; i < newSection.Labels.Count; i++)
            {
                var label = newSection.Labels[i];
                if (label is CasePatternSwitchLabelSyntax && ((CasePatternSwitchLabelSyntax)label).WhenClause != null)
                {
                    var modifiedCondition = ((CasePatternSwitchLabelSyntax)label).WhenClause.Condition;
                    var originalCondition = ((CasePatternSwitchLabelSyntax)node.Labels[i]).WhenClause.Condition;
                    var newModifiedCondition = TraceSyntaxGenerator.ConditionExpressionWithEndingTrace(originalCondition.Parent, modifiedCondition, this.sourceFileId);

                    var whenClauseSyntax = ((CasePatternSwitchLabelSyntax)label).WhenClause.WithCondition(newModifiedCondition);
                    label = ((CasePatternSwitchLabelSyntax)label).WithWhenClause(whenClauseSyntax);
                }

                syntaxListLabel = syntaxListLabel.Add(label);
            }
            newSection = newSection.WithLabels(syntaxListLabel);

            return newSection.WithStatements(list).WithoutTrivia().AddPreAndPostLine();
        }

        public override SyntaxNode VisitSwitchExpressionArm(SwitchExpressionArmSyntax node)
        {
            SwitchExpressionArmSyntax newArm = (SwitchExpressionArmSyntax)base.VisitSwitchExpressionArm(node);
            
            if (newArm.WhenClause != null)
            {
                var modifiedCondition = newArm.WhenClause.Condition;
                var originalCondition = node.WhenClause.Condition;
                var newModifiedCondition = 
                    TraceSyntaxGenerator.ConditionExpressionWithEndingTrace(originalCondition.Parent, modifiedCondition, this.sourceFileId);

                var whenClauseSyntax = newArm.WhenClause.WithCondition(newModifiedCondition);
                newArm = newArm.WithWhenClause(whenClauseSyntax);
            }
            else
            {
                var newModifiedCondition = TraceSyntaxGenerator.ConditionExpressionWithEndingTrace(node, this.sourceFileId);
                var newWhenClause = SyntaxFactory.WhenClause(newModifiedCondition);
                newArm = newArm.WithWhenClause(newWhenClause);
            }
            
            return newArm;
        }

        public override SyntaxNode VisitIfStatement(IfStatementSyntax node)
        {
            IfStatementSyntax stmtIfInstrumented = (IfStatementSyntax)base.VisitIfStatement(node);
            SyntaxNode modifiedCondition = base.Visit(node.Condition);

            var instrumentedScope = InstrumentScope((BlockSyntax)stmtIfInstrumented.Statement, node);
            var trace = TraceSyntaxGenerator.ConditionExpression(node, modifiedCondition, sourceFileId);

            stmtIfInstrumented = stmtIfInstrumented.WithStatement(instrumentedScope);
            stmtIfInstrumented = stmtIfInstrumented.WithCondition(trace);

            if (node.Else != null)
            {
                stmtIfInstrumented = stmtIfInstrumented.WithElse(
                    stmtIfInstrumented.Else.WithStatement(InstrumentScope((BlockSyntax)stmtIfInstrumented.Else.Statement, node))
                    .WithoutTrivia().AddPreAndPostLine());
            }
            var block = SyntaxFactory.Block();
            block = block.AddStatements(((IfStatementSyntax)stmtIfInstrumented)
                .WithoutTrivia()
                .AddPreAndPostLine());
            return block;
        }

        public override SyntaxNode VisitDoStatement(DoStatementSyntax node)
        {
            DoStatementSyntax instrumentedDo = (DoStatementSyntax)base.VisitDoStatement(node);
            var instrumentedDoScope = InstrumentScope((BlockSyntax)instrumentedDo.Statement, node);
            var doCondition = TraceSyntaxGenerator.ConditionExpression(node, instrumentedDo.Condition, sourceFileId);
            instrumentedDo = instrumentedDo.WithStatement(instrumentedDoScope);
            instrumentedDo = instrumentedDo.WithCondition(doCondition);
            return WrapLoop(SyntaxFactory.Block(instrumentedDo.WithoutTrivia().AddPreAndPostLine()), node);
        }

        public override SyntaxNode VisitWhileStatement(WhileStatementSyntax node)
        {
            WhileStatementSyntax newNode = (WhileStatementSyntax)base.VisitWhileStatement(node);
            WhileStatementSyntax stmtWhileOriginal = (WhileStatementSyntax)node;
            WhileStatementSyntax stmtWhileInstrumented = (WhileStatementSyntax)newNode;

            stmtWhileInstrumented =
                stmtWhileInstrumented.WithStatement(
                    InstrumentScope((BlockSyntax)stmtWhileInstrumented.Statement, stmtWhileOriginal));
            var stmtWhileInstrumentedCondition = Visit(stmtWhileOriginal.Condition);
            ExpressionSyntax trace = TraceSyntaxGenerator.ConditionExpression(stmtWhileOriginal, stmtWhileInstrumentedCondition, sourceFileId);
            stmtWhileInstrumented = stmtWhileInstrumented.WithCondition(trace);
            
            var block = SyntaxFactory.Block();
            block = block.AddStatements(stmtWhileInstrumented.WithoutTrivia().AddPreAndPostLine());

            return WrapLoop(block, node);
        }

        public override SyntaxNode VisitConditionalExpression(ConditionalExpressionSyntax node)
        {
            ConditionalExpressionSyntax newCondition = (ConditionalExpressionSyntax)base.VisitConditionalExpression(node);
            //var functionConditionalOperator = TraceSyntaxGenerator.ConditionalOperatorExpression(newCondition, node, Model, sourceFileId);
            var functionConditionalOperator = TraceSyntaxGenerator.ConditionalOperatorExpression_V2(newCondition, node, Model, sourceFileId);
            return functionConditionalOperator;
        }

        public override SyntaxNode VisitConditionalAccessExpression(ConditionalAccessExpressionSyntax node)
        {
            // TODO Check unsupported expression
            var exp = node.Expression;
            if (exp is ParenthesizedExpressionSyntax)
                exp = ((ParenthesizedExpressionSyntax)exp).Expression;
            if (exp.Kind() == SyntaxKind.AsExpression)
            {
                var filePath = node.GetLocation().SourceTree.FilePath;
                if (!notSupportedExpressions.ContainsKey(filePath))
                    notSupportedExpressions[filePath] = new List<string>();
                notSupportedExpressions[filePath].Add(node.ToString());
            }

            // Rewrite: A?.B as (A == null ? null : A.B)
            if ((node.Expression.RemoveParentesisAndMore() is InvocationExpressionSyntax || 
                (node.Expression.RemoveParentesisAndMore() is BinaryExpressionSyntax && 
                ((BinaryExpressionSyntax)node.Expression.RemoveParentesisAndMore()).OperatorToken.Kind() == SyntaxKind.AsKeyword)) && 
                node.WhenNotNull is InvocationExpressionSyntax)
            {
                var modifiedExpression = base.Visit(node.Expression);
                var modifiedWhenNotNull = base.Visit(node.WhenNotNull);
                return SyntaxFactory.ParseExpression(modifiedWhenNotNull.ToString().Replace("SLICER_COMPLETE", modifiedExpression.ToString()));
            }

            var modifiedFullExpression = (ConditionalAccessExpressionSyntax)base.VisitConditionalAccessExpression(node);
            
            if (node.WhenNotNull is InvocationExpressionSyntax)
            {
                var receiver = TraceSyntaxGenerator.ConditionalAccessCheckNull(node, modifiedFullExpression.Expression, sourceFileId);
                var strReceiver = receiver.ToString();
                if (!(node.Expression.RemoveParentesisAndMore() is MemberAccessExpressionSyntax))
                    strReceiver = strReceiver + "?";
                var newExp = SyntaxFactory.ParseExpression(modifiedFullExpression.WhenNotNull.ToString().Replace("SLICER_COMPLETE", strReceiver));

                //Utils.SaveConditionalAccessExpression(node, newExp, GetWrappedMethodsDictionary()); // TODOSWITCH
                return newExp;
            }

            var temp = TraceSyntaxGenerator.ConditionalAccessExpression(modifiedFullExpression, node, Model, sourceFileId);
            if (temp.Item2 != null)
            {
                var newMethods = GetWrappedMethodsDictionary();
                var spanHash = Utils.GetHashCodeFromFileAndSpan(sourceFileId, node.Span);
                if (!newMethods.ContainsKey(spanHash))
                    newMethods.Add(spanHash, temp.Item2);
            }

            return temp.Item1;
        }

        public override SyntaxNode VisitForStatement(ForStatementSyntax node)
        {
            ForStatementSyntax stmtForOriginal = (ForStatementSyntax)node;
            ForStatementSyntax stmtForInstrumented = (ForStatementSyntax)base.VisitForStatement(node);

            var newList = new SeparatedSyntaxList<ExpressionSyntax>();
            var i = 0;
            foreach (var expr in stmtForOriginal.Incrementors)
            {
                var tracing = TraceSyntaxGenerator.SimpleExpression(expr, sourceFileId);
                newList = newList.Add(tracing);
                newList = newList.Add(stmtForInstrumented.Incrementors[i]);
                i++;
            }
            newList = newList.Add(TraceSyntaxGenerator.ExitConditionExpression(stmtForOriginal, sourceFileId));

            //stmtForInstrumented = stmtForInstrumented.WithStatement(((BlockSyntax)stmtForInstrumented.Statement)
            //    .AddStatements(newList.Select(x => SyntaxFactory.ExpressionStatement(x)).ToArray()));

            stmtForInstrumented = stmtForInstrumented.WithIncrementors(newList);
            stmtForInstrumented = stmtForInstrumented.WithStatement(InstrumentScope((BlockSyntax)stmtForInstrumented.Statement, stmtForOriginal, true));

            ExpressionSyntax trace = TraceSyntaxGenerator.ConditionExpression(stmtForOriginal, stmtForInstrumented.Condition, sourceFileId);
            stmtForInstrumented = stmtForInstrumented.WithCondition(trace);

            BlockSyntax returnBlock = SyntaxFactory.Block();

            // Variable Declaration
            if (stmtForOriginal.Declaration != null)
                foreach (var variableDeclaration in stmtForOriginal.Declaration.Variables)
                    returnBlock = returnBlock.AddStatements(TraceSyntaxGenerator.SimpleStatement(variableDeclaration, sourceFileId));

            // Initializers
            newList = new SeparatedSyntaxList<ExpressionSyntax>();
            i = 0;
            foreach (var expr in stmtForOriginal.Initializers)
            {
                var tracing = TraceSyntaxGenerator.SimpleExpression(expr, sourceFileId);
                newList = newList.Add(tracing);
                newList = newList.Add(stmtForInstrumented.Initializers[i]);
                i++;
            }
            stmtForInstrumented = stmtForInstrumented.WithInitializers(newList);
            returnBlock = returnBlock.AddStatements(stmtForInstrumented);

            return WrapLoop(returnBlock, stmtForOriginal.WithoutTrivia().AddPreAndPostLine());
        }

        public override SyntaxNode VisitForEachVariableStatement(ForEachVariableStatementSyntax node)
        {
            ForEachVariableStatementSyntax stmtForEachInstrumented = (ForEachVariableStatementSyntax)base.VisitForEachVariableStatement(node);

            stmtForEachInstrumented = stmtForEachInstrumented.WithStatement(InstrumentScope((BlockSyntax)stmtForEachInstrumented.Statement, node));
            // XXX: Agregamos el EndInvocation para la llamada implícita al GetEnumerator.
            // Nota: Si ya hay un GetEnumerator no pasa nada.

            // *3 For differencing from a possible internal call
            var spanHash = Utils.GetHashCodeFromFileAndSpan(sourceFileId, node.Span) * 3;
            MethodDeclarationSyntax methodDecl = null;
            var newMethods = GetWrappedMethodsDictionary();
            if (newMethods.ContainsKey(spanHash))
                methodDecl = newMethods[spanHash];
            if (methodDecl == null)
            {
                methodDecl = TraceSyntaxGenerator.ExpressionWrapped(node.Expression, Model, sourceFileId, TraceSyntaxGenerator.AccessType.Invocation);
                newMethods.Add(spanHash, methodDecl);
            }
            var modifiedExpression = SyntaxFactory.ParseExpression(string.Format(" {0}({1}) ", methodDecl.Identifier.ValueText, stmtForEachInstrumented.Expression));

            stmtForEachInstrumented = SyntaxFactory.ForEachVariableStatement(node.AttributeLists,
                node.Variable, modifiedExpression, stmtForEachInstrumented.Statement);

            BlockSyntax newBlock = SyntaxFactory.Block();
            var stmt = TraceSyntaxGenerator.SimpleStatement(node, sourceFileId);
            newBlock = newBlock.AddStatements(stmt);

            newBlock = newBlock.AddStatements(stmtForEachInstrumented);

            return WrapLoop(newBlock, node.WithoutTrivia().AddPreAndPostLine());
        }

        public override SyntaxNode VisitForEachStatement(ForEachStatementSyntax node)
        {
            ForEachStatementSyntax stmtForEachInstrumented = (ForEachStatementSyntax)base.VisitForEachStatement(node);

            if (node.ToString().Contains("(var entry in Context.InitialSessionState.Types)"))
                ;

            stmtForEachInstrumented = stmtForEachInstrumented.WithStatement(InstrumentScope((BlockSyntax)stmtForEachInstrumented.Statement, node));
            // XXX: Agregamos el EndInvocation para la llamada implícita al GetEnumerator.
            // Nota: Si ya hay un GetEnumerator no pasa nada.

            // *3 For differencing from a possible internal call
            var spanHash = Utils.GetHashCodeFromFileAndSpan(sourceFileId, node.Span)*3;
            MethodDeclarationSyntax methodDecl = null;
            var newMethods = GetWrappedMethodsDictionary();
            if (newMethods.ContainsKey(spanHash))
                methodDecl = newMethods[spanHash];
            if (methodDecl == null)
            {
                methodDecl = TraceSyntaxGenerator.ExpressionWrapped(node.Expression, Model, sourceFileId, TraceSyntaxGenerator.AccessType.Invocation);
                newMethods.Add(spanHash, methodDecl);
            }
            var modifiedExpression = SyntaxFactory.ParseExpression(string.Format(" {0}({1}) ", methodDecl.Identifier.ValueText, stmtForEachInstrumented.Expression));
            
            stmtForEachInstrumented = SyntaxFactory.ForEachStatement(stmtForEachInstrumented.Type,
                stmtForEachInstrumented.Identifier, modifiedExpression, stmtForEachInstrumented.Statement);

            BlockSyntax newBlock = SyntaxFactory.Block();
            var stmt = TraceSyntaxGenerator.SimpleStatement(node, sourceFileId);
            newBlock = newBlock.AddStatements(stmt);

            newBlock = newBlock.AddStatements(stmtForEachInstrumented);

            return WrapLoop(newBlock, node.WithoutTrivia().AddPreAndPostLine());
        }

        public override SyntaxNode VisitSwitchStatement(SwitchStatementSyntax node)
        {
            SwitchStatementSyntax newNode = (SwitchStatementSyntax)base.VisitSwitchStatement(node);
            SwitchStatementSyntax stmtSwitchOriginal = (SwitchStatementSyntax)node;
            SwitchStatementSyntax stmtSwitchInstrumented = (SwitchStatementSyntax)newNode;

            SyntaxList<SwitchSectionSyntax> sections = new SyntaxList<SwitchSectionSyntax>();
            foreach (var section in stmtSwitchInstrumented.Sections)
            {
                SyntaxList<StatementSyntax> stmtList = new SyntaxList<StatementSyntax>();
                stmtList = stmtList.Add(TraceSyntaxGenerator.EnterConditionStatement(stmtSwitchOriginal, sourceFileId));
                stmtList = stmtList.AddRange(section.Statements);
                stmtList = stmtList.Add(TraceSyntaxGenerator.ExitConditionStatement(stmtSwitchOriginal, sourceFileId));

                var newSection = section.WithStatements(stmtList);
                sections = sections.Add(newSection);
            }

            stmtSwitchInstrumented = stmtSwitchInstrumented.WithSections(sections);
            BlockSyntax newBlock = SyntaxFactory.Block();
            newBlock = newBlock.AddStatements(TraceSyntaxGenerator.SimpleStatement(stmtSwitchOriginal, sourceFileId));
            newBlock = newBlock.AddStatements(stmtSwitchInstrumented.WithoutTrivia().AddPreAndPostLine());
            return newBlock;
        }

        public override SyntaxNode VisitSwitchExpression(SwitchExpressionSyntax node)
        {
            return base.VisitSwitchExpression(node);
        }

        public override SyntaxNode VisitLockStatement(LockStatementSyntax node)
        {
            LockStatementSyntax newNode = (LockStatementSyntax)base.VisitLockStatement(node);
            LockStatementSyntax lockStmt = (LockStatementSyntax)node;
            BlockSyntax newBlock = SyntaxFactory.Block();
            newBlock = newBlock.AddStatements(TraceSyntaxGenerator.SimpleStatement(lockStmt.Expression, sourceFileId));
            newBlock = newBlock.AddStatements(newNode);
            return newBlock;
        }

        public override SyntaxNode VisitEmptyStatement(EmptyStatementSyntax node)
        {
            return InstrumentAndConvertToBlock(node, node);
        }

        public override SyntaxNode VisitExpressionStatement(ExpressionStatementSyntax node)
        {
            var modifiedExpression = (ExpressionStatementSyntax)base.VisitExpressionStatement(node);
            return InstrumentAndConvertToBlock(node, modifiedExpression.WithoutTrivia().AddPreAndPostLine());
        }

        public override SyntaxNode VisitReturnStatement(ReturnStatementSyntax node)
        {
            try
            { 
                var modifiedReturn = (ReturnStatementSyntax)base.VisitReturnStatement(node);
                return InstrumentAndConvertToBlock(node, modifiedReturn.WithoutTrivia().AddPreAndPostLine());
            }
            catch(Exception ex)
            {
                throw;
            }
        }

        // XXX: Los lambda no se instrumentan
        public override SyntaxNode VisitSimpleLambdaExpression(SimpleLambdaExpressionSyntax node)
        {
            return node;
        }

        // XXX: Por ahora no instrumentaremos los métodos anónimos
        public override SyntaxNode VisitAnonymousMethodExpression(AnonymousMethodExpressionSyntax node)
        {
            return node;
        }

        // XXX: Las querys no se instrumentan
        public override SyntaxNode VisitQueryExpression(QueryExpressionSyntax node)
        {
            /*
            [from unaCasa in lista] == > FROM CLAUSE
            [join otraCasa in lista2 
            on unaCasa.cantVentanas equals otraCasa.cantVentanas - variableAuxiliar2
            where otraCasa.cantVentanas < variableAuxiliar3
            select new { unaCasa.cantVentanas, unaCasa.propietario.nombre, variableAuxiliar }] ==> BODY
            */

            // TODOQUERYS
            //var recFrom = Visit(node.FromClause);

            //// CLÁUSULAS DEL BODY: Hay varias. Pueden ser WHERE/JOIN... cada una se puede reescribir en LINQ
            //var rewritedClauses = new List<SyntaxNode>();
            //foreach (var clause in node.Body.Clauses)
            //    rewritedClauses.Add(Visit(clause));

            return TraceSyntaxGenerator.InvocationWrapperExpression(node, node, sourceFileId);
            //return node;
        }

        public override SyntaxNode VisitObjectCreationExpression(ObjectCreationExpressionSyntax node)
        {
            try
            {

                var modifiedObjCreation = (ObjectCreationExpressionSyntax)base.VisitObjectCreationExpression(node);

                var list = new SeparatedSyntaxList<ExpressionSyntax>();
                if (node.Initializer != null && node.Initializer.Expressions.Count != 0)
                {
                    bool firstInitializer = true;
                    foreach (var expr in modifiedObjCreation.Initializer.Expressions)
                    {
                        if (firstInitializer)
                        {
                            if (expr is AssignmentExpressionSyntax)
                            {
                                var assignExpr = expr as AssignmentExpressionSyntax;
                                var modifiedRight = TraceSyntaxGenerator.InitializerWrapperExpression(assignExpr.Right, Model, node, sourceFileId);
                                assignExpr = assignExpr.WithRight(modifiedRight);
                                list = list.Add(assignExpr);
                            }
                            else if (expr is InitializerExpressionSyntax)
                            {
                                // XXX: Caso Dictionary.
                                var complexInitExpr = expr as InitializerExpressionSyntax;
                                bool firstInitExpr = true; //TODO: Esto es igual a lo de arriba... algo se puede hacer...
                                var listComplexExpressions = new SeparatedSyntaxList<ExpressionSyntax>();
                                foreach (var initExpr in complexInitExpr.Expressions)
                                {
                                    if (firstInitExpr)
                                    {
                                        var modifInitExpr = TraceSyntaxGenerator.InitializerWrapperExpression(initExpr, Model, node, sourceFileId);
                                        listComplexExpressions = listComplexExpressions.Add(modifInitExpr);
                                        firstInitExpr = false;
                                    }
                                    else
                                        listComplexExpressions = listComplexExpressions.Add(initExpr);
                                }
                                complexInitExpr = complexInitExpr.WithExpressions(listComplexExpressions);
                                list = list.Add(complexInitExpr);
                            }
                            else
                            {
                                var first = TraceSyntaxGenerator.InitializerWrapperExpression(expr, Model, node, sourceFileId);
                                list = list.Add(first);
                            }
                            firstInitializer = false;
                        }
                        else
                            list = list.Add(expr);
                    }
                    modifiedObjCreation = modifiedObjCreation.WithInitializer(modifiedObjCreation.Initializer.WithExpressions(list));
                    return modifiedObjCreation;
                }
                else
                {
                    //return TraceSyntaxGenerator.InvocationWrapperExpression(modifiedObjCreation, node, sourceFileId);

                    if (Model.GetOperation(node).Kind == OperationKind.DelegateCreation)
                        return modifiedObjCreation;

                    var newMethods = GetWrappedMethodsDictionary();
                    var spanHash = Utils.GetHashCodeFromFileAndSpan(sourceFileId, node.Span);
                    MethodDeclarationSyntax methodDecl = null;
                    if (newMethods.ContainsKey(spanHash))
                        methodDecl = newMethods[spanHash];
                    if (methodDecl == null)
                    {
                        methodDecl = TraceSyntaxGenerator.GenericInvocationWrapped(modifiedObjCreation, Model, node, sourceFileId);
                        newMethods.Add(spanHash, methodDecl);
                    }

                    var arg_list = "";
                    if (modifiedObjCreation.ArgumentList != null)
                        foreach (var arg in modifiedObjCreation.ArgumentList.Arguments)
                            arg_list = arg_list + arg.ToString() + ", ";
                    if (arg_list.Length > 0)
                        arg_list = arg_list.Substring(0, arg_list.Length - 2);

                    return SyntaxFactory.ParseExpression(string.Format("{0}({1})", methodDecl.Identifier.ValueText, arg_list));
                }
            }
            catch (Exception ex)
            {
                //Console.WriteLine(node.ToString() + "&" + sourceFileId + "&" + node.SpanStart + "&" + node.Span.End);
                //throw;

                // BugLogging.Log(sourceFileId, node, BugLogging.Behavior.WithoutSymbol);

                return node;
            }
        }

        public override SyntaxNode VisitImplicitObjectCreationExpression(ImplicitObjectCreationExpressionSyntax node)
        {
            var modifiedObjCreation = (ImplicitObjectCreationExpressionSyntax)base.VisitImplicitObjectCreationExpression(node);
            SyntaxNode retObj = null;

            var list = new SeparatedSyntaxList<ExpressionSyntax>();
            if (node.Initializer != null && node.Initializer.Expressions.Count != 0)
            {
                bool firstInitializer = true;
                foreach (var expr in modifiedObjCreation.Initializer.Expressions)
                {
                    if (firstInitializer)
                    {
                        if (expr is AssignmentExpressionSyntax)
                        {
                            var assignExpr = expr as AssignmentExpressionSyntax;
                            var modifiedRight = TraceSyntaxGenerator.InitializerWrapperExpression(assignExpr.Right, Model, node, sourceFileId);
                            assignExpr = assignExpr.WithRight(modifiedRight);
                            list = list.Add(assignExpr);
                        }
                        else if (expr is InitializerExpressionSyntax)
                        {
                            // XXX: Caso Dictionary.
                            var complexInitExpr = expr as InitializerExpressionSyntax;
                            bool firstInitExpr = true; //TODO: Esto es igual a lo de arriba... algo se puede hacer...
                            var listComplexExpressions = new SeparatedSyntaxList<ExpressionSyntax>();
                            foreach (var initExpr in complexInitExpr.Expressions)
                            {
                                if (firstInitExpr)
                                {
                                    var modifInitExpr = TraceSyntaxGenerator.InitializerWrapperExpression(initExpr, Model, node, sourceFileId);
                                    listComplexExpressions = listComplexExpressions.Add(modifInitExpr);
                                    firstInitExpr = false;
                                }
                                else
                                    listComplexExpressions = listComplexExpressions.Add(initExpr);
                            }
                            complexInitExpr = complexInitExpr.WithExpressions(listComplexExpressions);
                            list = list.Add(complexInitExpr);
                        }
                        else
                        {
                            var first = TraceSyntaxGenerator.InitializerWrapperExpression(expr, Model, node, sourceFileId);
                            list = list.Add(first);
                        }
                        firstInitializer = false;
                    }
                    else
                        list = list.Add(expr);
                }
                modifiedObjCreation = modifiedObjCreation.WithInitializer(modifiedObjCreation.Initializer.WithExpressions(list));
                retObj = modifiedObjCreation;
            }
            else
            {
                //return TraceSyntaxGenerator.InvocationWrapperExpression(modifiedObjCreation, node, sourceFileId);

                if (Model.GetOperation(node).Kind == OperationKind.DelegateCreation)
                    retObj = modifiedObjCreation;
                else
                { 
                    var newMethods = GetWrappedMethodsDictionary();
                    var spanHash = Utils.GetHashCodeFromFileAndSpan(sourceFileId, node.Span);
                    MethodDeclarationSyntax methodDecl = null;
                    if (newMethods.ContainsKey(spanHash))
                        methodDecl = newMethods[spanHash];
                    if (methodDecl == null)
                    {
                        methodDecl = TraceSyntaxGenerator.GenericInvocationWrapped(modifiedObjCreation, Model, node, sourceFileId);
                        newMethods.Add(spanHash, methodDecl);
                    }

                    var arg_list = "";
                    if (modifiedObjCreation.ArgumentList != null)
                        foreach (var arg in modifiedObjCreation.ArgumentList.Arguments)
                            arg_list = arg_list + arg.ToString() + ", ";
                    if (arg_list.Length > 0)
                        arg_list = arg_list.Substring(0, arg_list.Length - 2);

                    retObj = SyntaxFactory.ParseExpression(string.Format("{0}({1})", methodDecl.Identifier.ValueText, arg_list));
                }
            }

            var filePath = node.GetLocation().SourceTree.FilePath;
            if (!infoExpressions.ContainsKey(filePath))
                infoExpressions[filePath] = new List<string>();

            var t_spanHash = Utils.GetHashCodeFromFileAndSpan(sourceFileId, node.Span);
            MethodDeclarationSyntax t_methodDecl = null;
            var t_newMethods = GetWrappedMethodsDictionary();
            if (t_newMethods.ContainsKey(t_spanHash))
                t_methodDecl = t_newMethods[t_spanHash];
            var s_wrapperMethod = t_methodDecl.ToString();
            infoExpressions[filePath].Add(node.ToString() + "\r\n" + retObj.ToString() + "\r\n\r\n" + s_wrapperMethod + "\r\n\r\n" + "\r\n");
            return retObj;
        }

        public override SyntaxNode VisitLocalDeclarationStatement(LocalDeclarationStatementSyntax node)
        {
            var modifiedDeclaration = (LocalDeclarationStatementSyntax)base.VisitLocalDeclarationStatement(node);
            IList<VariableDeclaratorSyntax> modifiedVariables = new List<VariableDeclaratorSyntax>();
            foreach (var variable in modifiedDeclaration.Declaration.Variables)
            {
                if (variable.Initializer == null)
                {
                    var init = SyntaxFactory.EqualsValueClause(SyntaxFactory.DefaultExpression(node.Declaration.Type.WithoutTrivia()));
                    var modifiedVar = variable.WithInitializer(init);
                    modifiedVariables.Add(modifiedVar);
                }
                else
                    modifiedVariables.Add(variable);
            }
            var modifiedDecl = modifiedDeclaration.Declaration.WithVariables(SyntaxFactory.SeparatedList(modifiedVariables));
            modifiedDeclaration = modifiedDeclaration.WithDeclaration(modifiedDecl);
            return InstrumentAndConvertToBlock(node, modifiedDeclaration.WithoutTrivia().AddPreAndPostLine());
        }

        public override SyntaxNode VisitBreakStatement(BreakStatementSyntax node)
        {
            var modifiedNode = node.WithoutTrivia().AddPreAndPostLine();
            BlockSyntax newBlock = SyntaxFactory.Block();
            var stmt = TraceSyntaxGenerator.BreakStatement(node, sourceFileId);
            newBlock = newBlock.AddStatements(stmt);
            newBlock = newBlock.AddStatements(modifiedNode);
            return newBlock.WithoutTrivia().AddPreAndPostLine();
        }

        public override SyntaxNode VisitCheckedExpression(CheckedExpressionSyntax node)
        {
            return base.VisitCheckedExpression(node);
        }

        public override SyntaxNode VisitContinueStatement(ContinueStatementSyntax node)
        {
            return InstrumentAndConvertToBlock(node, node.WithoutTrivia().AddPreAndPostLine());
        }

        public override SyntaxNode VisitFixedStatement(FixedStatementSyntax node)
        {
            return base.VisitFixedStatement(node);
        }

        public override SyntaxNode VisitYieldStatement(YieldStatementSyntax node)
        {
            var recNode = base.Visit(node.Expression);
            BlockSyntax newBlock = SyntaxFactory.Block();
            var traceInstr = TraceSyntaxGenerator.SimpleStatement(node, sourceFileId);
            newBlock = newBlock.AddStatements(traceInstr);
            StatementSyntax newStmt = null;
            // Es un yield break
            if (node.Expression != null)
                newStmt = SyntaxFactory.ParseStatement(listYieldReturn + ".Add(" + recNode.ToString() + ");").AddPreAndPostLine();
            else
                newStmt = currentMethodInfo.returnYieldReturnStmt;
            newBlock = newBlock.AddStatements(newStmt);
            return newBlock;
        }

        public override SyntaxNode VisitUsingStatement(UsingStatementSyntax node)
        {
            /* Si el contenido del Using es una expresion node.Expression tiene el nodo
             * y Declarion es null. En caso contrario, exactamente lo opuesto */
            var expression = (ExpressionSyntax)base.Visit(node.Expression);
            var declaration = (VariableDeclarationSyntax)base.Visit(node.Declaration); 
            var bodyBlock = (BlockSyntax)base.Visit(node.Statement);

            bodyBlock = ((BlockSyntax)bodyBlock).AddStatements(new StatementSyntax[] {  
                    TraceSyntaxGenerator.ExitUsingStatement(node, sourceFileId)
                });

            var newUsing = SyntaxFactory.UsingStatement(declaration, expression, bodyBlock);

            var block = SyntaxFactory.Block(new StatementSyntax[] {  
                    TraceSyntaxGenerator.SimpleStatement(node, sourceFileId),
                    newUsing });
            return block;
        }

        public override SyntaxNode VisitTryStatement(TryStatementSyntax node)
        {
            var instrumentedTryStmt = (TryStatementSyntax)base.VisitTryStatement(node);
            BlockSyntax newBlock = SyntaxFactory.Block();
            newBlock = newBlock.AddStatements(instrumentedTryStmt);
            return newBlock.WithoutTrivia().AddPreAndPostLine();
        }

        public override SyntaxNode VisitCatchClause(CatchClauseSyntax node)
        {
            var instrumentedCatchClause = (CatchClauseSyntax)base.VisitCatchClause(node);
            SyntaxList<StatementSyntax> stmtList = new SyntaxList<StatementSyntax>();
            stmtList = stmtList.Add(TraceSyntaxGenerator.EnterCatchStatement(node, sourceFileId));
            stmtList = stmtList.AddRange(instrumentedCatchClause.Block.Statements);
            stmtList = stmtList.Add(TraceSyntaxGenerator.ExitCatchStatement(node, sourceFileId));
            instrumentedCatchClause = instrumentedCatchClause.WithBlock(instrumentedCatchClause.Block.WithStatements(stmtList));
            return instrumentedCatchClause;
        }

        public override SyntaxNode VisitFinallyClause(FinallyClauseSyntax node)
        {
            var instrumentedFinallyClause = (FinallyClauseSyntax)base.VisitFinallyClause(node);
            SyntaxList<StatementSyntax> stmtList = new SyntaxList<StatementSyntax>();
            stmtList = stmtList.Add(TraceSyntaxGenerator.EnterFinallyStatement(node, sourceFileId));
            stmtList = stmtList.AddRange(instrumentedFinallyClause.Block.Statements);
            stmtList = stmtList.Add(TraceSyntaxGenerator.ExitFinallyStatement(node, sourceFileId));
            instrumentedFinallyClause = instrumentedFinallyClause.WithBlock(instrumentedFinallyClause.Block.WithStatements(stmtList));
            return instrumentedFinallyClause;
        }

        public override SyntaxNode VisitUnsafeStatement(UnsafeStatementSyntax node)
        {
            return base.VisitUnsafeStatement(node);
        }

        public override SyntaxNode VisitGlobalStatement(GlobalStatementSyntax node)
        {
            return base.VisitGlobalStatement(node);
        }

        public override SyntaxNode VisitGotoStatement(GotoStatementSyntax node)
        {
            // We don't deal with this expression
            throw new GoToStatementException();

            //var instrumentedGotoStmt = (GotoStatementSyntax)base.VisitGotoStatement(node);
            //BlockSyntax newBlock = SyntaxFactory.Block();
            //newBlock = newBlock.AddStatements(instrumentedGotoStmt.WithoutTrivia().AddPreAndPostLine());
            //return newBlock;
        }

        public override SyntaxNode VisitDefaultExpression(DefaultExpressionSyntax node)
        {
            return base.VisitDefaultExpression(node);
        }

        public override SyntaxNode VisitThrowStatement(ThrowStatementSyntax node)
        {
            var instrumentedThrowStmt = (ThrowStatementSyntax)base.VisitThrowStatement(node);
            BlockSyntax newBlock = SyntaxFactory.Block();
            var stmt = TraceSyntaxGenerator.SimpleStatement(node, sourceFileId);
            newBlock = newBlock.AddStatements(stmt, instrumentedThrowStmt.WithoutTrivia().AddPreAndPostLine());
            return newBlock;
        }

        public override SyntaxNode VisitLabeledStatement(LabeledStatementSyntax node)
        {
            var instrumentedLabelStmt = (BlockSyntax)((LabeledStatementSyntax)base.VisitLabeledStatement(node)).Statement;
            var modifiedLabel = node.WithStatement(instrumentedLabelStmt.Statements[0]);

            BlockSyntax newBlock = SyntaxFactory.Block();
            if (instrumentedLabelStmt.Statements.Count > 1)
                newBlock = newBlock.AddStatements(modifiedLabel, instrumentedLabelStmt.Statements[1]);
            else
                newBlock = newBlock.AddStatements(modifiedLabel);
            return newBlock;
        }

        public override SyntaxNode VisitFieldDeclaration(FieldDeclarationSyntax node)
        {
            return ((FieldDeclarationSyntax)base.VisitFieldDeclaration(node)).WithoutTrivia().AddPreAndPostLine();
        }

        public override SyntaxNode VisitPropertyDeclaration(PropertyDeclarationSyntax node)
        {
            var modifiedProperty = (PropertyDeclarationSyntax)base.VisitPropertyDeclaration(node);
            var modifiedAccessors = new SyntaxList<AccessorDeclarationSyntax>();
            if (modifiedProperty.AccessorList != null && modifiedProperty.AccessorList.Accessors != null)
            {
                foreach (var accessor in modifiedProperty.AccessorList.Accessors)
                {
                    var modifiedAccessor = accessor;
                    if (accessor.Body != null)
                        modifiedAccessor = accessor.WithBody(WrapBlock(accessor.Body, node, accessor.Keyword.WithoutTrivia().ValueText == "get"));
                    modifiedAccessors = modifiedAccessors.Add(modifiedAccessor);
                }
            }
            var modifiedAccessorList = SyntaxFactory.AccessorList(modifiedAccessors);
            if (modifiedAccessorList.Accessors.Count > 0)
                modifiedProperty = modifiedProperty.WithAccessorList(modifiedAccessorList);

            // Si tiene expression body, lo ponemos como getter
            if (modifiedProperty.ExpressionBody != null)
            {
                var enter_prop = TraceSyntaxGenerator.EnterMethodStatement(node.ExpressionBody, sourceFileId);
                var stmt_stmt = TraceSyntaxGenerator.SimpleStatement(node.ExpressionBody.Expression, sourceFileId);
                var ret_stmt = SyntaxFactory.ParseStatement(
                    (node.ExpressionBody.Expression is ThrowExpressionSyntax ? "" : "return ") + 
                    modifiedProperty.ExpressionBody.Expression.ToString() + ";");
                var exit_prop = TraceSyntaxGenerator.ExitMethodStatement(node.ExpressionBody, sourceFileId);
                
                var new_block = WrapBlock(SyntaxFactory.Block(enter_prop, stmt_stmt, ret_stmt, exit_prop), node.ExpressionBody, true);
                var accessor = SyntaxFactory.AccessorDeclaration(SyntaxKind.GetAccessorDeclaration, modifiedProperty.AttributeLists, SyntaxFactory.TokenList(), new_block);
                if (modifiedProperty.AccessorList != null)
                    modifiedProperty = modifiedProperty.WithAccessorList(modifiedProperty.AccessorList.AddAccessors(accessor));
                else
                {
                    var syntax_list = new SyntaxList<AccessorDeclarationSyntax>();
                    syntax_list = syntax_list.Add(accessor);
                    modifiedProperty = modifiedProperty.WithAccessorList(SyntaxFactory.AccessorList(syntax_list));
                }
            }

            // Removemos el initializer
            var newPropDeclaration = SyntaxFactory.PropertyDeclaration(modifiedProperty.AttributeLists, modifiedProperty.Modifiers, modifiedProperty.Type, modifiedProperty.ExplicitInterfaceSpecifier,
                modifiedProperty.Identifier, modifiedProperty.AccessorList, null, null).WithoutTrivia().AddPreAndPostLine();
            return newPropDeclaration;
        }

        public override SyntaxNode VisitInvocationExpression(InvocationExpressionSyntax node)
        {
            try
            {
                var operation = Model.GetOperation(node);
                
                // We don't instrument "nameof" expressions because they don't throw callbacks
                if (operation != null && operation.Kind == OperationKind.NameOf)
                    return node;
                
                var modifiedInvocation = (InvocationExpressionSyntax)base.VisitInvocationExpression(node);
                var symbol = Utils.GetMethodSymbolInfo(Model, node);

                #region Arguments
                var symParams = new ImmutableArray<IParameterSymbol>();
                if (symbol != null && symbol.Parameters != null)
                    symParams = symbol.Parameters;
                if (modifiedInvocation.ArgumentList != null && modifiedInvocation.ArgumentList.Arguments != null)
                {
                    var modifiedArguments = new SeparatedSyntaxList<ArgumentSyntax>();
                    int i = 0;
                    foreach (var arg in modifiedInvocation.ArgumentList.Arguments)
                    {
                        string typeName = null;
                        IParameterSymbol symParam = null;
                        if (symParams != null && arg.NameColon != null)
                            symParam = symParams.Where(x => x.Name == arg.NameColon.Name.Identifier.ValueText).Single();
                        else if (symParams != null && symParams.Length > i)
                            symParam = symParams[i];

                        if (symParam != null)
                        {
                            typeName = symParam.Type.ToString();
                            if (symParam.Type.TypeKind == TypeKind.Array && symParam.IsParams &&
                                node.ArgumentList.Arguments[i].Expression.GetText().ToString().Trim() != "null" &&
                                Model.GetOperation(node.ArgumentList.Arguments[i].Expression.RemoveParentesisAndMore()).Type.TypeKind != TypeKind.Array)
                            {
                                // XXX: Si es array y params le sacamos el "[]".
                                typeName = typeName.Remove(typeName.IndexOf("["));
                            }
                        }

                        // Se declara la variable inline
                        if (arg.Expression is DeclarationExpressionSyntax && symParam.RefKind == RefKind.Out)
                        {
                            var varName = ((DeclarationExpressionSyntax)(arg.Expression)).Designation.GetText().ToString();
                            if (currentMethodInfo.outParams.Add(varName))
                                currentMethodInfo.outParamsInline.Add(SyntaxFactory.ParseStatement(typeName + " " + varName + " = default(" + typeName + ");").AddPostLine());
                            modifiedArguments = modifiedArguments.Add(arg.WithExpression(SyntaxFactory.ParseExpression(varName)).AddPostLine().AddSpace());
                        }
                        else if (arg.Expression is CastExpressionSyntax)
                            // Not always... XXX
                            modifiedArguments = modifiedArguments.Add(SyntaxFactory.Argument(((CastExpressionSyntax)arg.Expression).Expression));
                        else
                            modifiedArguments = modifiedArguments.Add(arg);
                        i++;
                    }

                    modifiedInvocation = modifiedInvocation.WithArgumentList(SyntaxFactory.ArgumentList(modifiedArguments));
                }
                #endregion

                #region Don't wrap structs (it depends on the configuration)
                if (!Globals.wrap_structs_calls && operation != null &&
                    symbol != null && symbol.CustomStructReceiver())
                    return modifiedInvocation;
                #endregion

                #region Current arguments
                var arg_list = "";
                var j = 0;
                foreach (var arg in modifiedInvocation.ArgumentList.Arguments)
                {
                    if (arg.RefKindKeyword.ValueText == "in")
                        arg_list = arg_list + arg.Expression.ToString() + ", ";
                    else
                        arg_list = arg_list + arg.ToString() + ", ";
                    j++;
                }
                if (arg_list.Length > 0)
                    arg_list = arg_list.Substring(0, arg_list.Length - 2);
                #endregion

                #region Ref Receiver
                // Si la expresión visitada es un parámetro/variable local/this o base, o field access, entonces tenemos que enviarlo por referencia 
                // Cualquier de estos casos no se wrappean. Es decir, no wrappeamos ni parámetros ni fields, entonces podemos preguntar directamente por los mismos

                var ref_parameter = false;
                if (node.Expression is MemberAccessExpressionSyntax)
                {
                    var expression = ((MemberAccessExpressionSyntax)node.Expression).Expression;
                    var visitedKind = expression.Kind();
                    switch (visitedKind)
                    {
                        // Por ahora lo wrappeamos como antes
                        case SyntaxKind.BaseExpression:
                            return TraceSyntaxGenerator.InvocationWrapperExpression(modifiedInvocation, node, sourceFileId);
                        case SyntaxKind.IdentifierName:
                            // Acá tenemos que ver si es local variable o parámeter
                            var exp_op_iden = Model.GetOperation(expression);
                            if (exp_op_iden != null)
                                switch (exp_op_iden.Kind)
                                {
                                    case OperationKind.LocalReference:
                                        var syntax_ref = ((ILocalReferenceOperation)exp_op_iden).Local.DeclaringSyntaxReferences.Last();
                                        var is_foreach =
                                            syntax_ref.SyntaxTree.GetRoot()
                                            .DescendantNodes().Where(x => x.Span.Start == syntax_ref.Span.Start
                                            && x.Span.End == syntax_ref.Span.End).FirstOrDefault();
                                        ref_parameter = !(is_foreach is ForEachStatementSyntax);
                                        break;
                                    case OperationKind.ParameterReference:
                                        ref_parameter = true;
                                        break;
                                    default:
                                        break;
                                }
                            else
                                ;
                            break;
                        case SyntaxKind.SimpleMemberAccessExpression:
                            var exp_op_mem = Model.GetOperation(expression);
                            if (exp_op_mem != null)
                            {
                                if (exp_op_mem.Kind == OperationKind.FieldReference)
                                    ref_parameter = true;
                            }
                            else
                                ;
                            break;
                        case SyntaxKind.ThisExpression:
                            ref_parameter = true;
                            break;
                        default:
                            break;
                    }
                }
                else if (node.Expression is MemberBindingExpressionSyntax)
                {
                    // TODO (XXX): ESTO NO ES SIEMPRE ASÍ! Hay que analizarlo posta. Pondría false para dejarlo así.
                    ref_parameter = false;
                }
                // TODO ROSLYN
                else if (node.Expression is IdentifierNameSyntax && operation.Kind != OperationKind.Invalid) 
                {
                    if (operation is IDynamicInvocationOperation)
                        ref_parameter = ((IDynamicMemberReferenceOperation)((IDynamicInvocationOperation)operation).Operation).Instance is IFieldReferenceOperation;
                    else
                        ref_parameter = ((IInvocationOperation)operation).Instance != null && ((IInvocationOperation)operation).Instance is IFieldReferenceOperation;
                }
                #endregion

                #region Get previous method declaration
                var newMethods = GetWrappedMethodsDictionary();
                var spanHash = Utils.GetHashCodeFromFileAndSpan(sourceFileId, node.Span);
                MethodDeclarationSyntax methodDecl = null;
                if (newMethods.ContainsKey(spanHash))
                    methodDecl = newMethods[spanHash];
                #endregion

                #region Special case: conditional access instance operation
                if (operation is IInvocationOperation && ((IInvocationOperation)operation).Instance is IConditionalAccessInstanceOperation)
                {
                    var invocationName = ((MemberBindingExpressionSyntax)node.Expression).Name.ToString();
                    if (operation.Type.Name.ToString() == "Void")
                    {
                        var invExp = SyntaxFactory.ParseExpression(string.Format("SLICER_COMPLETE.{0}({1})", invocationName, arg_list));
                        return TraceSyntaxGenerator.InvocationWrapperExpression(invExp, node, sourceFileId);
                    }
                    else
                    { 
                        if (methodDecl == null)
                        {
                            methodDecl = TraceSyntaxGenerator.ExpressionWrapped(node.Parent, Model, sourceFileId, TraceSyntaxGenerator.AccessType.Invocation);
                            newMethods.Add(spanHash, methodDecl);
                        }
                        return SyntaxFactory.ParseExpression(string.Format("{0}(SLICER_COMPLETE.{1}({2}))", methodDecl.Identifier.ValueText, invocationName, arg_list));
                    }
                }
                #endregion

                #region Build invocation expression
                var static_receiver = symbol != null && symbol.IsStatic;
                var l_ref_parameter = static_receiver ? false : ref_parameter;
                if (methodDecl == null)
                {
                    methodDecl = TraceSyntaxGenerator.GenericInvocationWrapped(modifiedInvocation, Model, node, sourceFileId, l_ref_parameter);
                    newMethods.Add(spanHash, methodDecl);
                }

                if (static_receiver)
                    return SyntaxFactory.ParseExpression(string.Format("{0}({1})", methodDecl.Identifier.ValueText, arg_list));
                else
                {
                    #region Receiver
                    var receiver = "";
                    var ref_str = "";
                    if (modifiedInvocation.Expression is MemberAccessExpressionSyntax)
                    {
                        var exp = node.Expression;
                        var exp_op = Model.GetOperation(exp);
                        if (exp_op != null && exp_op.Kind == OperationKind.EventReference)
                            // Va con el evento completo
                            receiver = modifiedInvocation.Expression.GetText().ToString().Trim();
                        else
                        {
                            var ref_op = Model.GetOperation(((MemberAccessExpressionSyntax)exp).Expression.RemoveParentesisAndMore());
                            ref_str = ref_parameter && ref_op.Type.CustomIsStruct() ? "ref " : "";
                            receiver = ((MemberAccessExpressionSyntax)modifiedInvocation.Expression).Expression.GetText().ToString();
                        }
                    }
                    else if (node.Parent is ConditionalAccessExpressionSyntax && node == ((ConditionalAccessExpressionSyntax)node.Parent).WhenNotNull && (((ConditionalAccessExpressionSyntax)node.Parent).Expression is InvocationExpressionSyntax ||
                        ((ConditionalAccessExpressionSyntax)node.Parent).Expression is MemberAccessExpressionSyntax ||
                        (((ConditionalAccessExpressionSyntax)node.Parent).Expression.RemoveParentesisAndMore() is BinaryExpressionSyntax && ((BinaryExpressionSyntax)((ConditionalAccessExpressionSyntax)node.Parent).Expression.RemoveParentesisAndMore()).OperatorToken.Kind() == SyntaxKind.AsKeyword)))
                    {
                        receiver = "SLICER_COMPLETE";
                    }
                    else
                    {
                        var op = ((IInvocationOperation)operation).Instance;
                        while (op is IConversionOperation)
                            op = ((IConversionOperation)op).Operand;

                        if (op is IInstanceReferenceOperation)
                            receiver = "this";
                        else if (op is IParameterReferenceOperation)
                            receiver = ((IParameterReferenceOperation)op).Parameter.Name;
                        else if (op is ILocalReferenceOperation)
                            receiver = ((ILocalReferenceOperation)op).Local.Name;
                        else if (op is IFieldReferenceOperation)
                        {
                            if (!((IFieldReferenceOperation)op).Field.IsStatic)
                            {
                                if (((IFieldReferenceOperation)op).Field.Type.TypeKind == TypeKind.Delegate)
                                    receiver = "this";
                                else
                                    receiver = ((IFieldReferenceOperation)op).Field.Name;
                            }
                        }
                        else if (op is IPropertyReferenceOperation)
                            receiver = ((IPropertyReferenceOperation)op).Property.Name;
                        else if (op is IEventReferenceOperation)
                        {
                            // TODO: XXX: queda ver porque en algún momento puse "this"
                            //if (((IEventReferenceOperation)op).Event.IsStatic)
                            receiver = ((IEventReferenceOperation)op).Event.Name;
                            //else
                            //    receiver = "this" + ((IEventReferenceOperation)op).Event.Name;
                        }
                        else if (op is IConditionalAccessInstanceOperation)
                            receiver = TraceSyntaxGenerator.ConditionalAccessCheckNull((ConditionalAccessExpressionSyntax)op.Syntax.Parent.RemoveParentesisAndMore(), Visit(op.Syntax), sourceFileId).ToString();
                        else if (op == null && operation is IInvocationOperation && ((IInvocationOperation)operation).TargetMethod.MethodKind == MethodKind.LocalFunction)
                            receiver = "";
                        else
                            receiver = modifiedInvocation.GetText().ToString();

                        ref_str = op != null && op.Type != null && op.Type.CustomIsStruct() && ref_parameter ? "ref " : "";
                    }
                    #endregion

                    #region Add type parameters
                    var container = node.GetContainer();
                    var typeParameters = "";
                    if (container is MethodDeclarationSyntax &&
                        ((MethodDeclarationSyntax)container).TypeParameterList != null &&
                        methodDecl.Identifier.ValueText.Contains("<"))
                    {
                        var types = new List<string>();
                        var methodDeclTypeParams = methodDecl.Identifier.ValueText.Substring(
                            methodDecl.Identifier.ValueText.IndexOf("<")+1,
                            methodDecl.Identifier.ValueText.IndexOf(">")- methodDecl.Identifier.ValueText.IndexOf("<")-1).Split(",").Select(x => x.Trim());
                        foreach (var type in methodDeclTypeParams)
                        {
                            var allTypes = ((MethodDeclarationSyntax)container).GetTypeParameters();
                            if (allTypes.Contains(type))
                                types.Add(type);
                        }

                        if (types.Count > 0)
                        {
                            typeParameters = "<";
                            foreach (var type in types)
                                typeParameters += type + ",";
                            typeParameters = typeParameters.Substring(0, typeParameters.Length - 1);
                            typeParameters += ">";
                        }
                    }
                    #endregion

                    return SyntaxFactory.ParseExpression(string.Format("{0}{4}({3}{1}{2})", methodDecl.CustomGetName(), receiver, (arg_list.Length > 0 && receiver != "" ? ", " : "") + arg_list, ref_str, typeParameters));
                }
                #endregion
            }
            catch(Exception ex)
            {
                //BugLogging.Log(sourceFileId, node, BugLogging.Behavior.WithoutSymbol);
                return node;
            }
        }

        public override SyntaxNode VisitMemberAccessExpression(MemberAccessExpressionSyntax node)
        {
            // Visitamos la expresión base
            var visitedExpression = base.Visit(node.Expression);
            // Reconstuimos el member access, quedaría: (INSTRUMENTADO).Member
            var modifiedMemberAccess = SyntaxFactory.MemberAccessExpression(node.Kind(), (ExpressionSyntax)visitedExpression, node.OperatorToken, node.Name);

            if (NoWrapping(node))
                return modifiedMemberAccess;
            
            // Ahora bien:
            // Si la expresión visitada es un parámetro/variable local/this o base, o field access, entonces tenemos que enviarlo por referencia 
            // Cualquier de estos casos no se wrappean. Es decir, no wrappeamos ni parámetros ni fields, entonces podemos preguntar directamente por los mismos

            var visitedKind = visitedExpression.Kind();
            var ref_parameter = false;
            switch(visitedKind)
            {
                // Por ahora lo wrappeamos como antes
                case SyntaxKind.BaseExpression:
                    return TraceSyntaxGenerator.MemberAccessWrapperExpression(modifiedMemberAccess, Model, node, sourceFileId);
                case SyntaxKind.IdentifierName:
                    // Acá tenemos que ver si es local variable o parámeter
                    var exp_op_iden = Model.GetOperation(node.Expression);
                    if (exp_op_iden != null)
                        switch (exp_op_iden.Kind)
                        {
                            case OperationKind.LocalReference:
                                var syntax_ref = ((ILocalReferenceOperation)exp_op_iden).Local.DeclaringSyntaxReferences.Last();
                                var is_foreach =
                                    syntax_ref.SyntaxTree.GetRoot()
                                    .DescendantNodes().Where(x => x.Span.Start == syntax_ref.Span.Start 
                                    && x.Span.End == syntax_ref.Span.End).FirstOrDefault();
                                ref_parameter = !(is_foreach is ForEachStatementSyntax);
                                break;
                            case OperationKind.ParameterReference:
                                ref_parameter = true;
                                break;
                            default:
                                break;
                        }
                    else
                        ;
                    break;
                case SyntaxKind.SimpleMemberAccessExpression:
                    var exp_op_mem = Model.GetOperation(node.Expression);
                    if (exp_op_mem != null)
                    {
                        if (exp_op_mem.Kind == OperationKind.FieldReference)
                            ref_parameter = true;
                    }
                    else
                        ;
                    break;
                case SyntaxKind.ThisExpression:
                    ref_parameter = true;
                    break;
                case SyntaxKind.SuppressNullableWarningExpression:
                    var exp_op_sup = Model.GetOperation(((PostfixUnaryExpressionSyntax)node.Expression).Operand);
                    if (exp_op_sup != null)
                    {
                        if (exp_op_sup.Kind == OperationKind.FieldReference)
                            ref_parameter = true;
                    }
                    else
                        ;
                    break;
                default:
                    break;
            }

            var newMethods = GetWrappedMethodsDictionary();
            var spanHash = Utils.GetHashCodeFromFileAndSpan(sourceFileId, node.Span);
            MethodDeclarationSyntax methodDecl = null;
            if (newMethods.ContainsKey(spanHash))
                methodDecl = newMethods[spanHash];

            var args = "";
            if (methodDecl == null)
            {
                methodDecl = TraceSyntaxGenerator.GenericInvocationWrapped(modifiedMemberAccess, Model, node, sourceFileId, ref_parameter);
                if (methodDecl == null)
                    return modifiedMemberAccess;
                newMethods.Add(spanHash, methodDecl);
            }
            var expression = node.Expression.Kind() == SyntaxKind.SuppressNullableWarningExpression ? ((PostfixUnaryExpressionSyntax)node.Expression).Operand : node.Expression;
            if (!(expression is GenericNameSyntax || (!(expression is ParenthesizedExpressionSyntax) && Model.GetOperation(expression) == null)))
                args = visitedExpression.ToString();

            return SyntaxFactory.ParseExpression(string.Format("{0}({2}{1})", methodDecl.Identifier.ValueText, args,
                args != "" && ref_parameter && Model.GetOperation(node.Expression.RemoveParentesisAndMore()).Type.CustomIsStruct() ? "ref " : ""));
        }
        
        public override SyntaxNode VisitElementAccessExpression(ElementAccessExpressionSyntax node)
        {
            var operation = Model.GetOperation(node);
            if (NoWrapping(node) || operation.Kind != OperationKind.PropertyReference)
                return base.VisitElementAccessExpression(node);
            else
            {
                //return modifiedNode;
                // Si se entra acá es que es property reference y hay que wrappearlo
                // ANTERIOR
                //return TraceSyntaxGenerator.MemberAccessWrapperExpression(modifiedNode, Model, node, sourceFileId);

                var visitedExpression = (ExpressionSyntax)base.Visit(node.Expression);
                var visitedArgList = (BracketedArgumentListSyntax)base.Visit(node.ArgumentList);
                var newNode = SyntaxFactory.ElementAccessExpression(visitedExpression, visitedArgList);

                var newMethods = GetWrappedMethodsDictionary();
                var spanHash = Utils.GetHashCodeFromFileAndSpan(sourceFileId, node.Span);
                MethodDeclarationSyntax methodDecl = null;
                if (newMethods.ContainsKey(spanHash))
                    methodDecl = newMethods[spanHash];

                var args = "";
                if (methodDecl == null)
                {
                    methodDecl = TraceSyntaxGenerator.GenericInvocationWrapped(newNode, Model, node, sourceFileId, false);
                    if (methodDecl == null)
                        return newNode;
                    newMethods.Add(spanHash, methodDecl);
                }
                if (!(node.Expression is GenericNameSyntax || (!(node.Expression is ParenthesizedExpressionSyntax) && Model.GetOperation(node.Expression) == null)))
                    args = visitedExpression.ToString();

                // CASO PARTICULAR DE LOS ARGUMENTOS DE LA EXPRESIÓN:
                foreach (var accessArg in visitedArgList.Arguments)
                    args = args + ", " + accessArg.ToString();

                return SyntaxFactory.ParseExpression(string.Format("{0}({1})", methodDecl.Identifier.ValueText, args));
            }
        }

        public override SyntaxNode VisitIdentifierName(IdentifierNameSyntax node)
        {
            var modifiedIdentifierName = base.VisitIdentifierName(node);
            if (Wrapp(node))
            {
                var newMethods = GetWrappedMethodsDictionary();
                var spanHash = Utils.GetHashCodeFromFileAndSpan(sourceFileId, node.Span);
                MethodDeclarationSyntax methodDecl = null;
                if (newMethods.ContainsKey(spanHash))
                    methodDecl = newMethods[spanHash];

                if (methodDecl == null)
                {
                    methodDecl = TraceSyntaxGenerator.GenericInvocationWrapped(modifiedIdentifierName, Model, node, sourceFileId);
                    if (methodDecl == null)
                        return modifiedIdentifierName;

                    newMethods.Add(spanHash, methodDecl);
                }

                //return SyntaxFactory.ParseExpression(string.Format("{1}{0}()", methodDecl.Identifier.ValueText,
                //    Model.GetOperation(node).Type.CustomIsStruct() ? "ref " : ""));
                return SyntaxFactory.ParseExpression(string.Format("{0}()", methodDecl.Identifier.ValueText));
            }
            return modifiedIdentifierName;
        }

        public override SyntaxNode VisitPostfixUnaryExpression(PostfixUnaryExpressionSyntax node)
        {
            // Reescribimos el i++ como: () => i++ para luego de eso enviar un get.
            // TODO: ES UNA IDEA, HAY QUE REVISARLA, primero saltaría el GET y luego si hay otro callback sería por el SET
            // Otra sería la reescritura i = i + 1 pero también hay que revisar

            var rec = base.VisitPostfixUnaryExpression(node);
            if (Model.GetSymbolInfo(node.Operand).Symbol != null &&
                Model.GetSymbolInfo(node.Operand).Symbol.Kind == SymbolKind.Property &&
                node.OperatorToken.ValueText != "!")
            {
                var newMethods = GetWrappedMethodsDictionary();
                var spanHash = Utils.GetHashCodeFromFileAndSpan(sourceFileId, node.Span);
                MethodDeclarationSyntax methodDecl = null;
                if (newMethods.ContainsKey(spanHash))
                    methodDecl = newMethods[spanHash];
                if (methodDecl == null)
                {
                    methodDecl = TraceSyntaxGenerator.ExpressionWrapped(node, Model, sourceFileId, TraceSyntaxGenerator.AccessType.Member);
                    newMethods.Add(spanHash, methodDecl);
                }
                return SyntaxFactory.ParseExpression(string.Format("{0}({1})", methodDecl.Identifier.ValueText, rec.ToString()));
            }

                
            return rec;
        }

        public override SyntaxNode VisitPrefixUnaryExpression(PrefixUnaryExpressionSyntax node)
        {
            // Equal than postfix
            var rec = base.VisitPrefixUnaryExpression(node);
            if (Model.GetSymbolInfo(node.Operand).Symbol != null &&
                Model.GetSymbolInfo(node.Operand).Symbol.Kind == SymbolKind.Property)
            {
                var newMethods = GetWrappedMethodsDictionary();
                var spanHash = Utils.GetHashCodeFromFileAndSpan(sourceFileId, node.Span);
                MethodDeclarationSyntax methodDecl = null;
                if (newMethods.ContainsKey(spanHash))
                    methodDecl = newMethods[spanHash];
                if (methodDecl == null)
                {
                    methodDecl = TraceSyntaxGenerator.ExpressionWrapped(node, Model, sourceFileId, TraceSyntaxGenerator.AccessType.Member);
                    newMethods.Add(spanHash, methodDecl);
                }
                return SyntaxFactory.ParseExpression(string.Format("{0}({1})", methodDecl.Identifier.ValueText, rec.ToString()));
            }
            return rec;
        }

        public override SyntaxTrivia VisitTrivia(SyntaxTrivia trivia)
        {
            if (trivia.Kind() == SyntaxKind.RegionDirectiveTrivia || 
                trivia.Kind() == SyntaxKind.EndRegionDirectiveTrivia ||
                trivia.Kind() == SyntaxKind.IfDirectiveTrivia ||
                trivia.Kind() == SyntaxKind.ElseDirectiveTrivia ||
                trivia.Kind() == SyntaxKind.DisabledTextTrivia ||
                trivia.Kind() == SyntaxKind.EndIfDirectiveTrivia)
            {
                return new SyntaxTrivia();
            }
            return base.VisitTrivia(trivia);
        }

        public override SyntaxNode VisitParenthesizedLambdaExpression(ParenthesizedLambdaExpressionSyntax node)
        {
            // No visita lo de adentro
            return node;
        }
        #endregion

        #region Internal
        BlockSyntax InstrumentAndConvertToBlock(StatementSyntax originalNode, StatementSyntax modifiedNode)
        {
            BlockSyntax newBlock = SyntaxFactory.Block();
            var traceInstr = TraceSyntaxGenerator.SimpleStatement(originalNode, sourceFileId);
            newBlock = newBlock.AddStatements(traceInstr);
            newBlock = newBlock.AddStatements(modifiedNode);
            return newBlock;
        }

        BlockSyntax InstrumentScope(BlockSyntax blockInstrumented, StatementSyntax controllingExpressionOriginal, bool isForStmt = false)
        {
            blockInstrumented = blockInstrumented.WithStatements(blockInstrumented.Statements.Insert(0, TraceSyntaxGenerator.EnterConditionStatement(controllingExpressionOriginal, sourceFileId)));
            if (!isForStmt)
                blockInstrumented = blockInstrumented.WithStatements(blockInstrumented.Statements.Add(TraceSyntaxGenerator.ExitConditionStatement(controllingExpressionOriginal, sourceFileId)));
            return blockInstrumented
                .WithCloseBraceToken(SyntaxFactory.Token(SyntaxKind.CloseBraceToken).AddPostLine())
                .WithOpenBraceToken(SyntaxFactory.Token(SyntaxKind.OpenBraceToken))
                .WithoutTrivia().AddPreAndPostLine();
        }

        BlockSyntax WrapLoop(BlockSyntax instrumentedBlock, SyntaxNode originalStatement)
        {
            var exitLoopByException = TraceSyntaxGenerator.ExitLoopByExceptionStatement(originalStatement, sourceFileId);
            var tryStatement = (TryStatementSyntax)SyntaxFactory.ParseStatement(string.Format(@"try {{ }} catch(System.Exception) {{ {0} throw; }}", 
                exitLoopByException.GetText().ToString()));

            tryStatement = tryStatement.WithBlock(instrumentedBlock);
            tryStatement = tryStatement.WithFinally(
                SyntaxFactory.FinallyClause(
                        SyntaxFactory.Block(
                            TraceSyntaxGenerator.ExitLoopStatement(originalStatement, sourceFileId))));

            return SyntaxFactory.Block(tryStatement);
        }

        BlockSyntax WrapBlock(BlockSyntax instrumentedBlock, SyntaxNode node, bool returnSomething)
        {
            var stmtList = new SyntaxList<StatementSyntax>();
            stmtList = stmtList.Add(TraceSyntaxGenerator.EnterFinalCatchStatement(node, sourceFileId));
            stmtList = stmtList.Add(SyntaxFactory.ThrowStatement(null).AddPostLine());
            var globalCatchBlock = SyntaxFactory.Block().WithStatements(stmtList).AddPreAndPostLine();
            var globalCatchClause = SyntaxFactory.CatchClause().WithBlock(globalCatchBlock);
            var catches = new SyntaxList<CatchClauseSyntax>();
            catches = catches.Add(globalCatchClause);
            
            var globalTryStmt = SyntaxFactory.TryStatement(
                SyntaxFactory.Token(SyntaxKind.TryKeyword).WithTrailingTrivia(SyntaxFactory.Whitespace("\n")),
                instrumentedBlock, 
                catches, 
                SyntaxFactory.FinallyClause(
                SyntaxFactory.Block(
                    TraceSyntaxGenerator.EnterFinalFinallyStatement(node, sourceFileId)).AddPreLine())).AddPreAndPostLine();

            if (returnSomething)
            {
                // I add this exception because sometimes, the C# parser can't detect that a method which is supposed to returns something always reachs a return statement.
                var exceptionStmt = SyntaxFactory.ParseStatement("throw new System.Exception(\"Slicer error: unreachable code\");").WithTrailingTrivia(SyntaxFactory.Whitespace("\n"));
                return SyntaxFactory.Block(globalTryStmt, exceptionStmt).AddPostLine().AddTabs();
            }
            else
                return SyntaxFactory.Block(globalTryStmt).AddPostLine().AddTabs();
        }

        bool InsideFirstOfPartials(SyntaxNode originalNode, INamedTypeSymbol symbol)
        {
            /* Este metodo retorna true si, en el caso de que la clase sea partial, estemos
             * trabajando sobre la primera en orden de aparicion en "Locations" del classSymbol. */
            bool insideFirstOfPartials = true;
            if (alreadyVisitedClasses.Contains(symbol.ContainingNamespace + "." + symbol.Name)) insideFirstOfPartials = false;
            if (symbol.Locations.Count() > 1)
            {
                List<string> paths = symbol.Locations
                    .Select(x => x.SourceTree)
                    .OrderBy(s => s.FilePath)
                    .Select(p => p.FilePath)
                    .Where(p => analizingFile(p))
                    .ToList();
                if (paths.Any() && paths[0] != originalNode.SyntaxTree.FilePath) insideFirstOfPartials = false;
            }
            return insideFirstOfPartials;
        }

        static bool HasInstanceConstructor(IEnumerable<IMethodSymbol> methods)
        {
            bool instanceConstructorMissing = true;
            foreach (var method in methods)
            {
                if (method.MethodKind == MethodKind.Constructor)
                {
                    if (method.Name.Contains(".ctor"))
                    {
                        if (method.DeclaringSyntaxReferences.Count() != 0)
                        {
                            instanceConstructorMissing = false;
                            break;
                        }
                    }
                    else
                    {
                        instanceConstructorMissing = false;
                        break;
                    }
                }
            }
            return instanceConstructorMissing;
        }

        static bool HasStaticConstructor(IEnumerable<IMethodSymbol> methods)
        {
            bool staticConstructorMissing = true;
            foreach (var method in methods)
            {
                if (method.MethodKind == MethodKind.StaticConstructor)
                {
                    if (method.Name.Contains(".cctor"))
                    {
                        if (method.DeclaringSyntaxReferences.Count() != 0)
                        {
                            staticConstructorMissing = false;
                            break;
                        }
                    }
                    else
                    {
                        staticConstructorMissing = false;
                        break;
                    }
                }
            }
            return staticConstructorMissing;
        }

        /// <summary>
        /// Reemplazamos los nombres de los REF involucrados y los returns con las asignaciones previas a la salida
        /// </summary>
        BlockSyntax ReplaceRefOrOutParameters(BlockSyntax original, BlockSyntax modifiedBlock, IList<StatementSyntax> afterStatements)
        {
            // Reestructuramos los RETURN para que antes reasignen los valores correspondientes
            if (afterStatements.Count > 0)
                modifiedBlock = ReplaceReturnStatements(original, modifiedBlock, afterStatements);
            return modifiedBlock;
        }

        BlockSyntax ReplaceReturnStatements(BlockSyntax original, BlockSyntax modifiedBlock, IList<StatementSyntax> afterStatements)
        {
            try
            {
                if (afterStatements.Count == 0)
                    return modifiedBlock;

                throw new SlicerException("DynAbs: Unexpected behavior on the instrumentation");
                // No se debería seguir más por acá
                ;

                var tempStatements = new SyntaxList<StatementSyntax>();
                foreach (var stmt in modifiedBlock.Statements)
                {
                    if (stmt is ReturnStatementSyntax)
                    {
                        if (((ReturnStatementSyntax)stmt).Expression != null)
                        { 
                            string tmpVar = "_return_tmp_" + ++currentMethodInfo.tmpVariableNumber;
                            var typeVar = "var";
                            
                            var node = (SyntaxNode)original;
                            while(!(node.Parent is ClassDeclarationSyntax || node.Parent is StructDeclarationSyntax))
                                node = node.Parent;
                            // Acá "node.Parent" ya es Class o Struct, entonces Node será el método o acceso que sea...
                            if (node is ConstructorDeclarationSyntax)
                                throw new NotImplementedException();
                            else if (node is MethodDeclarationSyntax)
                                typeVar = Utils.GetTypeName((ITypeSymbol)(Model.GetSymbolInfo(((MethodDeclarationSyntax)node).ReturnType).Symbol));
                            else if (node is PropertyDeclarationSyntax)
                                typeVar = ((ITypeSymbol)Model.GetSymbolInfo(((PropertyDeclarationSyntax)node).Type).Symbol).Name;
                            else
                                throw new NotImplementedException();
                            // TODO: Falta con el IndexerDeclarationSyntax

                            tempStatements = tempStatements.Add(SyntaxFactory.ParseStatement(string.Format("{2} {0} = {1};", 
                                tmpVar, 
                                ((ReturnStatementSyntax)stmt).Expression.ToString(), 
                                typeVar))
                                .WithTrailingTrivia(SyntaxFactory.Whitespace("\n")));
                            tempStatements = tempStatements.AddRange(afterStatements);
                            tempStatements = tempStatements.Add(SyntaxFactory.ParseStatement(string.Format("return {0};", tmpVar)).WithTrailingTrivia(SyntaxFactory.Whitespace("\n")));
                        }
                        else
                        {
                            tempStatements = tempStatements.AddRange(afterStatements);
                            tempStatements = tempStatements.Add(SyntaxFactory.ParseStatement("return;")
                                .WithTrailingTrivia(SyntaxFactory.Whitespace("\n")));
                        }
                    }
                    else if (stmt is IfStatementSyntax)
                    {
                        var stmtToAdd = stmt;
                        if (((IfStatementSyntax)stmtToAdd).Statement != null && ((IfStatementSyntax)stmtToAdd).Statement is BlockSyntax)
                            stmtToAdd = ((IfStatementSyntax)stmtToAdd).WithStatement(ReplaceReturnStatements(original, (BlockSyntax)(((IfStatementSyntax)stmtToAdd).Statement), afterStatements));
                        if (((IfStatementSyntax)stmtToAdd).Else != null && ((IfStatementSyntax)stmtToAdd).Else.Statement is BlockSyntax)
                            stmtToAdd = ((IfStatementSyntax)stmtToAdd).WithElse(((IfStatementSyntax)stmtToAdd).Else.WithStatement(ReplaceReturnStatements(original, (BlockSyntax)(((IfStatementSyntax)stmtToAdd).Else.Statement), afterStatements)));
                        tempStatements = tempStatements.Add(stmtToAdd);
                    }
                    else if (stmt is WhileStatementSyntax)
                    {
                        var stmtToAdd = stmt;
                        if (((WhileStatementSyntax)stmtToAdd).Statement != null && ((WhileStatementSyntax)stmtToAdd).Statement is BlockSyntax)
                            stmtToAdd = ((WhileStatementSyntax)stmtToAdd).WithStatement(ReplaceReturnStatements(original, (BlockSyntax)(((WhileStatementSyntax)stmtToAdd).Statement), afterStatements));
                        tempStatements = tempStatements.Add(stmtToAdd);
                    }
                    else if (stmt is TryStatementSyntax)
                    {
                        var stmtToAdd = stmt;
                        if (((TryStatementSyntax)stmtToAdd).Block != null && ((TryStatementSyntax)stmtToAdd).Block is BlockSyntax)
                            stmtToAdd = ((TryStatementSyntax)stmtToAdd).WithBlock(ReplaceReturnStatements(original, (BlockSyntax)(((TryStatementSyntax)stmtToAdd).Block), afterStatements));
                        tempStatements = tempStatements.Add(stmtToAdd);
                    }
                    else
                        tempStatements = tempStatements.Add(stmt);
                }
                modifiedBlock = modifiedBlock.WithStatements(tempStatements);
                return modifiedBlock;
            }
            catch(Exception ex)
            {
                throw;
            }
        }

        void CleanRefAndOutParameters()
        {
            //methodRefOrOutParams = new HashSet<Tuple<string, string>>();
            currentMethodInfo.outParamsInline = new List<StatementSyntax>();
            currentMethodInfo.outParams = new HashSet<string>();
            currentMethodInfo.tmpVariableNumber = 0;
            currentMethodInfo.returnYieldReturnStmt = null;
        }

        private bool NoWrapping(MemberAccessExpressionSyntax node)
        {
            // No se wrappea si:
            // 1) Es asignación de lado izquierdo (no podés hacer TraceEnd(a.b) = 1; (por ej))
            // 2) No es invocación: esas se ven por otro lado con un EndInvocation
            // 3) No es ni postfix ni prefix (no podés hacer (End(a.b))++)
            // 4) No es un atributo (de un método por ej.)
            // 5) No estás dentro de un switch
            // 6) No es REF/OUT
            // 7) No es struct (no se puede en esos casos)
            // 8) No es parte de un namespace/method/clase/field constante (tampoco tiene sentido pq no tira callback)
            try { 
                return (node.Parent is AssignmentExpressionSyntax && ((AssignmentExpressionSyntax)node.Parent).Left == node) ||
                       (node.Parent is InvocationExpressionSyntax) ||
                       (node.Parent is PostfixUnaryExpressionSyntax) ||
                       (node.Parent is PrefixUnaryExpressionSyntax) ||
                       (node.Parent is AttributeArgumentSyntax) ||
                       (node.Parent is CaseSwitchLabelSyntax) ||
                       (node.Parent is ArgumentSyntax && ((ArgumentSyntax)node.Parent).RefOrOutKeyword.Value != null) ||
                   
                       // Structs
                       (!Globals.wrap_structs_calls && ((Model.GetTypeInfo(node.Expression).Type.CustomIsStruct()) ||
                       (node.Parent is MemberAccessExpressionSyntax // Structs
                           && (Model.GetTypeInfo(((MemberAccessExpressionSyntax)node.Parent).Expression).Type).CustomIsStruct()))) ||

                        // A ver... tendría que sacar lo de enum, pero andá a testear todo de nuevo... yo le pongo que no tiene que ser property y fue
                        (Model.GetTypeInfo(node).Type != null && Model.GetTypeInfo(node).Type.TypeKind == TypeKind.Enum
                           && (Model.GetSymbolInfo(node.Expression).Symbol != null && Model.GetSymbolInfo(node.Expression).Symbol.Kind == SymbolKind.NamedType)
                           && (Model.GetSymbolInfo(node).Symbol == null || !(Model.GetSymbolInfo(node).Symbol is IPropertySymbol))) ||
                       (node.Expression.Kind() == SyntaxKind.AliasQualifiedName) ||
                       (Model.GetSymbolInfo(node).Symbol != null && 
                            (Model.GetSymbolInfo(node).Symbol.Kind == SymbolKind.Namespace ||
                   		    Model.GetSymbolInfo(node).Symbol.Kind == SymbolKind.Method ||
                   		    Model.GetSymbolInfo(node).Symbol.Kind == SymbolKind.NamedType ||
                   		    (Model.GetSymbolInfo(node).Symbol is IFieldSymbol && ((IFieldSymbol)Model.GetSymbolInfo(node).Symbol).IsConst)));
            }
            catch(Exception e)
            {
                return true;
                //throw;
            }
        }

        private bool Wrapp(IdentifierNameSyntax node)
        {
            try
            { 
                return
                // No es asignación (IZQ) 
                !(node.Parent is AssignmentExpressionSyntax && ((AssignmentExpressionSyntax)node.Parent).Left == node) &&
                // POST o PREFIX UNARIY
                !(node.Parent is PostfixUnaryExpressionSyntax) &&
                !(node.Parent is PrefixUnaryExpressionSyntax) &&
                // ATTRIBUTE: son las anotaciones de los métodos o clases
                !(node.Parent is AttributeArgumentSyntax) &&
                // No se wrappea el "CASE" del SWITCH
                !(node.Parent is CaseSwitchLabelSyntax) &&
                // No se wrappean NameColon { Identifier: Algo }
                !(node.Parent is NameColonSyntax) &&
                // DIEGO DICE Q ES CABEZA
                !(node.Ancestors().Any(x => x is AttributeSyntax)) &&
                // LOS REF/OUT NO SE WRAPPEAN
                !(node.Parent is ArgumentSyntax && ((ArgumentSyntax)node.Parent).RefOrOutKeyword.Value != null) && // REF OUT
                // No hay enum, no podría pasar que se encapsule algo por error en este caso

                // STRUCTS
                !(!Globals.wrap_structs_calls && Model.GetOperation(node) != null && Model.GetOperation(node).CustomStructReceiver()) &&

                // Structs only if the global is setted
                //!((node.Parent is MemberAccessExpressionSyntax // Structs
                //    && (Model.GetTypeInfo(((MemberAccessExpressionSyntax)node.Parent).Expression).Type).CustomIsStruct()) ||
                //    (Model.GetSymbolInfo(node).Symbol != null &&
                //    Model.GetSymbolInfo(node).Symbol.Kind == SymbolKind.Field &&
                //    ((INamedTypeSymbol)(Model.GetSymbolInfo(node).Symbol).ContainingSymbol).CustomIsStruct())) &&

                // Los FIELDS o PROPERTIES DE ANONYMOUS tampoco
                (Model.GetSymbolInfo(node).Symbol != null &&
                    ((Model.GetSymbolInfo(node).Symbol.Kind == SymbolKind.Field &&
                    !((IFieldSymbol)Model.GetSymbolInfo(node).Symbol).IsConst)
                    || (Model.GetSymbolInfo(node).Symbol.Kind == SymbolKind.Property &&
                    !(node.Parent.Parent is AnonymousObjectMemberDeclaratorSyntax && // AnonymousTypes
                        node.Parent is NameEqualsSyntax && ((NameEqualsSyntax)node.Parent).Name == node))));
            }
            catch(Exception e)
            {
                return false;
            }
        }

        private bool NoWrapping(ElementAccessExpressionSyntax node)
        {
            // Identico al otro, por no haber herencia entre ambos.
            return (node.Parent is AssignmentExpressionSyntax && ((AssignmentExpressionSyntax)node.Parent).Left == node) ||
                   (node.Parent is InvocationExpressionSyntax) ||
                   (node.Parent is PostfixUnaryExpressionSyntax) ||
                   (node.Parent is PrefixUnaryExpressionSyntax) ||
                   (node.Parent is AttributeArgumentSyntax) ||
                   (node.Parent is CaseSwitchLabelSyntax) ||
                   (node.Parent is ArgumentSyntax && ((ArgumentSyntax)node.Parent).RefOrOutKeyword.Value != null) ||

                    // Structs
                    (!Globals.wrap_structs_calls && ((Model.GetTypeInfo(node.Expression).Type.CustomIsStruct()) ||
                    (node.Parent is MemberAccessExpressionSyntax // Structs
                        && (Model.GetTypeInfo(((MemberAccessExpressionSyntax)node.Parent).Expression).Type).CustomIsStruct()))) ||

                   (Model.GetTypeInfo(node).Type != null && Model.GetTypeInfo(node).Type.TypeKind == TypeKind.Enum
                       && (Model.GetSymbolInfo(node.Expression).Symbol.Kind == SymbolKind.NamedType)) ||
                   (node.Expression.Kind() == SyntaxKind.AliasQualifiedName) ||
                   (Model.GetSymbolInfo(node).Symbol != null &&
                        (Model.GetSymbolInfo(node).Symbol.Kind == SymbolKind.Namespace ||
                   		Model.GetSymbolInfo(node).Symbol.Kind == SymbolKind.Method ||
                   		Model.GetSymbolInfo(node).Symbol.Kind == SymbolKind.NamedType ||
                   		(Model.GetSymbolInfo(node).Symbol is IFieldSymbol && ((IFieldSymbol)Model.GetSymbolInfo(node).Symbol).IsConst)));
        }

        bool IgnoreMethod(MethodDeclarationSyntax node)
        {
            var className = "";
            if (node.Parent is ClassDeclarationSyntax)
                className = ((ClassDeclarationSyntax)node.Parent).Identifier.ValueText;
            if (node.Parent is StructDeclarationSyntax)
                className = ((StructDeclarationSyntax)node.Parent).Identifier.ValueText;
            return excludedClassesMethods.ContainsKey(className) && excludedClassesMethods[className].Contains(node.Identifier.ValueText);
        }
        
        bool IsStatic(ConstructorDeclarationSyntax node)
        {
            return (node.Modifiers.Select(x => x.ValueText).Contains(SyntaxFactory.Token(SyntaxKind.StaticKeyword).ValueText));
        }

        Dictionary<int, MethodDeclarationSyntax> GetWrappedMethodsDictionary()
        {
            if (insideBaseCall)
            {
                if (wrapperMethods.Count < 2)
                    return wrapperMethods.Peek();
                return wrapperMethods.ToList()[1];
            }
            return wrapperMethods.Peek();
        }

        LocalFunctionStatementSyntax ConvertoToLocal(MethodDeclarationSyntax methodDeclarationSyntax)
        {
            if (methodDeclarationSyntax == null)
                ;
            var local = SyntaxFactory.LocalFunctionStatement(methodDeclarationSyntax.ReturnType, methodDeclarationSyntax.Identifier);
            local = local.WithBody(methodDeclarationSyntax.Body);
            local = local.WithModifiers(methodDeclarationSyntax.Modifiers);
            local = local.WithParameterList(methodDeclarationSyntax.ParameterList);
            local = local.WithTypeParameterList(methodDeclarationSyntax.TypeParameterList);
            local = local.WithConstraintClauses(methodDeclarationSyntax.ConstraintClauses);
            local = local.WithAttributeLists(methodDeclarationSyntax.AttributeLists);
            local = local.WithLeadingTrivia(methodDeclarationSyntax.GetLeadingTrivia());
            local = local.WithTrailingTrivia(methodDeclarationSyntax.GetTrailingTrivia());
            return local;
        }
        #endregion
    }
}