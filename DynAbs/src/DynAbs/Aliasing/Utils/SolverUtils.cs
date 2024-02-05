
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynAbs
{
    class SolverUtils
    {
        public static ISet<PtgVertex> CreateReferenceComparedPTGHashSet()
        {
            return new HashSet<PtgVertex>(new ObjectReferenceEqualityComparer<PtgVertex>());
        }

        public static ISet<PtgVertex> CreateReferenceComparedPTGHashSet(IEnumerable<PtgVertex> nodos)
        {
            return new HashSet<PtgVertex>(nodos, new ObjectReferenceEqualityComparer<PtgVertex>());
        }
    }
}
