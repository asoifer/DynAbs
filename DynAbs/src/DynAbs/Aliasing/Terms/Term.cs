using Microsoft.CodeAnalysis;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DynAbs.Tracing;

namespace DynAbs
{
    public class Term
    {
        public Stmt Stmt { get; set; }

        public bool IsGlobal { get; set; }
        
        public bool IsScalar { get; set; }

        public bool IsStruct { get; set; }

        public Term ReferencedTerm { get; set; }

        public bool IsOutOrRef { get; set; }

        public bool IsTypeObject { get; set; }

        public bool IsTemporal { get; set; }

        public bool IsCallbackAlloc { get; set; }

        public bool IsRef { get; set; }

        public bool IsInitializedForException { get; set; }

        public bool IsVar
        {
            get
            {
                return Parts.Count == 1;
            }
        }

        public IList<Field> Parts { get; internal set; }
        public Field First
        {
            get
            {
                return Parts.First();
            }
        }
        public Field Last
        {
            get
            {
                return Parts.Last();
            }
        }

        public bool IsArrayAccess
        {
            get
            {
                return Parts.Last().IsArray;
            }
        }

        public bool IsDynamic
        {
            get
            {
                return Parts.Last().Symbol.Symbol is IDynamicTypeSymbol;
            }
        }

        public int Count { get { return Parts.Count; } }

        public string DebuggerDisplay { get; internal set; }

        public Term(string str, ISlicerSymbol symbol = null)
        {
            //this.Parts = Regex.Replace(str, @"\[.*?\]", ".[]").Split('.').Select(x => new Field(x, symbol)).ToList();
            this.Parts = new List<Field>() { new Field(str, symbol) };
            DebuggerDisplay = str;
        }

        public Term AddingField(Field field)
        {
            List<Field> parts = new List<Field>(Parts);
            parts.Add(field);
            return TermFactory.Create(parts, IsGlobal, false, IsCallbackAlloc);
        }

        public Term AddingField(string field, ISlicerSymbol symbol = null)
        {
            return AddingField(new Field(field, symbol));
        }

        public Term(IList<Field> newParts, bool isGlobal)
        {
            this.Parts = (IList<Field>)newParts;
            this.IsGlobal = isGlobal;
            DebuggerDisplay = string.Join(".", newParts);
        }

        public Term DiscardFirst()
        {
            if (Parts.Count == 1) throw new InvalidOperationException("Cannot discard");

            IList<Field> newParts = Parts.Skip(1).ToList();
            var returnTerm = TermFactory.Create(newParts, IsGlobal, IsTemporal && Count == 1, IsCallbackAlloc);
            returnTerm.Stmt = this.Stmt;
            return returnTerm;
        }

        public Term DiscardLast()
        {
            if (Parts.Count == 1) throw new InvalidOperationException("Cannot discard");

            IList<Field> newParts = Parts.Take(Parts.Count - 1).ToList();
            var returnTerm = TermFactory.Create(newParts, IsGlobal, IsTemporal && Count == 1, IsCallbackAlloc);
            returnTerm.Stmt = this.Stmt;
            return returnTerm;
        }

        public override string ToString()
        {
            return string.Join(".", Parts);
        }

        public override bool Equals(object obj)
        {
            Term other = (Term)obj;
            return obj.ToString().Equals(ToString());
        }

        public override int GetHashCode()
        {
            return ToString().GetHashCode();
        }

        public Field this[int i]
        {
            get { return Parts[i]; }
            set { Parts[i] = value; }
        }
    }
}