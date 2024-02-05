using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;
using QuikGraph;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DynAbs.Tracing;
using QuikGraph.Graphviz;
using System.Diagnostics;

namespace DynAbs
{
    public class Utils
    {
        // The first one is syntactic (instrumentation), the second one is semantic (trace analysis)
        public static SyntaxKind[] ShortcircuitsBinaries = new SyntaxKind[] {
            SyntaxKind.CoalesceExpression,
            SyntaxKind.LogicalOrExpression,
            SyntaxKind.LogicalAndExpression
        };
        public static BinaryOperatorKind[] IOperationShortcircuitsBinaries = new BinaryOperatorKind[] {
            BinaryOperatorKind.ConditionalOr,
            BinaryOperatorKind.ConditionalAnd
        };
        public static BinaryOperatorKind[] IOperationStringBinaries = new BinaryOperatorKind[] { 
            BinaryOperatorKind.Concatenate,
            BinaryOperatorKind.Equals,
            BinaryOperatorKind.NotEquals,
            BinaryOperatorKind.Like,
            BinaryOperatorKind.Add
        };

        public static string CleanupDotNodeLabel(string s)
        {
            return s.Replace("\"", "'");
        }
        
        public static IMethodSymbol GetMethodSymbolInfo(SemanticModel Model, CSharpSyntaxNode node)
        {
            var argsCount = node is InvocationExpressionSyntax ?
                ((InvocationExpressionSyntax)node).ArgumentList.Arguments.Count :
                ((ObjectCreationExpressionSyntax)node).ArgumentList.Arguments.Count;

            var symbol = Model.GetSymbolInfo(node);
            if (symbol.Symbol != null)
                return (IMethodSymbol)symbol.Symbol;

            if (symbol.CandidateSymbols.IsEmpty)
                return null;

            var mainCandidate = symbol.CandidateSymbols.FirstOrDefault(x => ((IMethodSymbol)x).Parameters.Length == argsCount);
            if (mainCandidate != null)
                return (IMethodSymbol)mainCandidate;

            var anotherCandidate = symbol.CandidateSymbols.FirstOrDefault(x => ((IMethodSymbol)x).Parameters.Last().IsParams);

            // This can be null
            return (IMethodSymbol)anotherCandidate;

            // I'd prefer to have something like... 
            //return (IMethodSymbol)symbol.CandidateSymbols.First();
        }

        public static ISymbol GetSymbolInfo(SemanticModel Model, CSharpSyntaxNode node)
        {
            var symbol = Model.GetSymbolInfo(node);
            if (symbol.Symbol != null)
                return symbol.Symbol;

            if (symbol.CandidateSymbols.IsEmpty)
                return null;

            var mainCandidate = symbol.CandidateSymbols.FirstOrDefault();
            return mainCandidate;
        }

        public static bool IsNullable(ITypeSymbol typeSymbol)
        {
            return typeSymbol.Name.Equals("Nullable");
        }

        public static bool IsNullableOrReference(ITypeSymbol typeSymbol, bool stringAsScalar = true, bool structAsScalar = false)
        {
            return typeSymbol.Name.Equals("Nullable") || typeSymbol.IsNotScalar(stringAsScalar, structAsScalar);
        }

        public static bool IsNullableOrReference(IFieldSymbol fieldSymbol, bool stringAsScalar = true, bool structAsScalar = false)
        {
            return IsNullableOrReference(fieldSymbol.Type, stringAsScalar, structAsScalar);
        }

        public static bool IsNullableOrReference(IPropertySymbol propertySymbol, bool stringAsScalar = true, bool structAsScalar = false)
        {
            return IsNullableOrReference(propertySymbol.Type, stringAsScalar, structAsScalar);
        }

        public static bool IsStaticTrace(TraceType traceType)
        {
            return traceType == TraceType.EnterStaticConstructor || traceType == TraceType.EnterStaticMethod;
        }

        public static bool IsMethod(TraceType traceType)
        {
            return traceType == TraceType.EnterMethod || traceType == TraceType.EnterStaticMethod;
        }

        public static bool IsEnterMethodOrConstructor(TraceType traceType)
        {
            return traceType == TraceType.EnterMethod || traceType == TraceType.EnterStaticMethod ||
                traceType == TraceType.EnterConstructor || traceType == TraceType.EnterStaticConstructor ||
                traceType == TraceType.BeforeConstructor || traceType == TraceType.BaseCall;
        }

