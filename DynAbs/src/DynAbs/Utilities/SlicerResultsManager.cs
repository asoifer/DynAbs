using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using QuikGraph;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DynAbs.Tracing;

namespace DynAbs
{
    class SlicerResultsManager
    {
        InstrumentationResult InstrumentationResult;
        public Dictionary<string, Tuple<int, int>> MaxSpans;
        public Dictionary<string, Stmt> VertexToStatement;
        public Dictionary<string, ISet<string>> LineStatements;

        public SlicerResultsManager(InstrumentationResult instrumentationResult)
        {
            InstrumentationResult = instrumentationResult;
        }
        
        public void SaveExecutedStatements(IEnumerable<Stmt> executedStatements, string writeToFile)
        {
            if (writeToFile == "")
                return;

            if (File.Exists(writeToFile))
                File.Delete(writeToFile);
            var file = File.CreateText(writeToFile);
            var distinctExecutedStatements = executedStatements.Select(x => new { FileId = x.FileId, SpanStart = x.SpanStart, SpanEnd = x.SpanEnd }).Distinct();
            foreach (var iValue in distinctExecutedStatements)
                file.WriteLine(string.Format("{0} {1} {2}", iValue.FileId, iValue.SpanStart, iValue.SpanEnd));
            file.Close();
        }

        public void SaveExecutedStatementsForUser(ExecutedStatementsContainer ExecutedStmtsContainer, string writeToFile)
        {
            var executedStatements = ExecutedStmtsContainer.ExecutedStatements;
            var executedMethods = ExecutedStmtsContainer.ExecutedMethods;
            var executedClasses = ExecutedStmtsContainer.ExecutedClasses;

            if (writeToFile == "")
                return;

            if (File.Exists(writeToFile))
                File.Delete(writeToFile);
            var file = File.CreateText(writeToFile);
            var stComparer = new StmtFileAndLineEqualityComparer();
            // We don't use just the file name because it can contains repeated names. 
            // Unfortunately, this can still contains repeated names (FileName:Line). (TODO)
            var distinctExecutedStatements = executedStatements.Union(executedMethods).Union(executedClasses)
                //.Where(x => !PropsAndFieldsInit.Contains(x, stComparer))
                .Select(x => new { FileId = x.FileId, Line = x.Line, FileName = x.FileName }).Distinct().OrderBy(x => x.FileName).ThenBy(x => x.Line);
            foreach (var iValue in distinctExecutedStatements)
                file.WriteLine(string.Format("{0}:{1}", iValue.FileName, iValue.Line));
            file.Close();
        }

        public void SaveExecutedStatementsWithoutHeadersForUser(ExecutedStatementsContainer ExecutedStmtsContainer, string writeToFile)
        {
            var executedStatements = ExecutedStmtsContainer.ExecutedStatements;

            if (writeToFile == "")
                return;

            if (File.Exists(writeToFile))
                File.Delete(writeToFile);
            var file = File.CreateText(writeToFile);
            var stComparer = new StmtFileAndLineEqualityComparer();
            // We don't use just the file name because it can contains repeated names. 
            // Unfortunately, this can still contains repeated names (FileName:Line). (TODO)
            var distinctExecutedStatements = executedStatements
                //.Where(x => !PropsAndFieldsInit.Contains(x, stComparer))
                .Select(x => new { FileId = x.FileId, Line = x.Line, FileName = x.FileName }).Distinct().OrderBy(x => x.FileName).ThenBy(x => x.Line);
            foreach (var iValue in distinctExecutedStatements)
                file.WriteLine(string.Format("{0}:{1}", iValue.FileName, iValue.Line));
            file.Close();
        }

        public void SaveExecutionCounters(ExecutedStatementsContainer ExecutedStmtsContainer, string writeToFile)
        {
            if (writeToFile == "")
                return;

            if (File.Exists(writeToFile))
                File.Delete(writeToFile);

            var sb = new StringBuilder();
            sb.Append(ExecutedStmtsContainer.ExecutedStatmentsCounter);
            sb.Append("|");
            sb.Append(ExecutedStmtsContainer.PropertiesOrFieldsInitializationCounter);
            sb.Append("|");
            sb.Append(ExecutedStmtsContainer.MethodsCounter);
            sb.Append("|");
            sb.Append(ExecutedStmtsContainer.ClassesCounter);

            File.WriteAllText(writeToFile, sb.ToString());
        }

