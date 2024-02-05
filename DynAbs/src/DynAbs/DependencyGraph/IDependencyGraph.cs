using QuikGraph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DynAbs.Tracing;

namespace DynAbs
{
    public interface IDependencyGraph
    {
        uint AddVertex(Stmt stmt, ISet<uint> edges);
        uint CriteriaVertex { get; set; }
        // Uno pide el slice en base a la última línea, otro pide todos los reachables de cada nodo
        ISet<Stmt> Slice();
        List<ISet<Stmt>> GetSlices();
        List<AdjacencyGraph<string, Edge<string>>> GetDependenciesGraphs();
        AdjacencyGraph<string, Edge<string>> GetCompleteDependencyGraph();
        IDictionary<string, string> GetVertexLabels();

        void PrintGraph(string writeToFile);
    }
}
