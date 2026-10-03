using System.Collections.Generic;

using QuikGraph;

namespace DynAbs
{
    public interface IDependencyGraph
    {
        uint AddVertex(Stmt stmt, ISet<uint> edges);
        uint CriteriaVertex { get; set; }
        
        ISet<Stmt> Slice();
        List<ISet<Stmt>> GetSlices();
        List<AdjacencyGraph<string, Edge<string>>> GetDependenciesGraphs();
        AdjacencyGraph<string, Edge<string>> GetCompleteDependencyGraph();
        IDictionary<string, string> GetVertexLabels();

        uint VertexCount { get; }
        uint EdgeCount { get; }
        void PrintGraph(string writeToFile);
    }
}