        public HashSet<Stmt> LoadExecutedStatements(string file)
        {
            var result = new HashSet<Stmt>();
            var lines = File.ReadAllLines(file);
            foreach (var line in lines)
            {
                var campos = line.Split(' ');
                var spanStart = Convert.ToInt32(campos[1]);
                var spanEnd = Convert.ToInt32(campos[2]);
                var span = Microsoft.CodeAnalysis.Text.TextSpan.FromBounds(spanStart, spanEnd);
                var currentAst = InstrumentationResult.fileIdToSyntaxTree[Convert.ToInt32(campos[0])];
                var node = currentAst.GetCompilationUnitRoot().FindNode(span);
                var spansCorrectos = GetMaxSpanFromSyntaxNode((CSharpSyntaxNode)node);

                result.Add(new Stmt()
                {
                    FileId = Convert.ToInt32(campos[0]),
                    SpanStart = spansCorrectos.Item1,
                    SpanEnd = spansCorrectos.Item2
                });
            }
            return result;
        }

        public void SaveDependencyGraph(AdjacencyGraph<string, Edge<string>> graph, string writeToFile)
        {
            if (writeToFile == "")
                return;

            if (File.Exists(writeToFile))
                File.Delete(writeToFile);
            var file = File.CreateText(writeToFile);
            foreach (var v in graph.Vertices)
                file.WriteLine(v);
            foreach (var e in graph.Edges)
                file.WriteLine(e.Source + " " + e.Target);
            file.Close();
        }

        public void SaveDependencyGraph(List<AdjacencyGraph<string, Edge<string>>> graphs, string folder)
        {
            if (!System.IO.Directory.Exists(folder))
                System.IO.Directory.CreateDirectory(folder);

            var iterationNumber = 1;
            foreach (var graph in graphs)
                SaveDependencyGraph(graph, System.IO.Path.Combine(folder, "result" + iterationNumber++ + ".dg"));
        }

        public AdjacencyGraph<string, Edge<string>> LoadDependencyGraph(string file)
        {
            var result = new AdjacencyGraph<string, Edge<string>>();
            var lines = File.ReadAllLines(file);
            foreach (var line in lines)
            {
                var split = line.Split(' ');
                if (split.Count() > 1)
                    result.AddEdge(new Edge<string>(split[0], split[1]));
                else
                    result.AddVertex(split[0]);
            }
            return result;
        }

        public AdjacencyGraph<string, Edge<string>> Transformation(AdjacencyGraph<string, Edge<string>> graph)
        {
            // For each vertex we get the file/line. We put all of this in a dictionary, O(N).
            // Then, for each vertex we try to add the file/line to the graph with their correponding edges.
            var returnGraph = new AdjacencyGraph<string, Edge<string>>();
            var localData = new Dictionary<string, Tuple<int, int>>();
            MaxSpans = new Dictionary<string, Tuple<int, int>>();
            VertexToStatement = new Dictionary<string, Stmt>();
            LineStatements = new Dictionary<string, ISet<string>>();
            var tupleToName = new Func<Tuple<int, int>, string>(x => x.Item1 + "." + x.Item2);

            foreach (var v in graph.Vertices.Where(x => (x != DependencyGraphStmtDescriptor.VertexNameForExternal)))
            {
                var vertexStructures = GetFileIdAndLine(v);
                var fileAndLine = vertexStructures.Item1;
                var keyAndStatement = vertexStructures.Item2;
                localData.Add(v, fileAndLine);
                var newVertex = tupleToName(fileAndLine);
                returnGraph.AddVertex(newVertex);

                if (LineStatements.ContainsKey(newVertex))
                    LineStatements[newVertex].Add(keyAndStatement.Item1);
                else
                    LineStatements.Add(newVertex, new HashSet<string>(new string[] { keyAndStatement.Item1 }));
                
                VertexToStatement.Add(keyAndStatement.Item1, keyAndStatement.Item2);
            }
            var edgesSet = new HashSet<string>();
            foreach (var e in graph.Edges.Where(x => (x.Target != DependencyGraphStmtDescriptor.VertexNameForExternal)))
            {
                var source = tupleToName(localData[e.Source]);
                var target = tupleToName(localData[e.Target]);
                var edgeKey = source + "|" + target;
                if (source != target && edgesSet.Add(edgeKey))
                    returnGraph.AddEdge(new Edge<string>(source, target));
            }

            return returnGraph;
        }

