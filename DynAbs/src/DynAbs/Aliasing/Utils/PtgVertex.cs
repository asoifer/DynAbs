using System;
using System.Collections.Generic;
using System.Linq;
using StaticModeKey = System.String;

namespace DynAbs
{
    public class PtgVertex
    {
        public const string DefaultKind = "Default";

        #region Properties and constructors
        // Para los conjuntos utilizamos lazy initialization

        IDictionary<string, ISet<PtgVertex>> _CommonVertex;
        public IDictionary<string, ISet<PtgVertex>> CommonVertex
        {
            get
            {
                if (_CommonVertex == null)
                    _CommonVertex = new Dictionary<string, ISet<PtgVertex>>();
                return _CommonVertex;
            }
            set
            {
                _CommonVertex = value;
            }
        }

        ISet<PtgVertex> _SigmaVertex;
        public ISet<PtgVertex> SigmaVertex
        {
            get
            {
                if (_SigmaVertex == null)
                    _SigmaVertex = SolverUtils.CreateReferenceComparedPTGHashSet();
                return _SigmaVertex;
            }
            set
            {
                _SigmaVertex = value;
            }
        }

        ISet<PtgVertex> _LambdaVertex;
        public ISet<PtgVertex> LambdaVertex
        {
            get
            {
                if (_LambdaVertex == null)
                    _LambdaVertex = SolverUtils.CreateReferenceComparedPTGHashSet();
                return _LambdaVertex;
            }
            set
            {
                _LambdaVertex = value;
            }
        }

        ISet<PtgWrapper> _SigmaVertexToYou;
        public ISet<PtgWrapper> SigmaVertexToYou
        {
            get
            {
                if (_SigmaVertexToYou == null)
                    _SigmaVertexToYou = new HashSet<PtgWrapper>();
                return _SigmaVertexToYou;
            }
            set
            {
                _SigmaVertexToYou = value;
            }
        }

        ISet<PtgWrapper> _LambdaVertexToYou;
        public ISet<PtgWrapper> LambdaVertexToYou
        {
            get
            {
                if (_LambdaVertexToYou == null)
                    _LambdaVertexToYou = new HashSet<PtgWrapper>();
                return _LambdaVertexToYou;
            }
            set
            {
                _LambdaVertexToYou = value;
            }
        }

        IDictionary<string, ISet<PtgWrapper>> _CommonVertexToYou;
        public IDictionary<string, ISet<PtgWrapper>> CommonVertexToYou
        {
            get
            {
                if (_CommonVertexToYou == null)
                    _CommonVertexToYou = new Dictionary<string, ISet<PtgWrapper>>();
                return _CommonVertexToYou;
            }
            set
            {
                _CommonVertexToYou = value;
            }
        }

        ISet<Field> _SigmaExcludedProperties;
        public ISet<Field> SigmaExcludedProperties
        {
            get
            {
                if (_SigmaExcludedProperties == null)
                    _SigmaExcludedProperties = new HashSet<Field>();
                return _SigmaExcludedProperties;
            }
            set
            {
                _SigmaExcludedProperties = value;
            }
        }

        ISet<StaticModeKey> _StaticKey;
        public ISet<StaticModeKey> StaticKey
        {
            get
            {
                if (_StaticKey == null)
                    _StaticKey = new HashSet<StaticModeKey>();
                return _StaticKey;
            }
            internal set
            {
                _StaticKey = value;
            }
        }

        public IEnumerable<PtgVertex> AllVertex()
        {
            foreach (var t in LambdaVertex)
                yield return t;
            foreach (var t in CommonVertex.SelectMany(x => x.Value))
                yield return t;
            foreach (var t in SigmaVertex)
                yield return t;
        }

        public IDictionary<Field, ISet<uint>> FieldsLastDef { get; set; }
        public VertexType VertexType { get; internal set; }
        public string Description { get; set; } // For debugging
        public bool IsMinType { get; internal set; }
        public bool Immutable { get; internal set; }
        public bool Multiple { get; set; }
        public int LoopDepthLevel { get; internal set; }
        public ISlicerSymbol Type { get; internal set; }
        public string Kind { get; internal set; }

