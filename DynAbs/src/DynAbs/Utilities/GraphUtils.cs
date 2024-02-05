using QuikGraph;
using QuikGraph.Algorithms.ShortestPath;
using QuikGraph.Algorithms.Observers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QuikGraph.Graphviz;

namespace DynAbs
{
    public static class GraphUtils
    {
        public static ISet<T> BFS<T>(AdjacencyGraph<T, Edge<T>> graph, T initial)
        {
            var vertices = new HashSet<T>(new T[] { initial });
            var visited = new HashSet<T>();
            var toVisit = new HashSet<T>();
            toVisit.UnionWith(vertices);

            while (toVisit.Count > 0)
            {
                var visiting = new HashSet<T>();
                foreach (var v in toVisit)
                {
                    if (visited.Contains(v)) continue;

                    foreach (var v2 in graph.OutEdges(v))
                        visiting.Add(v2.Target);
                }
                visited.UnionWith(toVisit);
                toVisit.Clear();
                toVisit.UnionWith(visiting);
            }

            return visited;
        }

        public static AdjacencyGraph<T, Edge<T>> GetSubgraph<T>(AdjacencyGraph<T, Edge<T>> graph, T vertex)
        {
            var returnValue = new AdjacencyGraph<T, Edge<T>>();
            var vertexs = GraphUtils.BFS(graph, vertex);
            returnValue.AddVertex(vertex);
            foreach (var v in vertexs)
                returnValue.AddVertex(v);

            // Agrego ejes
            foreach (var v in vertexs.Union(new T[] { vertex }))
            {
                IEnumerable<Edge<T>> outedges;
                if (graph.TryGetOutEdges(v, out outedges))
                    foreach (var e in outedges)
                        returnValue.AddEdge(new Edge<T>(e.Source, e.Target));
            }

            return returnValue;
        }

        /// <summary>
        /// Idea: dado el grafo (graph), y los nodos alcanzados (reachableVertices), queremos saber el camino mínimo entre la línea sliceada (currentVertex) y 
        /// alguna línea (fileId y lineNumber)... necesitamos el diccionario a Stmt (vertexToStmts) para ver que nodos representan esa línea
        /// </summary>
        public static void GetShortestPathToLine(AdjacencyGraph<uint, Edge<uint>> graph, List<uint> reachableVertices, Dictionary<uint, Stmt> vertexToStmtsRepresented, uint currentVertex, int fileId, int lineNumber)
        {
            var nodosLinea = reachableVertices.Where(x => vertexToStmtsRepresented[x] != null && vertexToStmtsRepresented[x].FileId == 9 && vertexToStmtsRepresented[x].Line == 108);
            var st = new BellmanFordShortestPathAlgorithm<uint, Edge<uint>>(graph, x => 1);

            // Luego en st.Predecessors está el diccionario que nos permite reconstruir el path gracias a este observer
            VertexPredecessorRecorderObserver<uint, Edge<uint>> predecessorObserver = new VertexPredecessorRecorderObserver<uint, Edge<uint>>();
            predecessorObserver.Attach(st);

            st.Compute(currentVertex);
            // Entre otras cosas intentamos obtener la distancia, el método va a fallar si no hay camino
            double distance = 0;
            st.TryGetDistance(nodosLinea.Last(), out distance);
        }

        public static void LoadReverseDependencyGraph(AdjacencyGraph<string, Edge<string>> dependencyGraph, out AdjacencyGraph<string, Edge<string>> reverseDependencyGraph)
        {
            // Para el forward slicing
            reverseDependencyGraph = new AdjacencyGraph<string, Edge<string>>();
            foreach (var vertex in dependencyGraph.Vertices)
                reverseDependencyGraph.AddVertex(vertex);
            foreach (var edge in dependencyGraph.Edges)
                reverseDependencyGraph.AddEdge(new Edge<string>(edge.Target, edge.Source));
        }

        public static string GeneratePTG_DOT(GraphvizAlgorithm<PtgVertex, PtgEdge> graph, string title = null)
        {
            var regexNombre = new System.Text.RegularExpressions.Regex(@"^[0-9]*/[0-9]*\-[0-9]*$");
            var vertexIds = new Dictionary<PtgVertex, int>();
            var i = 0;
            foreach (var vertex in graph.VisitedGraph.Vertices)
                vertexIds.Add(vertex, i++);

            var sb = new StringBuilder();
            sb.AppendLine("digraph G {");
            if (!string.IsNullOrWhiteSpace(title))
                sb.AppendLine(string.Format("label=\"{0}\";", title.Replace("\"", "'")));
            foreach (var vid in vertexIds)
            { 
                var shape = "";
                var color = "white";
                var fontColor = "black";
                var bold = false;
                switch(vid.Key.VertexType)
                {
                    case VertexType.Hub:
                        shape = "diamond";
                        break;
                    case VertexType.EntryPoint:
                        shape = "invhouse";
                        color = "black";
                        fontColor = "white";
                        break;
                    case VertexType.Common:
                        shape = "circle";
                        color = vid.Key.Type.IsObject ? "coral" : "gold";
                        bold = vid.Key.IsMinType;
                        break;
                }
                var label = string.Format(bold ? "{0} : {1}" : "{0} : {1}", regexNombre.IsMatch(vid.Key.Description) ?
                    TermFactory.GetFileTextBySyntaxNodeName(vid.Key.Description).Replace('"', '\'') : Utils.CleanupDotNodeLabel(vid.Key.Description), vid.Key.Type.ToString());
                sb.AppendLine(string.Format("{0} [shape={2}, style=filled, label=\"{1}\", fontcolor={4}, fillcolor={3}];", vid.Value, label, shape, color, fontColor));
            }
            foreach (var e in graph.VisitedGraph.Edges)
            {
                var edgeLabel = "";
                if (e.EdgeType == EdgeType.Lambda)
                    edgeLabel = "L";
                else if (e.EdgeType == EdgeType.Sigma)
                    edgeLabel = "S";
                else if (e.EdgeType == EdgeType.Common)
                    edgeLabel = e.EdgeName.Value;
                sb.AppendLine(string.Format("{0} -> {1} [ label=\"{2}\"];", vertexIds[e.Source], vertexIds[e.Target], edgeLabel));
            }
            sb.AppendLine("}");
            sb.AppendLine("");
            return sb.ToString();
        }

