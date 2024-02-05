using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynAbs.Aliasing.CS
{
    class Scope
    {
        /*
        LastDefs :: var → Set<DGNode>
        PointsTo :: var → Set<PtgNode>
        */

        public Dictionary<Field, IntSetWithData> LastDefs = new Dictionary<Field, IntSetWithData>();
        public Dictionary<Field, Dictionary<TypeKind, HashSet<PtgVertex>>> PointsTo = new Dictionary<Field, Dictionary<TypeKind, HashSet<PtgVertex>>>();

        public List<Term> Arguments { get; set; }
        public List<Term> Parameters { get; set; }
        public Term ReturnValue { get; set; }
    }
}
