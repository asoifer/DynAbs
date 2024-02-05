using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynAbs
{
    public class PtgWrapper
    {
        public WeakReference Vertex { get; set; }
        int ptgVertexHashCode;

        public PtgWrapper(PtgVertex Vertex)
        {
            this.Vertex = new WeakReference(Vertex);
            this.ptgVertexHashCode = Vertex.GetHashCode();
        }

        public static bool operator ==(PtgWrapper a, PtgWrapper b)
        {
            if (System.Object.ReferenceEquals(a, b))
                return true;

            if (((object)a == null) || ((object)b == null))
                return false;

            if (a.Vertex.IsAlive != b.Vertex.IsAlive)
                return false;

            if (a.Vertex.IsAlive)
                return ((PtgVertex)a.Vertex.Target) == ((PtgVertex)b.Vertex.Target);

            return false;
        }

        public static bool operator !=(PtgWrapper a, PtgWrapper b)
        {
            return !(a == b);
        }

        public override bool Equals(object obj)
        {
            PtgWrapper other = (PtgWrapper)obj;
            if (Vertex.IsAlive != other.Vertex.IsAlive)
                return false;
            if (Vertex.IsAlive)
                return ((PtgVertex)Vertex.Target).Equals((PtgVertex)other.Vertex.Target);
            return false;
        }

        /// <summary>
        /// Mantenemos siempre el HashCode de los vértices que representamos. 
        /// No pueden cambiar y queremos que los PtgWrappers "se llamen igual" que sus vértices
        /// </summary>
        public override int GetHashCode()
        {
            return ptgVertexHashCode;
        }

        public override string ToString()
        {
            if (Vertex.IsAlive)
                return ((PtgVertex)Vertex.Target).Description;
            return "PtgWrapper with dead reference";
        }
    }

    public class ScopeWrapper
    {
        public WeakReference Scope { get; set; }
        int scopeHashCode;

        public ScopeWrapper(ScopeContainer ScopeContainer)
        {
            this.Scope = new WeakReference(ScopeContainer);
            this.scopeHashCode = ScopeContainer.GetHashCode();
        }

        public static bool operator ==(ScopeWrapper a, ScopeWrapper b)
        {
            if (System.Object.ReferenceEquals(a, b))
                return true;

            if (((object)a == null) || ((object)b == null))
                return false;

            if (a.Scope.IsAlive != b.Scope.IsAlive)
                return false;

            if (a.Scope.IsAlive)
                return ((ScopeContainer)a.Scope.Target) == ((ScopeContainer)b.Scope.Target);

            return false;
        }

        public static bool operator !=(ScopeWrapper a, ScopeWrapper b)
        {
            return !(a == b);
        }

        public override bool Equals(object obj)
        {
            ScopeWrapper other = (ScopeWrapper)obj;
            if (Scope.IsAlive != other.Scope.IsAlive)
                return false;
            if (Scope.IsAlive)
                return ((ScopeContainer)Scope.Target).Equals((ScopeContainer)other.Scope.Target);
            return false;
        }

        /// <summary>
        /// Mantenemos siempre el HashCode de los vértices que representamos. 
        /// No pueden cambiar y queremos que los PtgWrappers "se llamen igual" que sus vértices
        /// </summary>
        public override int GetHashCode()
        {
            return scopeHashCode;
        }

        public override string ToString()
        {
            if (Scope.IsAlive)
                return ((ScopeContainer)Scope.Target).Name;
            return "ScopeContainer with dead reference";
        }
    }
}
