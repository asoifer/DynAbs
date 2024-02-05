using QuikGraph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DynAbs.Tracing;

namespace DynAbs
{
    public class PerformanceTestDependencyGraph : IDependencyGraph
    {
        public IDependencyGraph dg;

        public PerformanceTestDependencyGraph(IDependencyGraph dependencyGraph)
        {
            dg = dependencyGraph;
        }

        public uint AddVertex(Stmt stmt, ISet<uint> edges)
        {
            GlobalPerformanceValues.DependencyGraphValues.Counters.AddVertex++;
            var start = DateTime.Now;
            var temp = dg.AddVertex(stmt, edges);
            var diff = DateTime.Now.Subtract(start);
            GlobalPerformanceValues.DependencyGraphValues.Times.AddVertex =
                GlobalPerformanceValues.DependencyGraphValues.Times.AddVertex.Add(diff);
            return temp;
        }


        public uint CriteriaVertex
        {
            get
            {
                return dg.CriteriaVertex;
            }
            set
            {
                dg.CriteriaVertex = value;
            }
        }

        public ISet<Stmt> Slice()
        {
            return dg.Slice();
        }

        public List<ISet<Stmt>> GetSlices()
        {
            return dg.GetSlices();
        }

        public void PrintGraph(string writeToFile)
        {
            dg.PrintGraph(writeToFile);
        }

        public List<AdjacencyGraph<string, Edge<string>>> GetDependenciesGraphs() { return dg.GetDependenciesGraphs(); }
        public AdjacencyGraph<string, Edge<string>> GetCompleteDependencyGraph() { return dg.GetCompleteDependencyGraph(); }
        public IDictionary<string, string> GetVertexLabels() { return dg.GetVertexLabels(); }
    }
}