        Tuple<Tuple<int, int>, Tuple<string, Stmt>> GetFileIdAndLine(string v)
        {
            var currentFileId = Convert.ToInt32(v.Substring(0, v.IndexOf(':')));
            var initSpanStart = v.IndexOf('[') + 1;
            var endSpanEnd = v.IndexOf(')');
            var difference = endSpanEnd - initSpanStart;
            var keyValues = v.Substring(initSpanStart, difference).Split('.');
            var spanStart = Convert.ToInt32(keyValues[0]);
            var spanEnd = Convert.ToInt32(keyValues[2]);
            var span = Microsoft.CodeAnalysis.Text.TextSpan.FromBounds(spanStart, spanEnd);
            var currentAst = InstrumentationResult.fileIdToSyntaxTree[currentFileId];
            var node = currentAst.GetCompilationUnitRoot().FindNode(span);
            var line = node.GetLocation().GetLineSpan();
            var lineInRoslyn = line.StartLinePosition.Line;
            var fileKeyLine = lineInRoslyn + 1;
            // Updating max spans
            var nombre = currentFileId + "." + fileKeyLine;
            if (!MaxSpans.ContainsKey(nombre))
                MaxSpans.Add(nombre, GetMaxSpanFromSyntaxNode((CSharpSyntaxNode)node));

            return new Tuple<Tuple<int, int>, Tuple<string, Stmt>>(
                new Tuple<int, int>(currentFileId, fileKeyLine), 
                new Tuple<string, Stmt>(v,
                                        new Stmt() {
                                            FileId = currentFileId,
                                            Line = fileKeyLine,
                                            SpanStart = spanStart,
                                            SpanEnd = spanEnd,
                                            CSharpSyntaxNode = (CSharpSyntaxNode)node
                                        }));
        }

        Tuple<int, int> GetMaxSpanFromSyntaxNode(CSharpSyntaxNode node)
        {
            if (!(node is MethodDeclarationSyntax || node is ClassDeclarationSyntax || node is StructDeclarationSyntax || node is AccessorDeclarationSyntax))
                // Before the header we have the block syntax
                while (!(node.Parent is ClassDeclarationSyntax ||
                         node.Parent is StructDeclarationSyntax || 
                         node.Parent is BlockSyntax ||
                         node.Parent is ForStatementSyntax && ((ForStatementSyntax)node.Parent).Statement == node))
                    node = (CSharpSyntaxNode)node.Parent;

            if (node is SwitchStatementSyntax)
                node = ((SwitchStatementSyntax)node).Expression;
            if (node is IfStatementSyntax)
                node = ((IfStatementSyntax)node).Condition;
            if (node is WhileStatementSyntax)
                node = ((WhileStatementSyntax)node).Condition;
            if (node is ForStatementSyntax)
            {
                if (((ForStatementSyntax)node).Condition != null)
                    node = ((ForStatementSyntax)node).Condition;
                else
                    return new Tuple<int, int>(((ForStatementSyntax)node).ForKeyword.Span.Start, ((ForStatementSyntax)node).ForKeyword.Span.End);
            }
            if (node is DoStatementSyntax)
                node = ((DoStatementSyntax)node).Condition;
            if (node is ForEachStatementSyntax)
                node = ((ForEachStatementSyntax)node).Expression;
            if (node is MethodDeclarationSyntax)
                node = ((MethodDeclarationSyntax)node).ParameterList;
            if (node is ConstructorDeclarationSyntax)
                node = ((ConstructorDeclarationSyntax)node).ParameterList;
            if (node is LockStatementSyntax)
                node = ((LockStatementSyntax)node).Expression;
            if (node is AccessorDeclarationSyntax)
                if (node.Parent.Parent is PropertyDeclarationSyntax)
                    return new Tuple<int, int>(((PropertyDeclarationSyntax)node.Parent.Parent).Identifier.Span.Start,
                        ((PropertyDeclarationSyntax)node.Parent.Parent).Identifier.Span.End);
                else if (node.Parent.Parent is IndexerDeclarationSyntax)
                    return new Tuple<int, int>(((IndexerDeclarationSyntax)node.Parent.Parent).ThisKeyword.Span.Start,
                        ((IndexerDeclarationSyntax)node.Parent.Parent).ThisKeyword.Span.End);
            if (node is ClassDeclarationSyntax)
                return new Tuple<int, int>(((ClassDeclarationSyntax)node).Identifier.Span.Start, ((ClassDeclarationSyntax)node).Identifier.Span.End);
            if (node is StructDeclarationSyntax)
                return new Tuple<int, int>(((StructDeclarationSyntax)node).Identifier.Span.Start, ((StructDeclarationSyntax)node).Identifier.Span.End);

            return new Tuple<int, int>(node.Span.Start, node.Span.End);
        }
    }
}
