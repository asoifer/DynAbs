using QuikGraph;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DynAbs.Tracing;
using QuikGraph.Graphviz;

namespace DynAbs
{
    class SubsumedDependencyGraph : IDependencyGraph
    {
        // Main graph
        AdjacencyGraph<uint, Edge<uint>> graph = new AdjacencyGraph<uint, Edge<uint>>();

        // Approach 4 structures
        // A) Given a node of the graph, we get every reachable statement
        Dictionary<uint, ISet<Stmt>> reachableStmts = new Dictionary<uint, ISet<Stmt>>();
        // B) Given a statement, we get those vertices which represent it on the graph
        Dictionary<Stmt, ISet<uint>> stmtToRepresentingVertices = new Dictionary<Stmt, ISet<uint>>();

        // 1 is for EXTERNAL, nextId() begins in 2.
        // External is defined in DependencyGraphStmtDescriptor.
        uint id = 1; 
        uint nextId()
        {
            return ++id;
        }
        Dictionary<uint, string> vtxIdToName = new Dictionary<uint, string>();

        // Auxiliary structures
        DependencyGraphStmtDescriptor dgStmtDescriptor = new DependencyGraphStmtDescriptor();
        IDictionary<string, string> vertexNameToVertexLabel = new Dictionary<string, string>();
        public uint CriteriaVertex { get; set; }
        uint lastVertexAdded;

        // We store the previous slices
        List<ISet<Stmt>> slices = new List<ISet<Stmt>>();
        List<AdjacencyGraph<string, Edge<string>>> slicesGraph = new List<AdjacencyGraph<string, Edge<string>>>();

        public SubsumedDependencyGraph()
        {
            var committingVertex = DependencyGraphStmtDescriptor.VertexIdForExternal;
            var name = DependencyGraphStmtDescriptor.VertexNameForExternal;
            reachableStmts.Add(committingVertex, new HashSet<Stmt>());
            graph.AddVertex(committingVertex);
            vtxIdToName.Add(committingVertex, name);
        }

        public uint AddVertex(Stmt stmtBeingTreated, ISet<uint> dependenciesBeingAdded)
        {
            // Inicialmente se actualiza el contador de la cantidad de apariciones de un statement.
            dgStmtDescriptor.UpdateStmtOcurrence(stmtBeingTreated);

            // A) Dado un statement, tratamos de obtener los nodos que lo representan
            // Si el statement que estamos tratando no tiene ningún vértice que lo represente agregamos la entrada en el diccionario
            if (!stmtToRepresentingVertices.ContainsKey(stmtBeingTreated))
                stmtToRepresentingVertices.Add(stmtBeingTreated, new HashSet<uint>());

            // NOTA: Si uno quisiera chequear que hay un solo conjunto que cumple esta propiedad 
            // puede utilizar SingleOrDefault en lugar de FirstOrDefault
            // Aclaración: Si un vértice depende de un representativo no saltaría acá porque salta en el subsume y no se agrega. 
            // Por lo tanto: Si un vértice depende de un representativo, la comparación tendrá que ser con la unión de los ejes de salida del grafo.
            var committingVertex = stmtToRepresentingVertices[stmtBeingTreated]
                .FirstOrDefault(x =>
                    (dependenciesBeingAdded.Contains(x) ?
                     dependenciesBeingAdded.SetEquals(graph.OutEdges(x).Select(y => y.Target).Union(Enumerable.Repeat(x, 1))) :
                    (dependenciesBeingAdded.SetEquals(graph.OutEdges(x).Select(y => y.Target)))));

            if (committingVertex == 0)
            {
                ISet<Stmt> biggest = new HashSet<Stmt>();
                foreach (var v in dependenciesBeingAdded)
                    if (reachableStmts[v].Contains(stmtBeingTreated) && biggest.Count < reachableStmts[v].Count)
                    {
                        biggest = reachableStmts[v];
                        committingVertex = v;
                    }

                if (committingVertex != 0)
                    foreach (var w in dependenciesBeingAdded)
                        if (w != committingVertex)
                        {
                            if (!reachableStmts[w].IsSubsetOf(biggest))
                            {
                                committingVertex = 0;
                                break;
                            }
                        }
            }

            // Si no pudimos reutilizar nada agregamos el nodo al grafo
            // CASO CONTRARIO:
            // Estaríamos devolviendo committingVertex en lugar del nuevo nombre para el statement actual
            if (committingVertex == 0)
            {
                var name = dgStmtDescriptor.VertexNameForStmt(stmtBeingTreated);
                committingVertex = nextId();
                vtxIdToName.Add(committingVertex, name);

                // Actualizamos la estructura de datos indicando que nuestro vértice representa a todos estos statements
                var reachableStmtsFromPotentialNewVertex = new HashSet<Stmt>() { stmtBeingTreated };
                foreach (var v in dependenciesBeingAdded)
                    reachableStmtsFromPotentialNewVertex.UnionWith(reachableStmts[v]);
                this.reachableStmts.Add(committingVertex, reachableStmtsFromPotentialNewVertex);
                
                // Actualizamos el grafo
                UpdateQuikGraph(stmtBeingTreated, dependenciesBeingAdded, committingVertex);
            }

            // Actualizamos la estructura que indica que el statement está representado por este vértice
            stmtToRepresentingVertices[stmtBeingTreated].Add(committingVertex);

            Debug.Assert(committingVertex != 0);

            lastVertexAdded = committingVertex;
            return committingVertex;
        }

        /// <summary>
        /// Agrega un nodo y sus ejes al grafo
        /// </summary>
        void UpdateQuikGraph(Stmt stmtBeingTreated, ISet<uint> dependenciesBeingAdded, uint committingVertex)
        {
            // 1: Agregamos el vértice al grafo
            graph.AddVertex(committingVertex);
            // 2: Para cada dependencia agregamos un eje
            // NOTA: Se chequea en una estructura auxiliar que el vértice no exista previamente 
            // ya que según documentaron, puede fallar el QuikGraph
            foreach (var toVertex in dependenciesBeingAdded)
                graph.AddEdge(new Edge<uint>(committingVertex, toVertex));
            
            // Agregamos la descripción del vértice que estamos agregando
            vertexNameToVertexLabel[vtxIdToName[committingVertex]] = dgStmtDescriptor.NodeDescription(stmtBeingTreated) + " " + committingVertex;
        }

        #region Result
        public ISet<Stmt> Slice()
        {
            var currentVertex = CriteriaVertex != 0 ? CriteriaVertex : lastVertexAdded;
            var reachables = reachableStmts[currentVertex];
            // Utilizamos el comparador de File & Line porque buscamos eso en lugar de SpanStart/SpanEnd
            var slicedStatements = new HashSet<Stmt>(reachables, new StmtFileAndLineEqualityComparer());
            slices.Add(slicedStatements);

            var convertedGraph = TranslateGraph(GraphUtils.GetSubgraph(graph, currentVertex));
            slicesGraph.Add(convertedGraph);

            return slicedStatements;
        }

        public List<ISet<Stmt>> GetSlices()
        {
            return slices;
        }

        public List<AdjacencyGraph<string, Edge<string>>> GetDependenciesGraphs()
        {
            return slicesGraph;
        }

        public AdjacencyGraph<string, Edge<string>> GetCompleteDependencyGraph()
        {
            return TranslateGraph(graph);
        }

        public IDictionary<string, string> GetVertexLabels()
        {
            return vertexNameToVertexLabel;
        }

        public void PrintGraph(string writeToFile)
        {
            Console.WriteLine("Cantidad de nodos del DG: " + graph.VertexCount);
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
        #endregion
    }
}
