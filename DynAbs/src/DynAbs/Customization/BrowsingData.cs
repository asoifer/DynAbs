using QuikGraph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynAbs
{
    public class BrowsingData
    {
        // This the pure dependency graph
        public AdjacencyGraph<string, Edge<string>> RealDG { get; private set; }
        // This is the planned dependency graph, only with files and lines --> there is loss of information
        public AdjacencyGraph<string, Edge<string>> ReducedDG { get; private set; }
        // This is for know the lenght of the line (for underline it)
        public Dictionary<string, Tuple<int, int>> LineSpans { get; private set; }
        public Dictionary<string, Stmt> VertexToStatement { get; private set; }
        public Dictionary<string, ISet<string>> LineStatements { get; private set; }

        internal BrowsingData(SlicerResultsManager ResultsManager, AdjacencyGraph<string, Edge<string>> DG)
        {
            RealDG = DG;
            ReducedDG = ResultsManager.Transformation(DG);
            LineSpans = ResultsManager.MaxSpans;
            LineStatements = ResultsManager.LineStatements;
            VertexToStatement = ResultsManager.VertexToStatement;
        }
    }
}