        public static string GetDefaultValue(ITypeSymbol scalarType)
        {
            switch (scalarType.ToString().ToLower())
            {
                case "bool":
                    return "false";
                case "byte":
                    return "0";
                case "char":
                    return @"'\0'";
                case "decimal":
                    return "0.0M";
                case "double":
                    return "0.0D";
                //case "enum":
                //    return "The value produced by the expression (E)0, where E is the enum identifier.";
                case "float":
                    return "0.0F";
                case "int":
                    return "0";
                case "long":
                    return "0L";
                case "sbyte":
                    return "0";
                case "short":
                    return "0";
                //case "struct":
                //    return "The value produced by setting all value-type fields to their default values and all reference-type fields to null.";
                case "uint":
                    return "0";
                case "ulong":
                    return "0";
                case "ushort":
                    return "0";
                default:
                    {
                        if (scalarType.TypeKind == TypeKind.Enum)
                            return "(" + scalarType.Name + ")0";

                        throw new SlicerException(Exceptions.ErrorMessages.Types_NotFound);
                    }
                    
            }
        }

        public static bool HasDefaultValue(ITypeSymbol scalarType)
        {
            if (scalarType.Name == "Nullable")
                scalarType = ((INamedTypeSymbol)scalarType).TypeArguments.Single();

            switch (scalarType.ToString().Replace("?", "").ToLower())
            {
                case "bool":
                    return true;
                case "byte":
                    return true;
                case "char":
                    return true;
                case "decimal":
                    return true;
                case "double":
                    return true;
                //case "enum":
                //    return "The value produced by the expression (E)0, where E is the enum identifier.";
                case "float":
                    return true;
                case "int":
                    return true;
                case "long":
                    return true;
                case "sbyte":
                    return true;
                case "short":
                    return true;
                //case "struct":
                //    return "The value produced by setting all value-type fields to their default values and all reference-type fields to null.";
                case "uint":
                    return true;
                case "ulong":
                    return true;
                case "ushort":
                    return true;
                case "void":
                    return true;
                default:
                    {
                        if (scalarType.TypeKind == TypeKind.Enum)
                            return true;

                        return false;
                    }

            }
        }

        public static string GetNamespaceName(ISymbol symbol)
        {
            // Globals or Arrays don't have this (https://msdn.microsoft.com/en-us/library/bb383786.aspx)
            if (symbol.ContainingNamespace == null && symbol.Kind == SymbolKind.ArrayType)
                return "System";

            return symbol.ContainingNamespace != null ? symbol.ContainingNamespace.ToString() : "Global";
        }

        public static string GetClassName(ISymbol symbol)
        {
            if (symbol is INamedTypeSymbol)
                return symbol.Name;
            if (symbol.ContainingType == null && symbol.Kind == SymbolKind.ArrayType)
                return "Array";

            return symbol.ContainingType.Name;
        }

        public static string GetMethodName(ISymbol symbol, string methodName = null)
        {
            if (symbol is INamedTypeSymbol || symbol.Kind == SymbolKind.ArrayType)
                return methodName ?? "";
            // El replace es porque los constructores se llaman ".ctor"
            // TODO: Si hay un método que también se llama ctor se solapan...
            return symbol.MetadataName.Replace(".", "").Replace("[]", "");
        }

        public static string GetPropertyName(CSharpSyntaxNode syntaxNode, ISymbol currentSymbol)
        {
            if (syntaxNode is MethodDeclarationSyntax || (syntaxNode is ArrowExpressionClauseSyntax && syntaxNode.Parent is MethodDeclarationSyntax))
                return Utils.GetMethodName(currentSymbol);
            if (syntaxNode is AccessorDeclarationSyntax)
                return ((IMethodSymbol)currentSymbol).ContainingSymbol.Name + "." + ((IMethodSymbol)currentSymbol).MethodKind.ToString();
            if (syntaxNode is ArrowExpressionClauseSyntax)
            {
                if (!(currentSymbol is IPropertySymbol))
                    return ((IMethodSymbol)currentSymbol).ToString();
                return ((IPropertySymbol)currentSymbol).Name + ".get";
            }
            return Utils.GetClassName(currentSymbol);
        }

