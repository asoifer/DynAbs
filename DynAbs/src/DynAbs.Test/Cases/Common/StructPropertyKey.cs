using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynAbs.Test.Cases.Common
{
    class StructPropertyKey
    {
        static void Main()
        {
            var a = new PropertyKey(Guid.NewGuid(), 3);
            var b = a.GetHashCode();
            var c = a.ToString();
            return;
        }

        internal struct PropertyKey : IEquatable<PropertyKey>
        {
            public Guid FormatId { get; set; }

            public Int32 PropertyId { get; set; }

            internal PropertyKey(Guid formatId, Int32 propertyId)
            {
                this.FormatId = formatId;
                this.PropertyId = propertyId;
            }

            public bool Equals(PropertyKey other)
            {
                return other.Equals((object)this);
            }
            
            public override int GetHashCode()
            {
                return FormatId.GetHashCode() ^ PropertyId;
            }

            public override bool Equals(object obj)
            {
                if (obj == null)
                    return false;

                if (!(obj is PropertyKey))
                    return false;

                PropertyKey other = (PropertyKey)obj;
                return other.FormatId.Equals(FormatId) && (other.PropertyId == PropertyId);
            }

            public static bool operator ==(PropertyKey propKey1, PropertyKey propKey2)
            {
                return propKey1.Equals(propKey2);
            }

            public static bool operator !=(PropertyKey propKey1, PropertyKey propKey2)
            {
                return !propKey1.Equals(propKey2);
            }

            public override string ToString()
            {
                return string.Format(System.Globalization.CultureInfo.InvariantCulture,
                    "PropertyKeyFormatString",
                    FormatId.ToString("B"), PropertyId);
            }
        }
    }
}
