using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynAbs.Aliasing.CS
{
    class TypeKind
    {
        public ISlicerSymbol Type { get; internal set; }
        public string Kind { get; internal set; }

        public TypeKind(ISlicerSymbol Type, string Kind = Globals.DefaultKind) 
        {
            this.Type = Type; 
            this.Kind = Kind; 
        }

        public TypeKind (TypeKind other)
        {
            this.Type = other.Type;
            this.Kind = other.Kind;
        }

        public bool Compatibles(TypeKind other)
        {
            var compatibleTypes = TypesUtils.Compatibles(this.Type, other.Type) || TypesUtils.Compatibles(other.Type, this.Type);
            return compatibleTypes && CSUtils.Compatibles(this.Kind, other.Kind);
        }

        public bool Compatibles(ISlicerSymbol otherType)
        {
            var compatibleTypes = TypesUtils.Compatibles(this.Type, otherType) || TypesUtils.Compatibles(otherType, this.Type);
            return compatibleTypes;
        }

        public bool Compatibles(string otherKind)
        {
            return CSUtils.Compatibles(this.Kind, otherKind);
        }

        #region Operators
        public static bool operator ==(TypeKind a, TypeKind b)
        {
            if (System.Object.ReferenceEquals(a, b))
                return true;

            // If one is null, but not both, return false.
            if (((object)a == null) || ((object)b == null))
                return false;

            return a.Type == b.Type && a.Kind == b.Kind;
        }

        public static bool operator !=(TypeKind a, TypeKind b)
        {
            return !(a == b);
        }

        public override bool Equals(object obj)
        {
            TypeKind other = (TypeKind)obj;
            return this.Type.Equals(other.Type) && this.Kind.Equals(other.Kind);
        }

        public override int GetHashCode()
        {
            return Type.GetHashCode() + 13 * Kind.GetHashCode();
            //return System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(this);
        }

        public override string ToString()
        {
            return this.Type.ToString() + " - " + this.Kind;
        }
        #endregion
    }
}