        public static string GetTypeName(ITypeSymbol symbol)
        {
            var name = symbol.Name;
            if (symbol is INamedTypeSymbol)
            {
                var l_symbol = symbol as INamedTypeSymbol;
                if (l_symbol.TypeArguments != null && l_symbol.TypeArguments.Length > 0)
                {
                    name = name + "<";
                    for (var i = 0; i < l_symbol.TypeArguments.Length; i++)
                    {
                        var internalTypeName = GetTypeName(l_symbol.TypeArguments[i]);
                        name = name + internalTypeName;
                        if (i != l_symbol.TypeArguments.Length - 1)
                            name = name + ", ";
                    }
                    name = name + ">";
                }
                if (l_symbol.IsTupleType && l_symbol.TupleElements.Count() > 0)
                {
                    name += "(";
                    foreach(var t in l_symbol.TupleElements)
                        name += GetTypeName(t.Type) + ",";
                    // For the last comma
                    name = name.Substring(0, name.Length - 1); 
                    name += ")";
                }
            }
            else if (symbol is IArrayTypeSymbol)
                name = GetTypeName(((IArrayTypeSymbol)symbol).ElementType) + "[]";
            return name;
        }

        public static IDictionary<ITypeSymbol, ITypeSymbol> GetTypesDictionary(INamedTypeSymbol typeSymbol, IDictionary<ITypeSymbol, ITypeSymbol> lastTypesDictionary)
        {
            if (typeSymbol == null)
                return null;

            var cantTypeParameters = typeSymbol.TypeParameters.Count();
            if (cantTypeParameters == 0)
                return null;

            var listParameters = typeSymbol.TypeParameters.ToList();
            var listArguments = typeSymbol.TypeArguments.ToList();

            var returnValue = new Dictionary<ITypeSymbol, ITypeSymbol>();
            for (var i = 0; i < cantTypeParameters; i++)
                if (listArguments[i].TypeKind == TypeKind.TypeParameter)
                {
                    if (lastTypesDictionary == null)
                        return null;
                    var tempType = lastTypesDictionary.Where(x => x.Key.Name == listArguments[i].Name).FirstOrDefault().Value;
                    returnValue.Add(listParameters[i], tempType ?? listArguments[i]);
                }
                else
                    returnValue.Add(listParameters[i], listArguments[i]);
            return returnValue;
        }

        public static IDictionary<ITypeSymbol, ITypeSymbol> GetTypesDictionary(IMethodSymbol methodSymbol, IDictionary<ITypeSymbol, ITypeSymbol> lastTypesDictionary)
        {
            // Here we have to check the class and method type parameters, Class<T>.Method() o Class.Method<T>(). 
            if (methodSymbol == null)
                return null;

            try 
            { 
                var containingTypeDictionary = GetTypesDictionary(methodSymbol.ContainingType, lastTypesDictionary);

                var cantTypeParameters = methodSymbol.TypeParameters.Count();
                if (cantTypeParameters == 0)
                    return containingTypeDictionary;

                var listParameters = methodSymbol.TypeParameters.ToList();
                var listArguments = methodSymbol.TypeArguments.ToList();

                var returnValue = new Dictionary<ITypeSymbol, ITypeSymbol>();
                for (var i = 0; i < cantTypeParameters; i++)
                    if (listArguments[i].TypeKind == TypeKind.TypeParameter)
                        returnValue.Add(listParameters[i], lastTypesDictionary.Where(x => x.Key.Name == listArguments[i].Name).First().Value);
                    else
                        returnValue.Add(listParameters[i], listArguments[i]);

                if (containingTypeDictionary != null)
                    foreach (var keyValue in containingTypeDictionary)
                            returnValue.Add(keyValue.Key, keyValue.Value);

                return returnValue;
            }
            catch(ArgumentNullException)
            {
                // These kind of exceptions are thrown because the dictionary is not complete.
                return null;
            }
            catch(Exception)
            {
                throw;
            }
        }

        public static string GetRealName(string name, IDictionary<ITypeSymbol, ITypeSymbol> typesDictionary)
        {
            if (typesDictionary == null)
                return name;

            var returnName = name;
            if (typesDictionary != null)
                foreach (var keyValue in typesDictionary)
                {
                    var keyString = string.Format("<{0}>", keyValue.Key.Name);
                    var valueString = string.Format("<{0}>", keyValue.Value.Name);
                    returnName = returnName.Replace(keyString, valueString);
                }

            return returnName;
        }

