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
    public class DynamicDependencyGraph : IDependencyGraph
    {
        private readonly AdjacencyGraph<uint, Edge<uint>> graph = new AdjacencyGraph<uint, Edge<uint>>();
        private DependencyGraphStmtDescriptor dgStmtDescriptor = new DependencyGraphStmtDescriptor();
        private readonly IDictionary<string, string> vertexNameToVertexLabel = new Dictionary<string, string>();
        private uint lastVertexAdded;
        public uint CriteriaVertex { get; set; }

        // Aux structure because ContainsEdge of QuikGraph is not properly working
        private ISet<uint> hashEdges = new HashSet<uint>();

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

        public DynamicDependencyGraph()
        {
            var committingVertex = DependencyGraphStmtDescriptor.VertexIdForExternal;
            var name = DependencyGraphStmtDescriptor.VertexNameForExternal;
            graph.AddVertex(committingVertex);
            vtxIdToName.Add(committingVertex, name);
            vertexToStmtsRepresented.Add(1, null);
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
            graph.AddVertex(committingVertex);

            foreach (var toVertex in dependenciesBeingAdded)
                graph.AddEdge(new Edge<uint>(committingVertex, toVertex));
            var nodeDescription = dgStmtDescriptor.NodeDescription(stmtBeingTreated) + " " + committingVertex;
            vertexNameToVertexLabel[vtxIdToName[committingVertex]] = nodeDescription;
        }

        public ISet<Stmt> Slice()
        {
            var bfs = new BreadthFirstSearchAlgorithm<uint, Edge<uint>>(graph);

            List<uint> reachableVertices = new List<uint>();
            bfs.DiscoverVertex += (vtx) =>
            {
                reachableVertices.Add(vtx);
            };

            var currentVertex = CriteriaVertex != 0 ? CriteriaVertex : lastVertexAdded;
            bfs.Compute(currentVertex);

            var stmts = reachableVertices.Select(x => vertexToStmtsRepresented[x]).Where(x => x != null);
            ISet<Stmt> retSet = new HashSet<Stmt>();
            foreach (var singleStmt in stmts)
                retSet.Add(singleStmt);

            var slicedStmt = new HashSet<Stmt>(retSet, new StmtFileAndLineEqualityComparer());
            slices.Add(slicedStmt);
            var subgraph = GraphUtils.GetSubgraph(graph, currentVertex);
            var convertedGraph = TranslateGraph(subgraph);
            slicesGraph.Add(convertedGraph);

            return slicedStmt;
        }

        public List<ISet<Stmt>> GetSlices()
        {
            return slices;
        }

        public List<AdjacencyGraph<string, Edge<string>>> GetDependenciesGraphs() { return slicesGraph; }
        public AdjacencyGraph<string, Edge<string>> GetCompleteDependencyGraph() { return TranslateGraph(graph); }
        public IDictionary<string, string> GetVertexLabels()
        {
            return vertexNameToVertexLabel;
        }

        public void PrintGraph(string writeToFile)
        {
            if (File.Exists(writeToFile)) File.Delete(writeToFile);
            var gv = new GraphvizAlgorithm<string, Edge<string>>(TranslateGraph(graph));
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
    }
}