        public static int LastID = 0;
        public int ID = ++LastID;

        Tuple<ScopeContainer, ISet<string>> _GlobalScopeToYou;
        public Tuple<ScopeContainer, ISet<string>> GlobalScopeToYou
        {
            get { return _GlobalScopeToYou; }
            set { _GlobalScopeToYou = value; }
        }

        IDictionary<ScopeWrapper, ISet<string>> _ScopesContainersToYou;
        public IDictionary<ScopeWrapper, ISet<string>> ScopesContainersToYou
        {
            get
            {
                if (_ScopesContainersToYou == null)
                    _ScopesContainersToYou = new Dictionary<ScopeWrapper, ISet<string>>();
                return _ScopesContainersToYou;
            }
            set { _ScopesContainersToYou = value; }
        }

        UserSliceConfiguration Configuration;

        public PtgVertex(UserSliceConfiguration userSliceConfiguration, string description, bool isMinType, ISlicerSymbol initialType, bool immutable, VertexType vertexType = VertexType.Common, string kind = DefaultKind, bool multiple = false, int loopDepthLevel = 0, string staticKey = "")
        {
            Configuration = userSliceConfiguration;
            Description = description;
            FieldsLastDef = new Dictionary<Field, ISet<uint>>();
            VertexType = vertexType;
            Type = initialType;
            IsMinType = isMinType;
            Immutable = immutable;
            Kind = kind;
            Multiple = multiple;
            StaticKey.Add(staticKey);
            LoopDepthLevel = loopDepthLevel;
            Parent = this;
        }
        #endregion

        #region Union Find Set
        // Properties:
        public PtgVertex Parent { get; set; }
        public int Rank = 0;

        public PtgVertex Find()
        {
            if (this != Parent && Parent.Parent != Parent)
                Parent = Parent.Find(); //path compression
            return this.Parent;
        }

        public void Union(PtgVertex other, ISet<PtgVertex> excludeEdges)
        {
            PtgVertex child, parent = null;
            if (Globals.optimize_union_find_set) 
            {
                child = other.Find();
                parent = this.Find();
            }
            else 
            {
                // Traditional algorithm
                var repThis = this.Find(); //finding representative of x
                var repOther = other.Find(); //finding representative of y

                // Si ambos tienen mismo ranking elijo al this como padre
                if (repThis.Rank == repOther.Rank)
                {
                    repThis.Rank++; // Incrementing rank of y
                    child = repOther;
                    parent = repThis;
                }
                // Making parent which is having higher rank
                else if (repThis.Rank > repOther.Rank)
                {
                    child = repOther;
                    parent = repThis;
                }
                else
                {
                    child = repThis;
                    parent = repOther;
                }

                child.Parent = parent;
            }

            child.Parent = parent;

            if (!Globals.optimize_union_find_set)
            {
                parent.Multiple = true;
                if (Configuration.TypesOptimization)
                {
                    // LastDef:
                    if (!Globals.clean_last_def)
                        foreach (var dicEntry in child.FieldsLastDef)
                            if (parent.FieldsLastDef.ContainsKey(dicEntry.Key))
                                parent.FieldsLastDef[dicEntry.Key].UnionWith(dicEntry.Value);
                            else
                                parent.FieldsLastDef.Add(dicEntry.Key, dicEntry.Value);

                    // Out edges:
                    foreach (var edge in child.LambdaVertex.Where(x => !excludeEdges.Contains(x)))
                        parent.AddVertex(EdgeType.Lambda, edge);
                    foreach (var edge in child.CommonVertex)
                    {
                        var edgesTo = SolverUtils.CreateReferenceComparedPTGHashSet(edge.Value.Except(excludeEdges));
                        if (edgesTo.Count > 0)
                            parent.AddVertex(edge.Key, edgesTo);
                    }
                    foreach (var edge in child.SigmaVertex.Where(x => x != child && !excludeEdges.Contains(x)))
                        parent.AddVertex(EdgeType.Sigma, edge);
                    if (child.SigmaVertex.Contains(child))
                        parent.AddVertex(EdgeType.Sigma, parent);
                }
            }
        }
        #endregion