        public static Stmt StmtFromSyntaxNode(CSharpSyntaxNode node, InstrumentationResult instrumentationResult)
        {
            int lineInSourceCode = node.GetLocation().GetLineSpan().StartLinePosition.Line + 1;
            var id = instrumentationResult.FileToIdDictionary[node.SyntaxTree.FilePath];
            return new Stmt
            {
                CSharpSyntaxNode = node,
                Line = lineInSourceCode,
                SpanStart = node.Span.Start,
                SpanEnd = node.Span.End,
                FileId = id
            };
        }

        public static ISlicerSymbol GetThisBySyntaxNode(CSharpSyntaxNode syntaxNode, SemanticModel semanticModel)
        {
            if (syntaxNode is ConstructorDeclarationSyntax)
                return ISlicerSymbol.Create(semanticModel.GetDeclaredSymbol((ConstructorDeclarationSyntax)syntaxNode).ReceiverType);
            if (syntaxNode is MethodDeclarationSyntax)
                return ISlicerSymbol.Create(semanticModel.GetDeclaredSymbol((MethodDeclarationSyntax)syntaxNode).ReceiverType);
            if (syntaxNode is LocalFunctionStatementSyntax)
                return ISlicerSymbol.Create(((IMethodSymbol)semanticModel.GetDeclaredSymbol((LocalFunctionStatementSyntax)syntaxNode)).ReceiverType);
            if (syntaxNode is AccessorDeclarationSyntax)
                return ISlicerSymbol.Create(semanticModel.GetDeclaredSymbol((AccessorDeclarationSyntax)syntaxNode).ReceiverType);
            if (syntaxNode is ClassDeclarationSyntax)
                return ISlicerSymbol.Create(semanticModel.GetDeclaredSymbol((ClassDeclarationSyntax)syntaxNode));
            if (syntaxNode is ArrowExpressionClauseSyntax)
                return ISlicerSymbol.Create(semanticModel.GetDeclaredSymbol(syntaxNode.Parent).ContainingType);
            throw new SlicerException("Tipo de nodo no esperado");
        }

        public static void SaveSliceResult(List<ResultSummaryData> slicedStatements, string path)
        {
            var sb = new StringBuilder();
            foreach(var slice in slicedStatements)
            {
                var statementsSet = new List<string>(slice.SlicedStatements.OrderBy(x => 
                    System.IO.Path.GetFileName(x.FileName)).ThenBy(x => x.Line).Select(s => System.IO.Path.GetFileName(s.FileName) + ":" + s.Line).Distinct());
                foreach (var s in statementsSet)
                    sb.AppendLine(s);
                sb.AppendLine($"---- Last slice: [{slice.FilePath} - {slice.Line}]");
            }
            System.IO.File.WriteAllText(path, sb.ToString());
        }

        public static void SaveFilteredSliceResult(List<ResultSummaryData> slicedStatements, ExecutedStatementsContainer ExecutedStmtsContainer, string path)
        {
            ISet<Stmt> executed = ExecutedStmtsContainer.ExecutedStatements;
            ISet<Stmt> propAndFieldsInit = ExecutedStmtsContainer.PropertiesOrFieldsInitialization;
            
            var sb = new StringBuilder();
            foreach (var slice in slicedStatements)
            {
                var fileLineComparer = new StmtFileAndLineEqualityComparer();
                var statementsSet = new List<string>(slice.SlicedStatements.Where(x => executed.Contains(x, fileLineComparer) /*&& !propAndFieldsInit.Contains(x, fileLineComparer)*/).OrderBy(x =>
                    System.IO.Path.GetFileName(x.FileName)).ThenBy(x => x.Line).Select(s => System.IO.Path.GetFileName(s.FileName) + ":" + s.Line).Distinct());
                foreach (var s in statementsSet)
                    sb.AppendLine(s);
                sb.AppendLine($"---- Last slice: [{slice.FilePath} - {slice.Line}]");
            }
            System.IO.File.WriteAllText(path, sb.ToString());
        }

