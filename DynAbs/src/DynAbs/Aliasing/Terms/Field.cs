using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynAbs
{
    public class Field
    {
        public static readonly Field SIGMA_FIELD = SigmaField();
        public static readonly Field SIGMA_OBJ = SigmaField(ISlicerSymbol.CreateObjectSymbol());

        public const string SPECIAL_LAMBDA = "__LAMBDA__";
        public const string SPECIAL_SIGMA = "__SIGMA__";
        private Field field;

        public FieldType FieldType { get; set; }
        public string Value { get; private set; }
        public ISlicerSymbol Symbol { get; set; }
        public bool IsArray
        {
            get
            {
                return Value[0] == '[';
            }
        }

        public override string ToString()
        {
            return Value;
        }

        public static bool operator ==(Field a, Field b)
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

            return a.Value.Equals(b.Value) && a.FieldType == b.FieldType;
        }

        public static bool operator !=(Field a, Field b)
        {
            return !(a == b);
        }

        public override bool Equals(object obj)
        {
            Field other = (Field)obj;
            return Value.Equals(other.Value) && FieldType == other.FieldType;
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }

        public static Field SigmaField(ISlicerSymbol symbol = null)
        {
            Field ret = new Field(SPECIAL_SIGMA, symbol);
            ret.FieldType = FieldType.Sigma;
            return ret;
        }

        public Field(string s, ISlicerSymbol symbol)
        {
            Value = s;
            FieldType = FieldType.Common;
            Symbol = symbol;
        }

        public Field(Field other)
        {
            this.FieldType = other.FieldType;
            this.field = other.field;
            this.Value = other.Value;
            this.Symbol = other.Symbol;
        }
    }
}
