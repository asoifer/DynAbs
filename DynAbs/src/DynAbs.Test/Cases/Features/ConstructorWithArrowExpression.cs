using System;
using System.Collections.Generic;
using System.Text;

namespace DynAbs.Test.Cases.Features
{
    class ConstructorWithArrowExpression
    {
        public static void Main()
        {
            var r1 = Rope.ForString("test");
            var r2 = Rope.ForString("test");
            var c = Rope.Concat(r1, r2);
            var slicingVariable = c.Length;
            return;
        }

        internal abstract class Rope
        {
            public static readonly Rope Empty = ForString("");
            public abstract override string ToString();
            public abstract int Length { get; }
            protected abstract IEnumerable<char> GetChars();
            private Rope() { }

            public static Rope ForString(string s)
            {
                if (s == null)
                    throw new ArgumentNullException(nameof(s));

                return new StringRope(s);
            }

            public static Rope Concat(Rope r1, Rope r2)
            {
                if (r1 == null)
                    throw new ArgumentNullException(nameof(r1));

                if (r2 == null)
                    throw new ArgumentNullException(nameof(r2));

                return r1.Length == 0 ? r2 : r2.Length == 0 ? r1 : checked(r1.Length + r2.Length < 32) ? ForString(r1.ToString() + r2.ToString()) : new ConcatRope(r1, r2);
            }

            public override bool Equals(object? obj)
            {
                if (!(obj is Rope other) || Length != other.Length)
                    return false;
                if (Length == 0)
                    return true;
                var chars0 = GetChars().GetEnumerator();
                var chars1 = other.GetChars().GetEnumerator();
                while (chars0.MoveNext() && chars1.MoveNext())
                {
                    if (chars0.Current != chars1.Current)
                        return false;
                }

                return true;
            }

            public override int GetHashCode()
            {
                int result = Length;
                foreach (char c in GetChars())
                    result = c * result;

                return result;
            }

            private sealed class StringRope : Rope
            {
                private readonly string _value;
                public StringRope(string value) => _value = value;
                public override string ToString() => _value;
                public override int Length => _value.Length;
                protected override IEnumerable<char> GetChars() => _value;
            }

            private sealed class ConcatRope : Rope
            {
                private readonly Rope _left, _right;
                public override int Length { get; }

                StringBuilder psb = new StringBuilder();

                public ConcatRope(Rope left, Rope right)
                {
                    _left = left;
                    _right = right;
                    Length = checked(left.Length + right.Length);
                }

                public override string ToString()
                {
                    var stack = new Stack<Rope>();
                    stack.Push(this);
                    while (stack.Count != 0)
                    {
                        switch (stack.Pop())
                        {
                            case StringRope s:
                                psb.Append(s.ToString());
                                break;
                            case ConcatRope c:
                                stack.Push(c._right);
                                stack.Push(c._left);
                                break;
                            case var v:
                                throw new Exception(v.GetType().Name);
                        }
                    }

                    return psb.ToString();
                }

                protected override IEnumerable<char> GetChars()
                {
                    var stack = new Stack<Rope>();
                    stack.Push(this);
                    while (stack.Count != 0)
                    {
                        switch (stack.Pop())
                        {
                            case StringRope s:
                                foreach (var c in s.ToString())
                                    yield return c;
                                break;
                            case ConcatRope c:
                                stack.Push(c._right);
                                stack.Push(c._left);
                                break;
                            case var v:
                                throw new Exception(v.GetType().Name);
                        }
                    }
                }
            }
        }
    }
}