        #region Operators
        public static bool operator ==(PtgVertex a, PtgVertex b)
        {
            if (System.Object.ReferenceEquals(a, b))
                return true;

            // If one is null, but not both, return false.
            if (((object)a == null) || ((object)b == null))
                return false;

            return a.ID == b.ID;
        }

        public static bool operator !=(PtgVertex a, PtgVertex b)
        {
            return !(a == b);
        }

        public override bool Equals(object obj)
        {
            PtgVertex other = (PtgVertex)obj;
            return this.ID.Equals(other.ID);
        }

        public override int GetHashCode()
        {
            return System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(this);
        }

        public override string ToString()
        {
            return Description;
        }
        #endregion

        #region Methods
        public bool AddVertex(EdgeType edgeType, PtgVertex vertex, string fieldName = null)
        {
            var added = false;
            switch (edgeType)
            {
                case EdgeType.Common:
                    if (this.VertexType != VertexType.Hub || this != vertex)
                    {
                        if (!CommonVertex.ContainsKey(fieldName))
                            CommonVertex[fieldName] = SolverUtils.CreateReferenceComparedPTGHashSet();
                        added = CommonVertex[fieldName].Add(vertex);

                        if (!vertex.CommonVertexToYou.ContainsKey(fieldName))
                            vertex.CommonVertexToYou[fieldName] = new HashSet<PtgWrapper>();
                        vertex.CommonVertexToYou[fieldName].Add(new PtgWrapper(this));
                    }
                    break;
                case EdgeType.Sigma:
                    added = SigmaVertex.Add(vertex);
                    if (this != vertex)
                        vertex.SigmaVertexToYou.Add(new PtgWrapper(this));

                    // Si se agrega un sigma entre 2 vértices no hace falta tener todo el resto...
                    if (this.CommonVertex.Count > 1)
                    {
                        foreach (var cv in this.CommonVertex)
                            foreach (var v in cv.Value)
                                v.CommonVertexToYou[cv.Key].Remove(v.CommonVertexToYou[cv.Key].First(x => x.Vertex.IsAlive && ((PtgVertex)x.Vertex.Target) == this));
                        this.CommonVertex = new Dictionary<string, ISet<PtgVertex>>();
                    }

                    break;
                case EdgeType.Lambda:
                    if (this != vertex)
                    {
                        added = LambdaVertex.Add(vertex);
                        vertex.LambdaVertexToYou.Add(new PtgWrapper(this));
                    }
                    break;
            }
            return added;
        }

        public void AddVertex(string key, ISet<PtgVertex> value)
        {
            if (this.VertexType == VertexType.Hub)
                value.Remove(this);

            if (!CommonVertex.ContainsKey(key))
                CommonVertex[key] = value;
            else
                CommonVertex[key].UnionWith(value);

            foreach (var v in value)
            {
                if (!v.CommonVertexToYou.ContainsKey(key))
                    v.CommonVertexToYou[key] = new HashSet<PtgWrapper>();
                v.CommonVertexToYou[key].Add(new PtgWrapper(this));
            }
        }

        /// <summary>
        /// Solo utilizado en el LambdaCompression. No elimina la referencia DESDE (está iterando y no puede modificar la colección)
        /// </summary>
        public void RemoveVertexLambdaCompressionA(EdgeType edgeType, PtgVertex vertex, string fieldName = null)
        {
            switch (edgeType)
            {
                case EdgeType.Common:
                    //CommonVertex[fieldName].Remove(vertex);
                    vertex.CommonVertexToYou[fieldName].Remove(vertex.CommonVertexToYou[fieldName].First(x => x.Vertex.IsAlive && ((PtgVertex)x.Vertex.Target) == this));
                    break;
                case EdgeType.Sigma:
                    //SigmaVertex.Remove(vertex);
                    vertex.SigmaVertexToYou.Remove(vertex.SigmaVertexToYou.FirstOrDefault(x => x.Vertex.IsAlive && ((PtgVertex)x.Vertex.Target) == this));
                    break;
                case EdgeType.Lambda:
                    //LambdaVertex.Remove(vertex);
                    vertex.LambdaVertexToYou.Remove(vertex.LambdaVertexToYou.FirstOrDefault(x => x.Vertex.IsAlive && ((PtgVertex)x.Vertex.Target) == this));
                    break;
            }
        }

