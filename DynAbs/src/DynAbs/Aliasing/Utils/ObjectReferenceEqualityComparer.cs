using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynAbs
{

    /// borrowed from http://stackoverflow.com/questions/1890058/iequalitycomparert-that-uses-referenceequals
    /// <summary>
    /// A generic object comparerer that would only use object's reference, 
    /// ignoring any <see cref="IEquatable{T}"/> or <see cref="object.Equals(object)"/>  overrides.
    /// </summary>
    public class ObjectReferenceEqualityComparer<T> : EqualityComparer<T>
        where T : class
    {
        private static IEqualityComparer<T> _defaultComparer;

        public new static IEqualityComparer<T> Default
        {
            get { return _defaultComparer ?? (_defaultComparer = new ObjectReferenceEqualityComparer<T>()); }
        }

        #region IEqualityComparer<T> Members

        public override bool Equals(T x, T y)
        {
            return ReferenceEquals(x, y);
        }

        public override int GetHashCode(T obj)
        {
            return System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(obj);
        }

        #endregion
    }

    public class PtgWrapperEqualityComparer : EqualityComparer<PtgWrapper>
    {
        private static IEqualityComparer<PtgWrapper> _defaultComparer;

        public new static IEqualityComparer<PtgWrapper> Default
        {
            get { return _defaultComparer ?? (_defaultComparer = new PtgWrapperEqualityComparer()); }
        }

        #region IEqualityComparer<T> Members

        public override bool Equals(PtgWrapper x, PtgWrapper y)
        {
            if (x.Vertex.IsAlive && y.Vertex.IsAlive)
                return ReferenceEquals(x.Vertex.Target, y.Vertex.Target);
            return false;
        }

        public override int GetHashCode(PtgWrapper obj)
        {
            return obj.GetHashCode();
        }

        #endregion
    }

    public class VertexStringTupleEqualityComparer : EqualityComparer<Tuple<string, PtgVertex>>
    {
        public override bool Equals(Tuple<string, PtgVertex> x, Tuple<string, PtgVertex> y)
        {
            return x.Item1 == y.Item1 && ReferenceEquals(x.Item2, y.Item2);
        }

        public override int GetHashCode(Tuple<string, PtgVertex> obj)
        {
            return System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(obj.Item1) * 31 + System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(obj.Item2);
        }
    }

    public class VertexIntegerTupleEqualityComparer : EqualityComparer<Tuple<int, PtgVertex>>
    {
        public override bool Equals(Tuple<int, PtgVertex> x, Tuple<int, PtgVertex> y)
        {
            return ReferenceEquals(x.Item2, y.Item2);
        }

        public override int GetHashCode(Tuple<int, PtgVertex> obj)
        {
            return System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(obj.Item2);
        }
    }
}
