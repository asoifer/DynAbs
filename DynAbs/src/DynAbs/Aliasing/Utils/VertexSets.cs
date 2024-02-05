using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynAbs
{
    public class VertexSets
    {
        public ISet<PtgVertex> WritableVertex { get; set; }
        public ISet<PtgVertex> ReadOnlyVertex { get; set; }
        public ISlicerSymbol Symbol { get; set; }

        public VertexSets()
        {
            WritableVertex = SolverUtils.CreateReferenceComparedPTGHashSet();
            ReadOnlyVertex = SolverUtils.CreateReferenceComparedPTGHashSet();
        }
        public VertexSets(ISet<PtgVertex> WritableVertex, ISet<PtgVertex> ReadOnlyVertex, ISlicerSymbol Symbol)
        {
            this.WritableVertex = WritableVertex;
            this.ReadOnlyVertex = ReadOnlyVertex;
            this.Symbol = Symbol;
        }
    }
}