        /// <summary>
        /// Solo utilizado en el LambdaCompression. No elimina la referencia DESDE (está iterando y no puede modificar la colección)
        /// </summary>
        public void RemoveVertexLambdaCompressionA(string key, ISet<PtgVertex> value)
        {
            foreach (var v in value)
                v.CommonVertexToYou[key].Remove(v.CommonVertexToYou[key].First(x => x.Vertex.IsAlive && ((PtgVertex)x.Vertex.Target) == this));
        }

        /// <summary>
        /// Solo utilizado en el LambdaCompression. No elimina la referencia HASTA (está iterando y no puede modificar la colección)
        /// </summary>
        public void RemoveVertexLambdaCompressionB(EdgeType edgeType, PtgVertex vertex, string fieldName = null)
        {
            switch (edgeType)
            {
                case EdgeType.Common:
                    CommonVertex[fieldName].Remove(vertex);
                    //vertex.CommonVertexToYou[fieldName].Remove(vertex.CommonVertexToYou[fieldName].First(x => x.Vertex.IsAlive && ((PtgVertex)x.Vertex.Target) == this));
                    break;
                case EdgeType.Sigma:
                    SigmaVertex.Remove(vertex);
                    //vertex.SigmaVertexToYou.Remove(vertex.SigmaVertexToYou.FirstOrDefault(x => x.Vertex.IsAlive && ((PtgVertex)x.Vertex.Target) == this));
                    break;
                case EdgeType.Lambda:
                    LambdaVertex.Remove(vertex);
                    //vertex.LambdaVertexToYou.Remove(vertex.LambdaVertexToYou.FirstOrDefault(x => x.Vertex.IsAlive && ((PtgVertex)x.Vertex.Target) == this));
                    break;
            }
        }

        public void RemoveVertex(string fieldName)
        {
            if (CommonVertex.ContainsKey(fieldName))
            {
                foreach (var v in CommonVertex[fieldName])
                    v.CommonVertexToYou[fieldName].Remove(v.CommonVertexToYou[fieldName].First(x => x.Vertex.IsAlive && ((PtgVertex)x.Vertex.Target) == this));
                CommonVertex[fieldName] = SolverUtils.CreateReferenceComparedPTGHashSet();
            }
        }

        /// <summary>
        /// Limpiamos todo. 
        /// </summary>
        public void ClearVertex()
        {
            CommonVertex = null;
            SigmaVertex = null;
            LambdaVertex = null;
            CommonVertexToYou = null;
            SigmaVertexToYou = null;
            LambdaVertexToYou = null;
            ScopesContainersToYou = null;
            GlobalScopeToYou = null;
        }

        // Se asume que siempre te puede agregar el último scope. No los anteriores
        // Luego, si no está la entrada se crea
        public void AddEntryPoint(ScopeContainer scopeContainer, string key)
        {
            if (scopeContainer.IsGlobalScope)
            {
                if (GlobalScopeToYou == null)
                    GlobalScopeToYou = new Tuple<ScopeContainer, ISet<string>>(scopeContainer, new HashSet<string>(new string[] { key }));
                else
                    GlobalScopeToYou.Item2.Add(key);
                return;
            }

            var tempScopeWrapper = new ScopeWrapper(scopeContainer);
            if (!ScopesContainersToYou.ContainsKey(tempScopeWrapper))
                ScopesContainersToYou[tempScopeWrapper] = new HashSet<string>();
            ScopesContainersToYou[tempScopeWrapper].Add(key);
        }

