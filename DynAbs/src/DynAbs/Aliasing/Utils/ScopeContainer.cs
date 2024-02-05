using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynAbs
{
    public class ScopeContainer
    {
        public IDictionary<string, VertexSetWithType> EntryPointVerticesDict { get; set; }
        public IDictionary<Field, IntSetWithData> LastDefDict { get; set; }
        public Term ReturnValue { get; set; }
        public string Name { get; set; }
        public IMethodSymbol MethodSymbol { get; set; }
        public string BaseKey { get; set; }
        public bool IsGlobalScope { get; set; }
        public bool IsStaticModeStart { get; set; }
        public bool Alive { get; set; }

        public ScopeContainer(bool isGlobalScope = false)
        {
            EntryPointVerticesDict = new Dictionary<string, VertexSetWithType>();
            LastDefDict = new Dictionary<Field, IntSetWithData>();
            Name = GetFreshName();
            Alive = true;
            IsGlobalScope = isGlobalScope;
        }

        public List<Term> Arguments { get; set; }

        public List<Term> Parameters { get; set; }

        public void AddEntryPointValues(string key, IEnumerable<PtgVertex> value, ISlicerSymbol type = null)
        {
            if (!EntryPointVerticesDict.ContainsKey(key))
            {
                if (type == null)
                    throw new SlicerException("No hubo asignacón de tipo");

                EntryPointVerticesDict.Add(key, new VertexSetWithType(SolverUtils.CreateReferenceComparedPTGHashSet(), type));
            }

            EntryPointVerticesDict[key].VertexSet.UnionWith(value);

            foreach (var ptgVertex in value)
                ptgVertex.AddEntryPoint(this, key);
        }

        public void AddEntryPointValue(string key, PtgVertex value, ISlicerSymbol type = null)
        {
            if (!EntryPointVerticesDict.ContainsKey(key))
            {
                if (type == null)
                    throw new SlicerException("No hubo asignacón de tipo");

                EntryPointVerticesDict.Add(key, new VertexSetWithType(SolverUtils.CreateReferenceComparedPTGHashSet(), type));
            }

            EntryPointVerticesDict[key].VertexSet.Add(value);

            value.AddEntryPoint(this, key);
        }

        public void DeleteEntryPointValue(string key)
        {
            if (EntryPointVerticesDict.ContainsKey(key))
                foreach (var ep in EntryPointVerticesDict[key].VertexSet)
                    ep.RemoveEntryPoint(this, key);
            EntryPointVerticesDict.Remove(key);
        }

        /// <summary>
        /// This is not removing the reference from the node to avoid modify the enumeration. Everything is deleted at the end.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public ISlicerSymbol RemoveEntryPointValueLambdaCompression(string key, PtgVertex value)
        {
            if (!EntryPointVerticesDict.ContainsKey(key))
                return null;

            var returnValue = EntryPointVerticesDict[key].VertexType;
            EntryPointVerticesDict[key].VertexSet.Remove(value);
            return returnValue;
        }

        public void OverrideEntryPointValues(string key, ISet<PtgVertex> value, ISlicerSymbol type)
        {
            if (EntryPointVerticesDict.ContainsKey(key))
                foreach (var ptgVertex in EntryPointVerticesDict[key].VertexSet)
                    ptgVertex.RemoveEntryPoint(this, key);

            EntryPointVerticesDict[key] = new VertexSetWithType(value, type);

            foreach (var ptgVertex in value)
                ptgVertex.AddEntryPoint(this, key);
        }

        public void OverrideEntryPointValue(string key, PtgVertex value, ISlicerSymbol type)
        {
            if (EntryPointVerticesDict.ContainsKey(key))
                foreach (var ptgVertex in EntryPointVerticesDict[key].VertexSet)
                    ptgVertex.RemoveEntryPoint(this, key);

            EntryPointVerticesDict[key] = new VertexSetWithType(SolverUtils.CreateReferenceComparedPTGHashSet(new PtgVertex[] { value }), type);

            value.AddEntryPoint(this, key);
        }

        public void RemoveEntryPointsToYou()
        {
            foreach (var keyValue in EntryPointVerticesDict)
                foreach (var ptgVertex in keyValue.Value.VertexSet)
                    ptgVertex.RemoveEntryPointStack(this);
        }

        public static bool operator ==(ScopeContainer a, ScopeContainer b)
        {
            if (System.Object.ReferenceEquals(a, b))
                return true;

            // If one is null, but not both, return false.
            if (((object)a == null) || ((object)b == null))
                return false;

            return a.Name == b.Name;
        }

        public static bool operator !=(ScopeContainer a, ScopeContainer b)
        {
            return !(a == b);
        }

        public override bool Equals(object obj)
        {
            ScopeContainer other = (ScopeContainer)obj;
            return this.Name.Equals(other.Name);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public static string GetFreshName()
        {
            return (++Globals.ScopesInternalCounter).ToString();
        }
    }

    public class VertexSetWithType
    {
        public ISet<PtgVertex> VertexSet;
        public ISlicerSymbol VertexType;
        public VertexSetWithType(ISet<PtgVertex> VertexSet, ISlicerSymbol VertexType)
        {
            this.VertexSet = VertexSet;
            this.VertexType = VertexType;
        }
    }

    public class IntSetWithData
    {
        public ISet<uint> IntSet;
        public bool IsTemporal;
        public int LoopDepthLevel;
        public IntSetWithData(ISet<uint> IntSet, int LoopDepthLevel = 0, bool IsTemporal = true)
        {
            this.IntSet = IntSet;
            this.IsTemporal = IsTemporal;
            this.LoopDepthLevel = LoopDepthLevel;
        }
    }
}
