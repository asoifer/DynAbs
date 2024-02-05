using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynAbs.Aliasing.CS
{
    class PtgVertex
    {
        public int ID = Globals.NextPtgVertexID++;
        public TypeKind TypeKind { get; set; }
        public bool IsMinType { get; set; }
        public bool IsMultiple { get; set; }
        public int Rank { get; set; }
        public PtgVertex Parent { get; set; }
        public Cluster Rep { get; set; }

        public PtgVertex(TypeKind typeKind)
        {
            TypeKind = typeKind;
            IsMinType = true;
            IsMultiple = false;
            Parent = this;
            Rank = 0;
            Rep = new Cluster(TypeKind, true);
        }

        public PtgVertex(TypeKind typeKind, bool multiple)
        {
            TypeKind = typeKind;
            IsMinType = !multiple;
            IsMultiple = multiple;
            Parent = this;
            Rank = 0;
            Rep = new Cluster(TypeKind, !multiple);
        }

        public PtgVertex(PtgVertex other)
        {
            TypeKind = other.TypeKind;
            IsMinType = other.IsMinType;
            IsMultiple = other.IsMultiple;
            Rank = 0;
            Parent = this;
            Rep = new Cluster(TypeKind, !IsMultiple);
        }

        public PtgVertex Find()
        {
            if (this != Parent && Parent.Parent != Parent)
                Parent = Parent.Find(); //path compression
            return this.Parent;
        }
    }
}