        public static void SaveSliceDependenciesGraphResult(List<AdjacencyGraph<string, Edge<string>>> graphs, IDictionary<string, string> vertexLabels, string folder)
        {
            if (!System.IO.Directory.Exists(folder))
                System.IO.Directory.CreateDirectory(folder);

            var iterationNumber = 1;
            foreach(var graph in graphs)
            {
                var path = System.IO.Path.Combine(folder, "result" + iterationNumber++ + ".dgml");
                var gv = new GraphvizAlgorithm<string, Edge<string>>(graph);
                Func<string, string> func = x => x.Equals(DependencyGraphStmtDescriptor.VertexNameForExternal) ? x : vertexLabels[x].Split('\n')[0].Trim();
                File.WriteAllText(path, GraphUtils.GenerateDG_DGML(gv, func));
            }
        }

        public static void SaveSkippedFilesInfo(UserSliceConfiguration userSliceConfiguration, string path, string fileTraceInputPath, InstrumentationResult instrumentationResult, Solution solution)
        {
            if (string.IsNullOrWhiteSpace(path) || !DynAbs.Globals.skip_trace_enabled)
                return;

            var counterDictionary = new Dictionary<int, int>();
            var fileInfoDict = GetFilesInfo(userSliceConfiguration, instrumentationResult, solution);
            var txtLines = File.ReadAllLines(fileTraceInputPath);
            var newLines = new List<string>();
            foreach (var oldLine in txtLines)
            {
                var trimmed = oldLine.Split(',');
                if (trimmed.Length > 1)
                {
                    var fileId = int.Parse(trimmed[0]);
                    if (counterDictionary.ContainsKey(fileId))
                        counterDictionary[fileId]++;
                    else
                        counterDictionary[fileId] = 1;
                }
            }
            var total = counterDictionary.Sum(x => x.Value);

            var allFolders = fileInfoDict.Select(x => Path.GetDirectoryName(x.Value.Path)).Distinct();
            var dictFolder = new Dictionary<string, int>();
            foreach (var folder in allFolders)
            {
                var count = 0;
                foreach (var idCount in counterDictionary)
                {
                    if (fileInfoDict.ContainsKey(idCount.Key) && 
                        fileInfoDict[idCount.Key].Path.Contains(folder))
                    {
                        count += idCount.Value;
                    }
                }
                dictFolder[folder] = count;
            }

            var totalSkippedLines = userSliceConfiguration.FilesToSkip.Sum(x => counterDictionary.ContainsKey(x) ? counterDictionary[x] : 0);
            var totalExcludedExecuted = userSliceConfiguration.FilesToSkip.Count(x => counterDictionary.ContainsKey(x));
            var percentageSkippedLinesExcluded = Math.Round((double)totalSkippedLines * (double)100 / (double)total, 2);
            var percentageExecutedExcluded = Math.Round((double)totalExcludedExecuted * (double)100 / (double)counterDictionary.Count, 2);

            var sb = new StringBuilder();
            sb.AppendLine($"Total lines: {total}");
            sb.AppendLine($"Skipped lines: {totalSkippedLines}");
            sb.AppendLine($"Skipped lines (percentage): {percentageSkippedLinesExcluded}%");
            sb.AppendLine($"Total files with lines: {counterDictionary.Count}");
            sb.AppendLine($"Total executed files excluded: {totalExcludedExecuted}");
            sb.AppendLine($"Total executed files excluded (percentage): {percentageExecutedExcluded}");
            sb.AppendLine($"Excluded folders: ");
            if (userSliceConfiguration.FoldersToSkip != null)
                foreach (var excludedFolder in userSliceConfiguration.FoldersToSkip)
                {
                    var wasExecuted = dictFolder.ContainsKey(excludedFolder);
                    var l_skippedLines = wasExecuted ? dictFolder[excludedFolder] : 0;
                    var l_skippedLinesPercentage = Math.Round((double)l_skippedLines * (double)100 / (double)total, 2);
                    sb.AppendLine($"{excludedFolder} | {l_skippedLines} | {l_skippedLinesPercentage}%");
                }
            sb.AppendLine($"=========");
            File.WriteAllText(path, sb.ToString());
        }