        public static string GeneratePTG_DGML(GraphvizAlgorithm<PtgVertex, PtgEdge> graph, string title = null)
        {
            var regexNombre = new System.Text.RegularExpressions.Regex(@"^[0-9]*/[0-9]*\-[0-9]*$");
            var vertexIds = new Dictionary<PtgVertex, int>();
            var i = 0;
            foreach (var vertex in graph.VisitedGraph.Vertices)
                vertexIds.Add(vertex, i++);

            var sb = new StringBuilder();
            sb.AppendLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
            sb.AppendLine(string.Format("<DirectedGraph Title=\"{0}\" xmlns=\"http://schemas.microsoft.com/vs/2009/dgml\">", (title ?? "").Replace("\"", "'")));
            sb.AppendLine("<Nodes>");
            foreach (var vid in vertexIds)
            {
                var label = regexNombre.IsMatch(vid.Key.Description) ?
                    TermFactory.GetFileTextBySyntaxNodeName(vid.Key.Description).Replace('"', '\'') : Utils.CleanupDotNodeLabel(vid.Key.Description);
                var texture = vid.Key.Multiple ? "Glass" : "Plain";

                string category = "";
                switch (vid.Key.VertexType)
                {
                    case VertexType.Hub:
                        category = "HUB";
                        break;
                    case VertexType.EntryPoint:
                        category = "EntryPoint";
                        break;
                    case VertexType.Common:
                        category = vid.Key.IsMinType ? "CommonMinType" : "Common";
                        break;
                }

                sb.AppendLine(string.Format("<Node Id=\"{0}\" Label=\"{1} : {3}\" Category=\"{2}\" Style=\"{4}\" />", vid.Value, label, category, vid.Key.Type.ToString(), texture));

                //var shape = "";
                //var color = "white";
                //var fontColor = "black";
                //var bold = false;

                //var label = string.Format(bold ? "{0} : {1}" : "{0} : {1}", , vid.Key.Type.ToString());
                //sb.AppendLine(string.Format("{0} [shape={2}, style=filled, label=\"{1}\", fontcolor={4}, fillcolor={3}];", vid.Value, label, shape, color, fontColor));

            }
            sb.AppendLine("</Nodes>");
            sb.AppendLine("<Links>");
            foreach (var e in graph.VisitedGraph.Edges)
            {
                var edgeLabel = "";
                if (e.EdgeType == EdgeType.Lambda)
                    edgeLabel = "L";
                else if (e.EdgeType == EdgeType.Sigma)
                    edgeLabel = "S";
                else if (e.EdgeType == EdgeType.Common)
                    edgeLabel = e.EdgeName.Value;

                sb.AppendLine(string.Format("<Link Source=\"{0}\" Target=\"{1}\" Label=\"{2}\" />", 
                    vertexIds[e.Source], vertexIds[e.Target], edgeLabel)); // Category="PassedTest"
            }
            sb.AppendLine("</Links>");
            sb.AppendLine("<Categories>");
            sb.AppendLine("<Category Id=\"EntryPoint\" Background=\"Black\" />");
            sb.AppendLine("<Category Id=\"HUB\" Background=\"White\" />");
            sb.AppendLine("<Category Id=\"CommonMinType\" Background=\"Yellow\" />");
            sb.AppendLine("<Category Id=\"Common\" Background=\"Orange\" />");
            // ejemplo eje: <Category Id="PassedTest" Label="Passed" Stroke="Black" Background="Green" />  
            // ejemplo eje que hereda: <Category Id="FailedTest" Label="Failed" BasedOn="PassedTest" Background="Red" />  
            // ejemplo nodo: <Category Id="Person" Background="Orange" />  
            sb.AppendLine("</Categories>");
            sb.AppendLine("</DirectedGraph>");
            return sb.ToString();
        }

        public static string GenerateDG_DGML(GraphvizAlgorithm<string, Edge<string>> graph, Func<string, string> nameFunction)
        {
            var sb = new StringBuilder();
            sb.AppendLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
            sb.AppendLine(string.Format("<DirectedGraph Title=\"{0}\" xmlns=\"http://schemas.microsoft.com/vs/2009/dgml\">", "DG"));
            sb.AppendLine("<Nodes>");
            foreach (var vid in graph.VisitedGraph.Vertices)
                sb.AppendLine(string.Format("<Node Id=\"{0}\" Label=\"{1}\" />", vid, nameFunction(vid).Replace("<", "$").Replace(">", "$"))); // 
            sb.AppendLine("</Nodes>");
            sb.AppendLine("<Links>");
            foreach (var e in graph.VisitedGraph.Edges)
                sb.AppendLine(string.Format("<Link Source=\"{0}\" Target=\"{1}\" />", e.Source, e.Target));
            sb.AppendLine("</Links>");
            sb.AppendLine("</DirectedGraph>");
            return sb.ToString();
        }
    }
}
