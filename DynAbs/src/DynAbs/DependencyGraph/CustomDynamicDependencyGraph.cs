using QuikGraph;
using QuikGraph.Algorithms.Search;
using QuikGraph.Graphviz;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace DynAbs
{
    public class CustomDynamicDependencyGraph : IDependencyGraph
    {
        public class Node
        {
            public Node(uint id) { this.id = id; }
            public uint id { get; set; }
            public HashSet<uint> edges { get; set; } = new HashSet<uint>();
            public override int GetHashCode()
            {
                return (int)id;
            }
            public override bool Equals(object obj)
            {
                return ((Node)obj).id == this.id;
            }
        }

        Dictionary<uint, Node> nodes = new Dictionary<uint, Node>();
        DependencyGraphStmtDescriptor dgStmtDescriptor = new DependencyGraphStmtDescriptor();
        private readonly IDictionary<string, string> vertexNameToVertexLabel = new Dictionary<string, string>();
        private uint lastVertexAdded;
        public uint CriteriaVertex { get; set; }

        // 1 is for EXTERNAL, nextId() begins in 2.
        // External is defined in DependencyGraphStmtDescriptor.
        uint id = 1;
        uint nextId()
        {
            return ++id;
        }
        Dictionary<uint, string> vtxIdToName = new Dictionary<uint, string>();

        // Approach 3 structures
        private Dictionary<uint, Stmt> vertexToStmtsRepresented = new Dictionary<uint, Stmt>();
        // We store the previous slices
        List<ISet<Stmt>> slices = new List<ISet<Stmt>>();
        List<AdjacencyGraph<string, Edge<string>>> slicesGraph = new List<AdjacencyGraph<string, Edge<string>>>();

        public CustomDynamicDependencyGraph()
        {
            var committingVertex = DependencyGraphStmtDescriptor.VertexIdForExternal;
            var name = DependencyGraphStmtDescriptor.VertexNameForExternal;
            AddVertex(committingVertex);
            vtxIdToName.Add(committingVertex, name);
            vertexToStmtsRepresented.Add(1, null);
        }

        void AddVertex(uint vertex)
        {
            var n = new Node(vertex);
            nodes.Add(vertex, n);
        }

        public uint AddVertex(Stmt stmtBeingTreated, ISet<uint> dependenciesBeingAdded)
        {
            dgStmtDescriptor.UpdateStmtOcurrence(stmtBeingTreated);
            var committingVertex = nextId();
            var name = dgStmtDescriptor.VertexNameForStmt(stmtBeingTreated);
            vertexToStmtsRepresented.Add(committingVertex, stmtBeingTreated);
            Trace.Assert(committingVertex != 0);
            lastVertexAdded = committingVertex;
            vtxIdToName.Add(committingVertex, name);
            UpdateQuikGraph(stmtBeingTreated, dependenciesBeingAdded, committingVertex);
            return committingVertex;
        }

        private void UpdateQuikGraph(Stmt stmtBeingTreated, ISet<uint> dependenciesBeingAdded, uint committingVertex)
        {
            var n = new Node(committingVertex);
            nodes.Add(committingVertex, n);
            foreach (var d in dependenciesBeingAdded)
                n.edges.Add(d);
            
            var nodeDescription = dgStmtDescriptor.NodeDescription(stmtBeingTreated) + " " + committingVertex;
            vertexNameToVertexLabel[vtxIdToName[committingVertex]] = nodeDescription;
        }

        public ISet<Stmt> Slice()
        {
            var currentVertex = CriteriaVertex != 0 ? CriteriaVertex : lastVertexAdded;
            var bfsResult = BFS(currentVertex);
            var stmts = bfsResult.Select(x => vertexToStmtsRepresented[x]).Where(x => x != null);
            ISet<Stmt> retSet = new HashSet<Stmt>();
            foreach (var singleStmt in stmts)
                retSet.Add(singleStmt);

            var slicedStmt = new HashSet<Stmt>(retSet, new StmtFileAndLineEqualityComparer());
            slices.Add(slicedStmt);

            if (Globals.generate_dgs)
            {
                var adjGraph = GetAdjacencyGraph();
                var convertedGraph = TranslateGraph(GraphUtils.GetSubgraph(adjGraph, currentVertex));
                slicesGraph.Add(convertedGraph);
            }

            return slicedStmt;
        }

        public List<ISet<Stmt>> GetSlices()
        {
            return slices;
        }

        public List<AdjacencyGraph<string, Edge<string>>> GetDependenciesGraphs() 
        {
            if (Globals.generate_dgs)
                return slicesGraph;

            return new List<AdjacencyGraph<string, Edge<string>>>(); 
        }
        public AdjacencyGraph<string, Edge<string>> GetCompleteDependencyGraph() 
        {
            if (Globals.generate_dgs)
                return TranslateGraph(GetAdjacencyGraph());

            return new AdjacencyGraph<string, Edge<string>>(); 
        }
        
        public IDictionary<string, string> GetVertexLabels()
        {
            return vertexNameToVertexLabel;
        }

        public void PrintGraph(string writeToFile)
        {
            if (File.Exists(writeToFile)) File.Delete(writeToFile);
            var gv = new GraphvizAlgorithm<string, Edge<string>>(TranslateGraph(GetAdjacencyGraph()));
            gv.FormatVertex += (s, formatArgs) =>
            {
                formatArgs.VertexFormat.Label = formatArgs.Vertex.Equals(DependencyGraphStmtDescriptor.VertexNameForExternal) ? formatArgs.Vertex : vertexNameToVertexLabel[formatArgs.Vertex];
            };
            File.WriteAllText(writeToFile, gv.Generate());

            Func<string, string> func = x => x.Equals(DependencyGraphStmtDescriptor.VertexNameForExternal) ? x : vertexNameToVertexLabel[x];
            File.WriteAllText(writeToFile.Replace(".dot", ".dgml"), GraphUtils.GenerateDG_DGML(gv, func));
        }

        public void PrintSlicedGraph(string writeToFile)
        {
            if (File.Exists(writeToFile)) File.Delete(writeToFile);
            var gv = new GraphvizAlgorithm<string, Edge<string>>(slicesGraph[0]);
            gv.FormatVertex += (s, formatArgs) =>
            {
                formatArgs.VertexFormat.Label = formatArgs.Vertex.Equals(DependencyGraphStmtDescriptor.VertexNameForExternal) ? formatArgs.Vertex : vertexNameToVertexLabel[formatArgs.Vertex];
            };
            File.WriteAllText(writeToFile, gv.Generate());

            Func<string, string> func = x => x.Equals(DependencyGraphStmtDescriptor.VertexNameForExternal) ? x : vertexNameToVertexLabel[x];
            File.WriteAllText(writeToFile.Replace(".dot", ".dgml"), GraphUtils.GenerateDG_DGML(gv, func));
        }

        public AdjacencyGraph<string, Edge<string>> TranslateGraph(AdjacencyGraph<uint, Edge<uint>> graph)
        {
            AdjacencyGraph<string, Edge<string>> convertedGraph = new AdjacencyGraph<string, Edge<string>>();
            foreach (var vtx in graph.Vertices)
            {
                convertedGraph.AddVertex(vtxIdToName[vtx]);
            }
            foreach (var edge in graph.Edges)
            {
                Edge<string> newEdge = new Edge<string>(vtxIdToName[edge.Source], vtxIdToName[edge.Target]);
                convertedGraph.AddEdge(newEdge);
            }
            return convertedGraph;
        }

        #region Aux classes and procedures
        HashSet<uint> BFS(uint from)
        {
            var visited = new HashSet<uint>();
            visited.Add(from);

            var toVisit = new HashSet<uint>();

            var targets = nodes[from].edges;
            toVisit.UnionWith(targets);

            while (toVisit.Count > 0)
            {
                var v = toVisit.First();
                toVisit.Remove(v);
                if (visited.Contains(v))
                    continue;
                visited.Add(v);

                targets = nodes[v].edges;
                toVisit.UnionWith(targets.Where(x => !visited.Contains(x)));
            }

            return visited;
        }

        AdjacencyGraph<uint, Edge<uint>> GetAdjacencyGraph()
        {
            var graph = new AdjacencyGraph<uint, Edge<uint>>();

            foreach (var node in nodes)
                graph.AddVertex(node.Key);

            foreach (var node in nodes)
                foreach (var edge in node.Value.edges)
                    graph.AddEdge(new Edge<uint>(node.Key, edge));

            return graph;
        }
        #endregion
    }
}