        static Dictionary<int, LocalFileInfo> GetFilesInfo(UserSliceConfiguration userSliceConfiguration, InstrumentationResult instrumentationResult, Solution solution)
        {
            var retDict = new Dictionary<int, LocalFileInfo>();

            foreach (var proj in userSliceConfiguration.User.targetProjects?.excluded ?? new UserConfiguration.ExcludedProject[] { })
            {
                var currentProjectName = proj.name;
                if (proj.files != null)
                    foreach (var file in proj.files)
                    {
                        var fileInfo = new LocalFileInfo();
                        fileInfo.FileId = file.id;
                        fileInfo.ProjectName = currentProjectName;
                        fileInfo.Path = file.name;
                        if (fileInfo.FileId > 0)
                            retDict.Add(fileInfo.FileId, fileInfo);
                    }
                else
                {
                    var project = solution.Projects.Where(x => x.Name == currentProjectName).SingleOrDefault();
                    if (project == null)
                        continue;
                    var directoryName = System.IO.Path.GetDirectoryName(project.FilePath);
                    foreach (var fileInDirectory in Directory.GetFiles(directoryName, "*.cs", SearchOption.AllDirectories))
                        if (instrumentationResult.FileToIdDictionary.ContainsKey(fileInDirectory))
                            retDict.Add(instrumentationResult.FileToIdDictionary[fileInDirectory], new LocalFileInfo()
                            {
                                FileId = instrumentationResult.FileToIdDictionary[fileInDirectory],
                                ProjectName = currentProjectName,
                                Path = fileInDirectory
                            });
                }
            }

            return retDict;
        }

        public static bool InRange(int value, Tuple<int, int> range)
        {
            return value >= range.Item1 && value <= range.Item2;
        }

        class LocalFileInfo
        {
            public int FileId { get; set; }
            public string ProjectName { get; set; }
            public string Path { get; set; }
        }

        public static int GetHashCodeFromFileAndSpan(int FileId, Microsoft.CodeAnalysis.Text.TextSpan span)
        {
            int prime = 31;
            int result = 1;
            result = prime * result + FileId;
            result = prime * result + span.Start;
            result = prime * result + span.End;
            return result;
        }

        // AUX
        static int yesCounter = 0;
        static int noCounter = 0;
        public static void SaveConditionalAccessExpression(SyntaxNode sn, SyntaxNode instrumented, Dictionary<int, MethodDeclarationSyntax> newMethods)
        {
            var folder = @"C:\temp\CondAccess";
            var file = sn.SyntaxTree.FilePath;
            var allContainers = sn.GetAllContainers();
            var fullFileName = Guid.NewGuid() + ".txt";
            var fullPath = Path.Combine(folder, fullFileName);
            var sb = new StringBuilder();
            sb.AppendLine("File: " + file);
            sb.AppendLine("Internal location: " + string.Join(".", allContainers));
            var typeName = "";
            yesCounter++;
            sb.AppendLine("Type: " + typeName);
            sb.AppendLine("==============================");
            sb.AppendLine("");
            sb.AppendLine("");
            sb.Append(instrumented.ToString());
            sb.AppendLine("");
            sb.AppendLine("");
            sb.AppendLine("==============================");
            sb.AppendLine("");
            sb.AppendLine("");
            foreach (var newMethod in newMethods)
            {
                sb.AppendLine(newMethod.Value.ToString());
                sb.AppendLine("");
                sb.AppendLine("");
            }
            var fileContent = sb.ToString();
            File.WriteAllText(fullPath, fileContent);
        }

        public static void Print(string msg)
        {
            if (Globals.print_console)
                Console.WriteLine(msg);
        }

        public static int CountAsterix(string s)
        {
            var i = s.Length - 1;
            while (s[i] == '*')
                i--;
            return s.Length - 1 - i;
        }

        public static string RunDotnet(string workingDirectory, string arguments)
        {
            ProcessStartInfo procStartInfo = new ProcessStartInfo();
            procStartInfo.FileName = "dotnet";
            procStartInfo.Arguments = arguments;
            procStartInfo.WorkingDirectory = workingDirectory;
            procStartInfo.UseShellExecute = false;
            procStartInfo.CreateNoWindow = false;
            procStartInfo.RedirectStandardOutput = true;
            procStartInfo.RedirectStandardError = true;

            StringBuilder sb = new StringBuilder();
            Process pr = new Process();
            pr.StartInfo = procStartInfo;

            pr.OutputDataReceived += (s, ev) =>
            {
                if (string.IsNullOrWhiteSpace(ev.Data))
                    return;
                sb.AppendLine(ev.Data);
            };

            pr.ErrorDataReceived += (s, err) => { };

            pr.EnableRaisingEvents = true;
            pr.Start();
            pr.BeginOutputReadLine();
            pr.BeginErrorReadLine();

            pr.WaitForExit();

            return sb.ToString();
        }