        public void RemoveEntryPoint(ScopeContainer scopeContainer, string key)
        {
            if (scopeContainer.IsGlobalScope)
            {
                if (GlobalScopeToYou != null)
                    GlobalScopeToYou.Item2.Remove(key);
                return;
            }

            var tempScopeWrapper = new ScopeWrapper(scopeContainer);
            if (ScopesContainersToYou.ContainsKey(tempScopeWrapper))
                ScopesContainersToYou[tempScopeWrapper].Remove(key);
        }

        public void RemoveEntryPointStack(ScopeContainer scopeContainer)
        {
            if (!scopeContainer.IsGlobalScope)
                ScopesContainersToYou.Remove(new ScopeWrapper(scopeContainer));
        }
        #endregion

        #region Others
        public PtgVertex(PtgVertex other, int LoopDepthLevel, int StackCounter)
        {
            if (other._CommonVertex != null)
                foreach (var t in other._CommonVertex)
                    this.AddVertex(t.Key, t.Value);

            if (other._LambdaVertex != null)
                foreach (var t in other._LambdaVertex)
                    this.AddVertex(EdgeType.Lambda, t);

            if (other._SigmaVertex != null)
                foreach (var t in other._SigmaVertex)
                    this.AddVertex(EdgeType.Sigma, t);

            if (this._CommonVertexToYou != null)
                foreach (var t in other._CommonVertexToYou)
                    foreach (var t2 in t.Value)
                        ((PtgVertex)t2.Vertex.Target).AddVertex(EdgeType.Common, this, t.Key);

            if (this._LambdaVertexToYou != null)
                foreach (var t in other._LambdaVertexToYou)
                    ((PtgVertex)t.Vertex.Target).AddVertex(EdgeType.Lambda, this);

            if (this._SigmaVertexToYou != null)
                foreach (var t in other._SigmaVertexToYou)
                    ((PtgVertex)t.Vertex.Target).AddVertex(EdgeType.Sigma, this);

            if (this._SigmaExcludedProperties != null)
                foreach (var t in other._SigmaExcludedProperties)
                    this.SigmaExcludedProperties.Add(t);

            this.Description = Guid.NewGuid().ToString();
            if (other.FieldsLastDef != null)
            {
                this.FieldsLastDef = new Dictionary<Field, ISet<uint>>();
                foreach (var field in other.FieldsLastDef)
                {
                    Field copiedField = new Field(field.Key);
                    var copiedSet = new HashSet<uint>(field.Value);
                    this.FieldsLastDef.Add(copiedField, copiedSet);
                }
            }
            this.Type = other.Type;
            this.VertexType = other.VertexType;
            this.IsMinType = other.IsMinType;
            this.Kind = other.Kind;
            this.Immutable = other.Immutable;
            this.Multiple = other.Multiple;
            this.LoopDepthLevel = LoopDepthLevel;
            this.Parent = this;
        }

        public int CountVertexToYou()
        {
            return LambdaVertexToYou.Count + CommonVertexToYou.Count + SigmaVertexToYou.Count;
        }
        public int CountVertex()
        {
            return LambdaVertex.Count + CommonVertex.Count + SigmaVertex.Count;
        }
        public ISet<PtgWrapper> AllVertexToYou()
        {
            var result = new HashSet<PtgWrapper>();
            result.UnionWith(LambdaVertexToYou);
            result.UnionWith(CommonVertexToYou.SelectMany(x => x.Value));
            result.UnionWith(SigmaVertexToYou);
            return result;
        }

        public void CheckConsistency()
        {
            var wrappedThis = new PtgWrapper(this);
            foreach (var v in LambdaVertex)
                if (!v.LambdaVertexToYou.Contains(wrappedThis))
                    throw new SlicerException("");
            foreach (var v in SigmaVertex)
                if (v != this && !v.SigmaVertexToYou.Contains(wrappedThis))
                    throw new SlicerException("");
            foreach (var v in CommonVertex)
                foreach (var k in v.Value)
                    if (!k.CommonVertexToYou[v.Key].Contains(wrappedThis))
                        throw new SlicerException("");
        }

        ~PtgVertex()
        {
            //Console.WriteLine("Se eliminó un vértice");
        }
        #endregion
    }
}
