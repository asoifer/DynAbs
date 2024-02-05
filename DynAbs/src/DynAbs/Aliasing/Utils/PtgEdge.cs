using QuikGraph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynAbs
{
    public class PtgEdge : IEdge<PtgVertex>
    {
        public Field EdgeName { get; set; }
        public PtgVertex Source { get; set; }
        public PtgVertex Target { get; set; }
        public EdgeType EdgeType { get; set; }

        public PtgEdge()
        {
            EdgeType = EdgeType.Common;
        }

        public static bool operator ==(PtgEdge a, PtgEdge b)
        {
            if (System.Object.ReferenceEquals(a, b))
            {
                return true;
            }

            // If one is null, but not both, return false.
            if (((object)a == null) || ((object)b == null))
            {
                return false;
            }

            return a.Source == b.Source && a.Target == b.Target && a.EdgeType == b.EdgeType;
        }
        
        public static bool operator !=(PtgEdge a, PtgEdge b)
        {
            return !(a == b);
        }

        public override bool Equals(object obj)
        {
            PtgEdge other = (PtgEdge)obj;
            return this.Source.Equals(other.Source) && this.Target.Equals(other.Target) && this.EdgeType.Equals(other.EdgeType);
        }

        public override int GetHashCode()
        {
            return this.Source.GetHashCode() * 31 + this.Target.GetHashCode();
        } 
    }
}