        public static (bool Timeout, TimeSpan ElapsedTime) RunProject(string projectFilePath, string arguments = "", int? timeoutMilliseconds = null)
        {
            ProcessStartInfo procStartInfo = new ProcessStartInfo();
            procStartInfo.FileName = "dotnet";
            procStartInfo.Arguments = $"run -- {arguments}";
            procStartInfo.WorkingDirectory = Path.GetDirectoryName(projectFilePath);
            procStartInfo.UseShellExecute = false;
            procStartInfo.CreateNoWindow = false;
            procStartInfo.RedirectStandardOutput = true;
            procStartInfo.RedirectStandardError = true;

            if (procStartInfo.EnvironmentVariables.ContainsKey("MSBuildSDKsPath"))
            {
                procStartInfo.EnvironmentVariables.Remove("MSBuildSDKsPath");
                procStartInfo.EnvironmentVariables.Remove("MSBuildExtensionsPath");
                procStartInfo.EnvironmentVariables.Remove("MSBUILD_EXE_PATH");
            }

            StringBuilder sb = new StringBuilder();
            Process pr = new Process();
            pr.StartInfo = procStartInfo;

            pr.OutputDataReceived += (s, ev) =>
            {
                if (string.IsNullOrWhiteSpace(ev.Data)) { return; }
                sb.AppendLine(ev.Data);
            };

            pr.ErrorDataReceived += (s, err) => { };

            pr.EnableRaisingEvents = true;
            pr.Start();
            pr.BeginOutputReadLine();
            pr.BeginErrorReadLine();

            pr.WaitForExit();
            var timeout = false;
            if (timeoutMilliseconds.HasValue)
                if (!pr.WaitForExit(timeoutMilliseconds.Value))
                {
                    pr.Kill();
                    timeout = true;
                }
            
            return (timeout, pr.TotalProcessorTime);
        }

        public static Process GetRunProcess(string projectFilePath, string arguments = "")
        {
            ProcessStartInfo procStartInfo = new ProcessStartInfo();
            procStartInfo.FileName = "dotnet";
            procStartInfo.Arguments = $"run -- {arguments}";
            procStartInfo.WorkingDirectory = Path.GetDirectoryName(projectFilePath);
            procStartInfo.UseShellExecute = false;
            procStartInfo.CreateNoWindow = false;
            procStartInfo.RedirectStandardOutput = true;
            procStartInfo.RedirectStandardError = true;
            procStartInfo.Verb = "runas";

            if (procStartInfo.EnvironmentVariables.ContainsKey("MSBuildSDKsPath"))
            {
                procStartInfo.EnvironmentVariables.Remove("MSBuildSDKsPath");
                procStartInfo.EnvironmentVariables.Remove("MSBuildExtensionsPath");
                procStartInfo.EnvironmentVariables.Remove("MSBUILD_EXE_PATH");
            }

            StringBuilder sb = new StringBuilder();
            Process pr = new Process();
            pr.StartInfo = procStartInfo;

            pr.OutputDataReceived += (s, ev) =>
            {
                if (string.IsNullOrWhiteSpace(ev.Data)) { return; }
                sb.AppendLine(ev.Data);
            };

            pr.ErrorDataReceived += (s, err) => { };

            pr.EnableRaisingEvents = true;
            return pr;
        }

        public static (bool Timeout, TimeSpan ElapsedTime) RunCommand(string command, int? timeoutMilliseconds = null)
        {
            var timeout = false;
            var processInfo = new ProcessStartInfo("cmd.exe", "/c " + command)
            {
                CreateNoWindow = true,
                UseShellExecute = false,

                RedirectStandardOutput = true
            };

            Process p = Process.Start(processInfo);
            StringBuilder sb = new StringBuilder();
            while (!p.StandardOutput.EndOfStream)
            {
                sb.AppendLine(p.StandardOutput.ReadLine());
            }

            if (timeoutMilliseconds.HasValue)
                if (!p.WaitForExit(timeoutMilliseconds.Value))
                {
                    p.Kill();
                    timeout = true;
                }

            return (timeout, p.TotalProcessorTime);
        }
    }

    public static class BugLogging
    {
        static string fullPath = @"C:\temp\buglog.txt";

        public class LogInfo
        {
            public int FileId;
            public int SpanStart;
            public int SpanEnd;
            public string Name;
            public HashSet<Statement> Statements = new HashSet<Statement>();
        }
        // Kind = Invocation, ObjectCreation
        public class Statement
        {
            public int FileId;
            public int SpanStart;
            public int SpanEnd;
            public string Kind;
            public string Text;
            public Behavior Reason;

            public Statement (int fileId, CSharpSyntaxNode node, Behavior reason)
            {
                FileId = fileId;
                SpanStart = node.SpanStart;
                SpanEnd = node.Span.End;

                var KindName = "";
                switch(node.Kind())
                {
                    case SyntaxKind.ObjectCreationExpression:
                        KindName = "ObjectCreation";
                        break;
                    case SyntaxKind.InvocationExpression:
                        KindName = "Invocation";
                        break;
                    default:
                        KindName = node.Kind().ToString();
                        break;
                }

                Kind = KindName;
                Text = string.Join("", node.ToString().Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries));
                Reason = reason;
            }

            public override bool Equals(object obj)
            {
                var item = obj as Statement;
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
        }
        public static Dictionary<string, Dictionary<int, LogInfo>> LoggingInfo = new Dictionary<string, Dictionary<int, LogInfo>>();

        public enum Behavior 
        {
            WithIOperation,
            WithoutIOperation,
            WithoutSymbol
        }

        public static void Log(int fileId, CSharpSyntaxNode node, Behavior reason)
        {
            // string fullNameClass, CSharpSyntaxNode currentMethod, 

            var currentContainer = (CSharpSyntaxNode)node.GetContainer();
            CSharpSyntaxNode currentMethod = currentContainer is ClassDeclarationSyntax || currentContainer is StructDeclarationSyntax ? null : currentContainer;
            var currentClass = (CSharpSyntaxNode)currentContainer.GetContainerClass();
            var fullNameClass = currentClass.GetName();

            Dictionary<int, LogInfo> classInfo = null;
            if (!LoggingInfo.ContainsKey(fullNameClass))
            {
                classInfo = new Dictionary<int, LogInfo>();
                LoggingInfo.Add(fullNameClass, classInfo);
            }
            else
                classInfo = LoggingInfo[fullNameClass];

            // Method
            var key = 1;
            if (currentMethod != null)
            {
                int prime = 31;
                key = prime * key + fileId;
                key = prime * key + currentMethod.Span.Start;
                key = prime * key + currentMethod.Span.End;
            }

            LogInfo logInfo = null;
            if (!classInfo.ContainsKey(key))
            {
                logInfo = new LogInfo();
                classInfo.Add(key, logInfo);
            }
            else
                logInfo = classInfo[key];

            logInfo.FileId = fileId;
            if (currentMethod != null)
            {
                logInfo.SpanStart = currentMethod.Span.Start;
                logInfo.SpanEnd = currentMethod.Span.End;
                logInfo.Name = currentMethod.GetName();
            }
            else
            { 
                logInfo.SpanStart = 0;
                logInfo.SpanEnd = 0;
            }

            // Statement
            var s = new Statement(fileId, node, reason);

            logInfo.Statements.Add(s);
        }

        public static void Clean()
        {
            LoggingInfo = new Dictionary<string, Dictionary<int, LogInfo>>();
        }

        public static void Save()
        {
            var sb = new StringBuilder();
            foreach (var c in LoggingInfo)
            {
                foreach(var m in c.Value)
                { 
                    foreach(var s in m.Value.Statements)
                    {
                        sb.AppendLine(
                            c.Key + "%" +
                            m.Value.FileId + "%" +
                            m.Value.SpanStart + "%" +
                            m.Value.SpanEnd + "%" +
                            m.Value.Name + "%" +
                            s.Reason.ToString() + "%" +
                            s.Kind + "%" +
                            s.FileId + "%" +
                            s.SpanStart + "%" +
                            s.SpanEnd + "%" +
                            s.Text
                        );
                    }
                }
            }
            System.IO.File.AppendAllText(fullPath, sb.ToString());
        }
    }
}
