using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using DynAbs.Summaries;
using Microsoft.CodeAnalysis;
using QuikGraph;
using QuikGraph.Graphviz;
using StaticModeKey = System.String;

namespace DynAbs
{
    public abstract class AbstractSolver
    {
        #region Properties
        #region Basic
        public UserSliceConfiguration Configuration;
        public ScopeContainer globalScope = new ScopeContainer(true);
        public Stack<ScopeContainer> scopeStack = new Stack<ScopeContainer>();
        public static readonly Field SIGMA_FIELD = Field.SigmaField();
        // Se utiliza para crear nombres distintos para cada HUB
        public int HubUniqueId = 0;
        // Solo está para que el DumpPtg no pinche al final de la ejecución cuando ya la pila de scopes esté vacía
        public ScopeContainer lastPopedScope;
        #endregion

        #region Static Mode
        public ScopesGraph scopeGraph = new ScopesGraph();

        public bool StaticModeEnabledByCallbackOrRecursion = false;
        public CustomStack<ISet<PtgVertex>> StaticModeStack = new CustomStack<ISet<PtgVertex>>();
        public Dictionary<StaticModeKey, PtgVertex> StaticModeVertex = new Dictionary<StaticModeKey, PtgVertex>();
        public Dictionary<StaticModeKey, PtgVertex> StaticModeHUB = new Dictionary<StaticModeKey, PtgVertex>();

        public Dictionary<StaticModeKey, PtgVertex> StaticModeCallbackRegion = new Dictionary<StaticModeKey, PtgVertex>();
        public Dictionary<StaticModeKey, PtgVertex> StaticModeCallbackVertex = new Dictionary<StaticModeKey, PtgVertex>();
        public bool StaticMode { get { return StaticModeStack.Count > 0; } }
        #endregion

        #region Optimizations
        // Loops
        public Stack<bool> LoopStack = new Stack<bool>();
        public bool AllInFalse = false;
        #endregion
        #endregion

        #region Public
        /// <summary>
        /// Creates a new container and binds the parameters of the previous scope
        /// Actual param: are the variables of the previous scope.
        /// Formal param: are the parameters of the current function.
        /// </summary>
        public void EnterMethodAndBind(string methodSymbol, List<Term> actualParams, List<Term> formalParams, Term receiver, Term @this)
        {
            ScopeContainer previousScope = null;
            ScopeContainer curScope = null;
            if (scopeStack.Count > 0)
                previousScope = CurrentScope();

            curScope = EnterMethod();
            curScope.Arguments = actualParams;
            curScope.Parameters = formalParams;

            if (Configuration.StaticModeEnabled)
            {
                bool existingMethod;
                curScope.BaseKey = scopeGraph.PushMethod(methodSymbol, out existingMethod);

                // If we're repeating the method and we're not in static mode, we start the static mode
                if (existingMethod)
                    curScope.IsStaticModeStart = EnterStaticMode(true);
            }

            // Bind each non scalar parameter from the previous scope
            for (var i = 0; i < formalParams.Count; i++)
                if (actualParams.Count > i && (actualParams[i] != null) && (!actualParams[i].IsScalar) && (!formalParams[i].IsScalar))
                {
                    var vertices = aPt(actualParams[i].IsGlobal ? globalScope : previousScope, actualParams[i]);
                    // XXX: Si es struct, se hace la copia en el PTG
                    if (formalParams[i].IsStruct)
                        if (!StaticMode)
                            vertices = StructCopy(vertices);
                        else
                            foreach (var c in vertices)
                                c.Multiple = true;
                    Add(formalParams[i], vertices);
                }

            // We do the same as the explicit non-scalar parameters with the receiver (an implicit parameter)
            if (receiver != null)
                Add(@this, aPt(receiver.IsGlobal ? globalScope : previousScope, receiver));
        }

        /// <summary>
        /// Removes the current scope, and binds the return term to the term of the scope that invokes it
        /// </summary>
        public ISet<uint> ExitMethodAndUnbind(Term term, Term returnExceptionTerm, Term exceptionTerm)
        {
            if (Configuration.StaticModeEnabled)
                scopeGraph.PopMethod();

            var lastScope = scopeStack.Pop();
            lastScope.RemoveEntryPointsToYou();
            // Lo declaramos muerto para que si un nodo es alcanzado por este scope no crea que está vivo
            lastScope.Alive = false;

            // XXX: Esta línea es para guardarnos el último scope, si no hacemos esto al final el DumpPtg pincha
            lastPopedScope = lastScope;

            for (var i = 0; i < lastScope.Arguments.Count; i++)
            {
                var parameter = lastScope.Parameters[i];
                var argument = lastScope.Arguments[i].ReferencedTerm; // Usamos el original
                if (parameter.IsOutOrRef)
                {
                    Assign(argument, ScopeForTerm(argument), parameter, lastScope);
                    LastDef_Set(argument, LastDef_Get(parameter, lastScope));
                }
            }

            // Si no hay return value o no tenemos ningún término a quien devolver algo, retornamos
            ISet<uint> returnValue = null;
            if (term != null && lastScope.ReturnValue != null)
            {
                var returnValueScope = lastScope.ReturnValue.IsGlobal ? globalScope : lastScope;

                if (!term.IsScalar && !lastScope.ReturnValue.IsScalar)
                    Assign(term, ScopeForTerm(term), lastScope.ReturnValue, returnValueScope);

                returnValue = LastDef_Get(lastScope.ReturnValue, returnValueScope);
            }

            // TODO: exception term

            // Si estamos removiendo el scope que dio inicio al modo estático lo liberamos
            if (Configuration.StaticModeEnabled && lastScope.IsStaticModeStart)
                ExitStaticMode();

            return returnValue;
        }

        /// <summary>
        /// Stores the last definition of a term
        /// </summary>
        public void LastDef_Set(Term term, uint lastDef)
        {
            LastDef_Set(term, new HashSet<uint>() { lastDef });
        }

        public void LastDef_Set(Term term, ISet<uint> lastDef, bool weak = false)
        {
            // 1. Obtenemos el scope
            var scope = ScopeForTerm(term);

            // 2. CASO 1: El término es una variable
            if (term.IsVar)
            {
                // 2.A. Agregamos el nuevo vértice
                if (scope.LastDefDict.ContainsKey(term.First))
                {
                    // Mergeamos ambos, contempla el if siguiente y el bit de modificación
                    if (weak)
                        lastDef.UnionWith(scope.LastDefDict[term.First].IntSet);

                    if (CheckEqualsSet &&
                        !scope.LastDefDict[term.First].IsTemporal &&
                        scope.LastDefDict[term.First].LoopDepthLevel < StaticModeStack.Count &&
                        !scope.LastDefDict[term.First].IntSet.SetEquals(lastDef))
                        SetNotConverged(scope.LastDefDict[term.First].LoopDepthLevel);
                    scope.LastDefDict[term.First].IntSet = lastDef;
                }
                else
                    scope.LastDefDict[term.First] = new IntSetWithData(lastDef, StaticModeStack.Count, term.IsTemporal);

                return;
            }

            // 2. CASO 2: Cuando el término es complejo
            // Obtenemos los vértices que podrían apuntar a nuestro término
            var vertices = aPt(scope, term.DiscardLast());
            if (vertices == null && term.IsGlobal)
                throw new UninitializedTerm(term.DiscardLast());

            // 3. Soft/Hard Update dependiendo si hay un único nodo no hub o no
            if (vertices.Count == 1 && vertices.Single().VertexType != VertexType.Hub && term.Last.FieldType != FieldType.Sigma && !vertices.Single().Multiple && !weak)
            {
                if (CheckEqualsSet &&
                    (!StaticMode || StaticModeEnabledByCallbackOrRecursion || !StaticModeStack.Peek().Contains(vertices.Single())) &&
                    (!vertices.Single().FieldsLastDef.ContainsKey(term.Last) ||
                    !vertices.Single().FieldsLastDef[term.Last].SetEquals(lastDef)))
                    SetNotConverged(vertices.Single().LoopDepthLevel);

                vertices.Single().FieldsLastDef[term.Last] = lastDef;
                vertices.Single().SigmaExcludedProperties.Add(term.Last);
            }
            else
            {
                foreach (var vertex in vertices.Where(x => !x.Immutable))
                {
                    if (!vertex.FieldsLastDef.ContainsKey(term.Last))
                        vertex.FieldsLastDef[term.Last] = new HashSet<uint>();

                    if (CheckEqualsSet &&
                        (!StaticMode || StaticModeEnabledByCallbackOrRecursion || !StaticModeStack.Peek().Contains(vertex)) &&
                        !lastDef.IsSubsetOf(vertex.FieldsLastDef[term.Last]))
                        SetNotConverged(vertex.LoopDepthLevel);
                    vertex.FieldsLastDef[term.Last].UnionWith(lastDef);
                }
            }
        }

        /// <summary>
        /// Returns the last DG vertex where the term may have been defined
        /// </summary>
        public ISet<uint> LastDef_Get(Term term)
        {
            return LastDef_Get(term, ScopeForTerm(term));
        }

        public ISet<uint> LastDef_Get(Term term, ScopeContainer scope)
        {
            if (term.IsGlobal && term.IsVar && !scope.LastDefDict.ContainsKey(term.First))
                throw new UninitializedTerm(term);

            // CASO 1: si el término es una variable retornamos su entrada en el diccionario
            if (term.IsVar)
                return new HashSet<uint>(scope.LastDefDict[term.First].IntSet);

            // CASO 2: caso contrario obtenemos los vértices que lo alcanzan con aPt
            var collectedNodes = new HashSet<uint>();

            var vertices = aPt(scope, term.DiscardLast(), Configuration.IncludeAllUses ? collectedNodes : null);

            // Por cada vértice que lo podría apuntar:
            if (vertices == null && term.IsGlobal)
                throw new UninitializedTerm(term.DiscardLast());

            foreach (var vertex in vertices)
            {
                if (term.Last == SIGMA_FIELD)
                {
                    foreach (var key in vertex.FieldsLastDef.Keys)
                        foreach (var v in vertex.FieldsLastDef[key])
                            collectedNodes.Add(v);
                }
                else
                {
                    // Si el vértice contiene el field, obtenemos sus last def, además nos movemos por SIGMA si este vértice no está excluido
                    if (vertex.FieldsLastDef.ContainsKey(term.Last) && term.Last != SIGMA_FIELD)
                        foreach (var t in vertex.FieldsLastDef[term.Last])
                            collectedNodes.Add(t);
                    if (!vertex.SigmaExcludedProperties.Contains(term.Last) && vertex.FieldsLastDef.ContainsKey(SIGMA_FIELD))
                        foreach (var t in vertex.FieldsLastDef[SIGMA_FIELD])
                            collectedNodes.Add(t);
                }
            }
            return collectedNodes;
        }

        /// <summary>
        /// Add a new node to the graph
        /// </summary>
        public void Alloc(Term term, bool @override = true, string kind = null)
        {
            kind = kind ?? PtgVertex.DefaultKind;
            var scope = ScopeForTerm(term);
            StaticModeKey key = "";
            PtgVertex vertex = null;
            if (StaticMode)
            {
                if (term.IsCallbackAlloc)
                {
                    key = GetTermKey(term.Stmt, term.Parts.Last().Symbol);
                    MaySetVertexValue(ref vertex, StaticModeCallbackVertex, key);
                }
                else
                {
                    key = GetTermKey(term.Stmt);
                    MaySetVertexValue(ref vertex, StaticModeVertex, key);
                }
            }

            // Si no se obtuvo por el diccionario o no estamos en modo estático se crea el nodo para luego agregar al grafo
            if (vertex == null)
            {
                vertex = new PtgVertex(Configuration, term.Last.ToString(), true, term.Last.Symbol, false, VertexType.Common, kind, StaticMode && term.IsCallbackAlloc, StaticModeStack.Count, key);

                // Si se llegó hasta acá y estamos en modo estático es que no estaba la clave
                if (StaticMode)
                {
                    if (term.IsCallbackAlloc)
                        StaticModeCallbackVertex[key] = vertex;
                    else
                        StaticModeVertex[key] = vertex;
                    if (StaticModeStack.Count > 0)
                        StaticModeStack.Peek().Add(vertex);
                }

                // Con nodos nuevos ya el PTG está siendo modificado (por ahora)
                if (CheckEqualsSet && (!StaticMode || StaticModeEnabledByCallbackOrRecursion || !StaticModeStack.Peek().Contains(vertex)))
                    SetNotConverged(vertex.LoopDepthLevel);
            }

            // Cuando el término tiene 1 elemento
            if (term.IsVar)
            {
                if (@override || (!scope.EntryPointVerticesDict.ContainsKey(term.Last.ToString())))
                    scope.OverrideEntryPointValue(term.Last.ToString(), vertex, term.Last.Symbol);
                return;
            }

            // Muy similar al asign
            // El término izquierdo es complejo, primero obtenemos los vértices que podrían apuntar a nuestro nuevo vértice
            var lhsVertices = aPt(scope, term.DiscardLast());

            if (lhsVertices == null && term.IsGlobal)
                throw new UninitializedTerm(term.DiscardLast());

            if (@override || !(lhsVertices.Any(x => x.CommonVertex.ContainsKey(term.Last.Value))))
                foreach (var lhsVertex in lhsVertices)
                {
                    lhsVertex.RemoveVertex(term.Last.ToString());
                    lhsVertex.AddVertex(EdgeType.Common, vertex, term.Last.ToString());
                }
        }

        /// <summary>
        /// Assigns Right to Left implies that those who point to Left must now point to Right
        /// </summary>
        public void Assign(Term lhsTerm, Term rhsTerm)
        {
            var leftScope = ScopeForTerm(lhsTerm);
            var rightScope = ScopeForTerm(rhsTerm);
            Assign(lhsTerm, leftScope, rhsTerm, rightScope);
        }

        public void Assign(Term lhsTerm, ScopeContainer lhsScope, Term rhsTerm, ScopeContainer rhsScope)
        {
            var currentSet = aPt(rhsScope, rhsTerm, null) ?? SolverUtils.CreateReferenceComparedPTGHashSet();
            Assign(lhsTerm, lhsScope, currentSet);
        }

        public void Assign(Term lhsTerm, ScopeContainer lhsScope, ISet<PtgVertex> rightSet)
        {
            // We copy the structs. But if we're in static mode, we will use the same nodes.
            if (lhsTerm.Last.Symbol.IsStruct)
                if (!StaticMode)
                    rightSet = StructCopy(rightSet);
                else
                    foreach (var c in rightSet)
                        c.Multiple = true;

            if (lhsTerm.IsVar)
            {
                var filteredRightSet = Configuration.TypesOptimization ?
                    SolverUtils.CreateReferenceComparedPTGHashSet(rightSet.Where(x => Compatibles(x, lhsTerm.First.Symbol))) : rightSet;
                if (CheckEqualsSet &&
                    lhsScope.LastDefDict.ContainsKey(lhsTerm.First) &&
                    !lhsScope.LastDefDict[lhsTerm.First].IsTemporal &&
                    lhsScope.LastDefDict[lhsTerm.First].LoopDepthLevel < StaticModeStack.Count &&
                    (!lhsScope.EntryPointVerticesDict.ContainsKey(lhsTerm.ToString()) ||
                    lhsScope.EntryPointVerticesDict[lhsTerm.ToString()] == null ||
                    !lhsScope.EntryPointVerticesDict[lhsTerm.ToString()].VertexSet.SetEquals(filteredRightSet)))
                    SetNotConverged(lhsScope.LastDefDict[lhsTerm.First].LoopDepthLevel);

                lhsScope.OverrideEntryPointValues(lhsTerm.First.ToString(), filteredRightSet, lhsTerm.Last.Symbol);
                return;
            }

            var lhsVertices = aPt(lhsScope, lhsTerm.DiscardLast());

            // In strong updates we delete previous references
            var convergedChecked = false;
            if (lhsVertices.Count == 1 && lhsVertices.Single().VertexType != VertexType.Hub && lhsTerm.Last.FieldType != FieldType.Sigma && !lhsVertices.Single().Immutable && !lhsVertices.Single().Multiple)
            {
                if (CheckEqualsSet &&
                    (!StaticMode || StaticModeEnabledByCallbackOrRecursion || !StaticModeStack.Peek().Contains(lhsVertices.Single())) &&
                    lhsVertices.Single().CommonVertex.ContainsKey(lhsTerm.Last.ToString()) && lhsVertices.Single().CommonVertex[lhsTerm.Last.ToString()].Count > 0)
                    SetNotConverged(lhsVertices.Single().LoopDepthLevel);
                convergedChecked = true;
                lhsVertices.Single().RemoveVertex(lhsTerm.Last.ToString());
            }

            foreach (var lhsVertex in lhsVertices.Where(x => !x.Immutable))
                foreach (var rhsVertex in rightSet)
                {
                    var added = false;
                    if (lhsTerm.Last.FieldType == FieldType.Sigma)
                        added = lhsVertex.AddVertex(EdgeType.Sigma, rhsVertex);
                    else
                        added = lhsVertex.AddVertex(EdgeType.Common, rhsVertex, lhsTerm.Last.ToString());
                    if (CheckEqualsSet && (!StaticMode || StaticModeEnabledByCallbackOrRecursion || !StaticModeStack.Peek().Contains(rhsVertex)) && !convergedChecked && added)
                        SetNotConverged(lhsVertex.LoopDepthLevel);
                }
        }

        public void RedefineType(Term term)
        {
            var vertexSet = aPt(term, null);
            if (vertexSet.Count == 1 &&
                vertexSet.Single().VertexType != VertexType.Hub &&
                !vertexSet.Single().IsMinType &&
                TypesUtils.GetMin(vertexSet.Single().Type, term.Last.Symbol) == term.Last.Symbol)
                vertexSet.Single().Type = term.Last.Symbol;

            if (term.IsVar)
                ScopeForTerm(term).EntryPointVerticesDict[term.ToString()].VertexType = TypesUtils.GetMin(ScopeForTerm(term).EntryPointVerticesDict[term.ToString()].VertexType, term.Last.Symbol);
        }

        /// <summary>
        /// Assigns the term to the ReturnValue of the current scope
        /// Space and time complexity: O(1)
        /// </summary>
        public void AssignRV(Term term)
        {
            CurrentScope().ReturnValue = term;
        }

        public void CleanTemporaryEntries()
        {
            var currentScope = CurrentScope();
            var keys = currentScope.LastDefDict.Where(x => x.Value.IsTemporal).Select(x => x.Key).ToList();
            foreach (var k in keys)
            {
                currentScope.DeleteEntryPointValue(k.ToString());
                currentScope.LastDefDict.Remove(k);
            }
        }

        /// <summary>
        /// Draws the PtG in the path param
        /// </summary>
        public void DumpPTG(string path = @"C:\temp\PointsTo.dot", string label = null, bool getGlobalScope = false, string key = null)
        {
            ScopeContainer currentScope;
            if (!getGlobalScope)
            {
                try
                {
                    currentScope = CurrentScope();
                }
                catch
                {
                    currentScope = lastPopedScope;
                }
            }
            else
                currentScope = globalScope;
            var graphCopy = new AdjacencyGraph<PtgVertex, PtgEdge>();

            var regexNombre = new System.Text.RegularExpressions.Regex(@"^[0-9]*/[0-9]*\-[0-9]*$");
            var regexNumerico = new System.Text.RegularExpressions.Regex(@"^[0-9]*$");
            var conjunto = string.IsNullOrWhiteSpace(key) ?
                currentScope.EntryPointVerticesDict.Keys.Where(x => currentScope.EntryPointVerticesDict[x] != null &&
                !regexNumerico.IsMatch(x.ToString())).ToList() : new List<string>() { key };
            foreach (var k in conjunto)
            {
                if (currentScope.EntryPointVerticesDict[k] == null)
                    continue;

                var descripcion = k.ToString();
                if (regexNombre.IsMatch(descripcion))
                    descripcion = TermFactory.GetFileTextBySyntaxNodeName(descripcion);

                var createdVertex = new PtgVertex(Configuration, descripcion, false, currentScope.EntryPointVerticesDict[k].VertexType, false, VertexType.EntryPoint);
                graphCopy.AddVertex(createdVertex);

                foreach (var vt in currentScope.EntryPointVerticesDict[k].VertexSet)
                {
                    graphCopy.AddVertex(vt);
                    graphCopy.AddEdge(new PtgEdge { Source = createdVertex, Target = vt, EdgeName = new Field("", null), EdgeType = EdgeType.Common });
                }
            }
            foreach (var k in conjunto)
            {
                if (currentScope.EntryPointVerticesDict[k] == null)
                    continue;
                foreach (var vt in currentScope.EntryPointVerticesDict[k].VertexSet)
                    AddToGraph(graphCopy, vt);
            }
            var algo = new GraphvizAlgorithm<PtgVertex, PtgEdge>(graphCopy);

            if (File.Exists(path)) File.Delete(path);

            File.WriteAllText(path, GraphUtils.GeneratePTG_DOT(algo, label));
            File.WriteAllText(path.Replace(".dot", ".dgml"), GraphUtils.GeneratePTG_DGML(algo, label));
        }

        #region Public StaticMode
        public bool EnterStaticMode(bool EnterByCallbackOrRecursion = false)
        {
            if (Configuration.StaticModeEnabled)
            {
                // Solo permitimos entrar si no estamos en uno de estos casos
                if (!StaticModeEnabledByCallbackOrRecursion)
                {
                    StaticModeEnabledByCallbackOrRecursion = EnterByCallbackOrRecursion;
                    StaticModeStack.Push(new HashSet<PtgVertex>());
                    return true;
                }
                return false;
            }
            return false;
        }

        public void ExitStaticMode()
        {
            if (Configuration.StaticModeEnabled)
            {
                // Si estábamos en callback lo liberamos (si estaba en true/false-->false)
                StaticModeEnabledByCallbackOrRecursion = false;

                // Limpiamos lo que queda
                // Para cada vértice agregado vemos si llega a uno con menor LoopDepth
                var LastStaticModeSet = StaticModeStack.Peek();
                foreach (var v in LastStaticModeSet)
                {
                    var minEscapeLevel = Escape(v, v.LoopDepthLevel);

                    // Si no escapó lo eliminamos de la entrada
                    if (!minEscapeLevel.HasValue)
                        ClearStaticStructures(v);
                    // Escapa a un nivel superior
                    else
                    {
                        // Escapa al root, no se debe chequear más y este pasa a nivel 0
                        v.LoopDepthLevel = minEscapeLevel.Value;
                        if (v.LoopDepthLevel > 0)
                            StaticModeStack.GetAt(v.LoopDepthLevel - 1).Add(v);
                    }
                }
                StaticModeStack.Pop();

                // Salimos de todo, limpiamos los diccionarios
                if (StaticModeStack.Count == 0)
                {
                    StaticModeVertex.Clear();
                    StaticModeHUB.Clear();
                    StaticModeCallbackRegion.Clear();
                    StaticModeCallbackVertex.Clear();
                }
            }
        }
        #endregion

        #region Public LoopOptimization
        public bool Converged()
        {
            if (Configuration.LoopsOptimizationEnabled && LoopStack.Count > 0)
                return LoopStack.Peek();
            return false;
        }

        public void EnterLoop()
        {
            if (Configuration.LoopsOptimizationEnabled)
            {
                LoopStack.Push(true);
                AllInFalse = false;
            }
        }

        public void NextLoopIteration()
        {
            if (Configuration.LoopsOptimizationEnabled)
            {
                LoopStack.Pop();
                LoopStack.Push(true);
                AllInFalse = false;
            }
        }

        public void ExitLoop()
        {
            if (Configuration.LoopsOptimizationEnabled)
                LoopStack.Pop();
        }
        #endregion
        #endregion

        #region Private
        #region Static mode functions
        public StaticModeKey GetTermKey(Stmt stmt)
        {
            return CurrentScope().BaseKey + stmt.GetHashCode();
        }

        public StaticModeKey GetTermKey(Stmt stmt, ISlicerSymbol type)
        {
            return CurrentScope().BaseKey + type.ToString() + ":" + stmt.GetHashCode();
        }

        /// <summary>
        /// If it escapes it tells you the level, 0 = ROOT
        /// </summary>
        public int? Escape(PtgVertex w, int LoopDepthValue)
        {
            bool CheckSameLevelScope = StaticModeEnabledByCallbackOrRecursion;
            int DefaultReturnValue = -1;

            // No solo se tiene que escapar de su scope sino también del rango de scopes
            // Es decir: Si alguno del scope 3 apunta a uno del 2 pero estamos popeando el 2 figuraría como escape cuando no lo es en realidad
            var toVisit = SolverUtils.CreateReferenceComparedPTGHashSet();
            var visited = new HashSet<PtgVertex>();

            // Si me apunta un scope de nivel superior ya está siendo alcanzado, pero queremos los scopes que están posta, otro pudo no ser recolectado por el GC
            foreach (var sc in w.ScopesContainersToYou.Where(x => x.Key.Scope.IsAlive))
            {
                var target = (ScopeContainer)sc.Key.Scope.Target;
                if (target == null || !target.Alive)
                    continue;
                foreach (var ep in sc.Value.Select(x => new Field(x, null)))
                    // El único motivo por el que no está definido es porque se llega acá en un ExitMethod que baja un nivel de recursión, en el caso mínimo baja un nivel
                    if (!target.LastDefDict.ContainsKey(ep))
                        return Math.Max(w.LoopDepthLevel - 1, 0);
                    else if (target.LastDefDict[ep].LoopDepthLevel < LoopDepthValue)
                        return target.LastDefDict[ep].LoopDepthLevel;
                    else if (CheckSameLevelScope)
                        return DefaultReturnValue;
            }
            // Si te apunta uno del global ya está... escapó al 0
            if (w.GlobalScopeToYou != null && w.GlobalScopeToYou.Item2.Count > 0)
                return 0;
            foreach (var t in w.LambdaVertexToYou.Where(x => x.Vertex.IsAlive).Select(x => (PtgVertex)x.Vertex.Target))
                if ((!Configuration.TypesOptimization) || t.VertexType == VertexType.Hub || Compatibles(w, t.Type))
                {
                    if (t.LoopDepthLevel < LoopDepthValue)
                        return t.LoopDepthLevel;
                    else
                        toVisit.Add(t);
                }
            foreach (var t in w.SigmaVertexToYou.Where(x => x.Vertex.IsAlive).Select(x => (PtgVertex)x.Vertex.Target))
            {
                if (t.LoopDepthLevel < LoopDepthValue)
                    return t.LoopDepthLevel;
                else
                    toVisit.Add(t);
            }
            foreach (var t in w.CommonVertexToYou.SelectMany(x => x.Value).Where(x => x.Vertex.IsAlive).Select(x => (PtgVertex)x.Vertex.Target))
            {
                if (t.LoopDepthLevel < LoopDepthValue)
                    return t.LoopDepthLevel;
                else
                    toVisit.Add(t);
            }
            
            while (toVisit.Count > 0)
            {
                var t = toVisit.First();
                toVisit.Remove(t);
                if (visited.Contains(t))
                    continue;

                visited.Add(t);
                foreach (var sc in t.ScopesContainersToYou.Where(x => x.Key.Scope.IsAlive))
                {
                    var target = (ScopeContainer)sc.Key.Scope.Target;
                    if (target == null || !target.Alive)
                        continue;
                    foreach (var ep in sc.Value.Select(x => new Field(x, null)))
                        if (!target.LastDefDict.ContainsKey(ep))
                            return Math.Max(w.LoopDepthLevel - 1, 0);
                        else if (target.LastDefDict[ep].LoopDepthLevel < LoopDepthValue)
                            return target.LastDefDict[ep].LoopDepthLevel;
                        else if (CheckSameLevelScope)
                            return DefaultReturnValue;
                }
                if (t.GlobalScopeToYou != null && t.GlobalScopeToYou.Item2.Count > 0)
                    return 0;

                foreach (var v in t.LambdaVertexToYou.Where(x => x.Vertex.IsAlive).Select(x => (PtgVertex)x.Vertex.Target))
                    if ((!Configuration.TypesOptimization) || t.VertexType == VertexType.Hub || Compatibles(v, t.Type))
                        if (v.LoopDepthLevel < LoopDepthValue)
                            return v.LoopDepthLevel;
                        else
                            toVisit.Add(t);
                foreach (var v in t.SigmaVertexToYou.Where(x => x.Vertex.IsAlive).Select(x => (PtgVertex)x.Vertex.Target))
                    if (v.LoopDepthLevel < LoopDepthValue)
                        return v.LoopDepthLevel;
                    else
                        toVisit.Add(t);
                foreach (var v in t.CommonVertexToYou.SelectMany(x => x.Value).Where(x => x.Vertex.IsAlive).Select(x => (PtgVertex)x.Vertex.Target))
                    if (v.LoopDepthLevel < LoopDepthValue)
                        return v.LoopDepthLevel;
                    else
                        toVisit.Add(t);
            }


            return null;
        }

        public void ClearStaticStructures(PtgVertex v)
        {
            if (v.VertexType == VertexType.Common)
            {
                foreach (var t in v.StaticKey)
                {
                    StaticModeVertex.Remove(t);
                    StaticModeCallbackVertex.Remove(t);
                }
            }
            else
            {
                foreach (var t in v.StaticKey)
                {
                    StaticModeHUB.Remove(t);
                    StaticModeCallbackRegion.Remove(t);
                }
            }
        }

        public void MaySetVertexValue(ref PtgVertex vertex, Dictionary<StaticModeKey, PtgVertex> dictionary, StaticModeKey key)
        {
            if (dictionary.ContainsKey(key))
            {
                var v = dictionary[key];
                var minEscapeLevel = Escape(v, StaticModeStack.Count);

                // Si no escapó, este nodo hay que limpiarlo :)
                if (!minEscapeLevel.HasValue)
                {
                    // Esto podría NO hacerlo ya que la entrada va a ser reemplazada
                    ClearStaticStructures(v);
                }
                // Si escapó antes y ahora se mantuvo, devolvemos ese, pero antes lo hacemos múltiple!
                else
                {
                    if (StaticModeStack.Peek().Contains(v) && minEscapeLevel >= 0)
                    {
                        StaticModeStack.Peek().Remove(v);
                        v.LoopDepthLevel = minEscapeLevel.Value;
                        if (minEscapeLevel.Value > 0)
                            StaticModeStack.GetAt(v.LoopDepthLevel - 1).Add(v);
                    }
                    v.Multiple = true;
                    vertex = v;
                }
            }
        }
        #endregion

        #region Loops optimization
        public bool CheckEqualsSet
        {
            get
            {
                return LoopStack.Count > 0 && !AllInFalse; // && LoopStack.Peek();
            }
        }

        public void SetNotConverged(int LoopDepthLevel)
        {
            var Levels = StaticModeStack.Count - LoopDepthLevel;
            // Cuando no converge un loop, no convergen los de nivel superior... 
            // TODO: Falso. porque la convergencia debe ser con respecto a lo observable desde afuera del loop...
            // Por ahora lo dejo así, lo charlamos próx reunión. 
            var i = 0;
            while (LoopStack.Count > 0 && LoopStack.Peek() && Levels > 0)
            {
                LoopStack.Pop();
                i++;
                Levels--;
            }
            int j;
            for (j = 0; j < i; j++)
                LoopStack.Push(false);
            if (j == LoopStack.Count)
                AllInFalse = true;
        }
        #endregion

        public abstract ISet<PtgVertex> aPt(ScopeContainer scope, Term term, ISet<uint> lastDefinitions = null);

        /// <summary>
        /// Overload of the aPt that does not receives the scope
        /// </summary>
        public ISet<PtgVertex> aPt(Term term, ISet<uint> lastDefinitions = null)
        {
            ScopeContainer scope = term.IsGlobal ? globalScope : CurrentScope();
            return aPt(scope, term, lastDefinitions);
        }

        /// <summary>
        /// Returns the set of vertices corresponding to a term
        /// If it is global and it is not found, it adds it
        /// </summary>
        public VertexSetWithType EntryVertexForTerm(ScopeContainer scope, Term term, ISet<uint> lastDefinitions = null)
        {
            if (lastDefinitions != null)
                if (term.IsGlobal && term.IsVar && !scope.LastDefDict.ContainsKey(term.First))
                    throw new UninitializedTerm(term);
                else
                    lastDefinitions.UnionWith(scope.LastDefDict[term.First].IntSet);

            if (!scope.EntryPointVerticesDict.ContainsKey(term.First.ToString()))
                return null;

            return scope.EntryPointVerticesDict[term.First.ToString()];
        }

        /// <summary>
        /// Returns the scope corresponding to a term (global or local)
        /// O(1) in time and space complexity
        /// </summary>
        public ScopeContainer ScopeForTerm(Term term)
        {
            return term.IsGlobal ? globalScope : CurrentScope();
        }

        /// <summary>
        /// Return the current scope
        /// O(1) in time and space complexity
        /// </summary>
        public ScopeContainer CurrentScope()
        {
            return scopeStack.Peek();
        }

        /// <summary>
        /// Crea y pushea un ScopeContainer a la pila
        /// Complejidad espacial y temporal O(1)
        /// </summary>
        ScopeContainer EnterMethod()
        {
            var ret = new ScopeContainer();
            scopeStack.Push(ret);
            return ret;
        }

        /// <summary>
        /// Agrega un término con sus respectivos vértices del PtG al scope
        /// Complejidad espacial: no influyente
        /// Complejidad temporal: en el enter method ninguna ya que los términos que se agregan son variables
        /// En el exit method depende del término que se devuelva. O(Add(term)) << O(aPt(term))
        /// La complejidad depende de la función aPt
        /// </summary>
        public void Add(Term term, ISet<PtgVertex> destinationVertices)
        {
            var scope = ScopeForTerm(term);
            if (destinationVertices == null && term.IsGlobal)
                throw new UninitializedTerm(term);

            // Pasando un null como parámetro los destination vértices son nulos
            var currentSet = destinationVertices != null ?
                (Configuration.TypesOptimization ? SolverUtils.CreateReferenceComparedPTGHashSet(destinationVertices.Where(x => Compatibles(x, term.Last.Symbol))) : destinationVertices) : SolverUtils.CreateReferenceComparedPTGHashSet();

            // Si el término es una variable, agrega las entradas al diccionario del scope y listo
            if (term.IsVar)
            {
                scope.OverrideEntryPointValues(term.First.ToString(), currentSet, term.Last.Symbol);
                return;
            }

            throw new SlicerException("No se debería llegar hasta acá");
        }

        public ISet<PtgVertex> StructCopy(ISet<PtgVertex> nodes)
        {
            Dictionary<PtgVertex, PtgVertex> toUpdate = new Dictionary<PtgVertex, PtgVertex>();
            Dictionary<PtgVertex, PtgVertex> alreadyCopied = new Dictionary<PtgVertex, PtgVertex>();
            ISet<PtgVertex> init = new HashSet<PtgVertex>();

            foreach (var v in nodes)
            {
                if (v.Type.IsStruct)
                {
                    var vc = new PtgVertex(Configuration, v.Description, v.IsMinType, v.Type, v.Immutable, v.VertexType);
                    toUpdate.Add(v, vc);
                    init.Add(vc);
                }
                else
                    init.Add(v);
            }

            while (toUpdate.Count > 0)
            {
                var kv = toUpdate.First();
                var v = kv.Key;
                var vc = kv.Value;

                foreach (var edge in v.CommonVertex)
                {
                    foreach (var w in edge.Value)
                    {
                        if (w.Type.IsStruct)
                        {
                            PtgVertex wc = null;
                            if (!alreadyCopied.ContainsKey(w))
                            {
                                if (!toUpdate.ContainsKey(w))
                                {
                                    wc = new PtgVertex(Configuration, w.Description, w.IsMinType, w.Type, w.Immutable, w.VertexType, PtgVertex.DefaultKind, w.Multiple, StaticModeStack.Count, "");
                                    toUpdate.Add(w, wc);
                                }
                                else
                                    wc = toUpdate[w];
                            }
                            else
                                wc = alreadyCopied[w];
                            vc.AddVertex(EdgeType.Common, wc, edge.Key);
                        }
                        else
                            vc.AddVertex(EdgeType.Common, w, edge.Key);
                    }
                }

                foreach (var t in v.SigmaVertex)
                    vc.AddVertex(EdgeType.Sigma, t);

                foreach (var t in v.LambdaVertex)
                    vc.AddVertex(EdgeType.Lambda, t);
                
                foreach (var ld in v.FieldsLastDef)
                    vc.FieldsLastDef.Add(ld.Key, new HashSet<uint>(ld.Value));

                toUpdate.Remove(v);
                alreadyCopied.Add(v, vc);
            }

            return init;
        }

        public void AddToGraph(AdjacencyGraph<PtgVertex, PtgEdge> graph, PtgVertex ptgVertex)
        {
            foreach (var commonVertexPair in ptgVertex.CommonVertex)
            {
                foreach (var commonVertex in commonVertexPair.Value)
                {
                    if (!graph.ContainsVertex(commonVertex))
                    {
                        graph.AddVertex(commonVertex);
                        AddToGraph(graph, commonVertex);
                    }
                    graph.AddEdge(new PtgEdge { Source = ptgVertex, Target = commonVertex, EdgeName = new Field(commonVertexPair.Key, null), EdgeType = EdgeType.Common });
                }
            }
            foreach (var sigmaVertex in ptgVertex.SigmaVertex)
            {
                if (!graph.ContainsVertex(sigmaVertex))
                {
                    graph.AddVertex(sigmaVertex);
                    AddToGraph(graph, sigmaVertex);
                }
                graph.AddEdge(new PtgEdge { Source = ptgVertex, Target = sigmaVertex, EdgeName = new Field("", null), EdgeType = EdgeType.Sigma });
            }
            foreach (var lambdaVertex in ptgVertex.LambdaVertex)
            {
                if (!graph.ContainsVertex(lambdaVertex))
                {
                    graph.AddVertex(lambdaVertex);
                    AddToGraph(graph, lambdaVertex);
                }
                graph.AddEdge(new PtgEdge { Source = ptgVertex, Target = lambdaVertex, EdgeName = new Field("", null), EdgeType = EdgeType.Lambda });
            }
        }

        void ValidarSet(VertexSets vertices)
        {
            // Primero que no estemos devolviendo vacío
            //if (vertices.WritableVertex.Count == 0 && vertices.ReadOnlyVertex.Count == 0)
            //    throw new SlicerException("No podemos devolver vacío");
            foreach (var v in vertices.WritableVertex)
                if (!Compatibles(v, vertices.Symbol))
                    throw new SlicerException("Error");
            
            if (vertices.WritableVertex.Count > 100)
            {
                var cantidadHubs = vertices.WritableVertex.Where(x => x.VertexType == VertexType.Hub).Count();
                var cantidadObjectWritable = vertices.WritableVertex.Where(x => x.VertexType != VertexType.Hub && x.Type.IsObject).Count();
                var cantidadObjectTotal = cantidadObjectWritable + cantidadHubs;
                var cantidadTipoAparente = vertices.WritableVertex.Where(x => x.VertexType != VertexType.Hub && !x.Type.IsObject && !x.IsMinType).Count();
                var cantidadTipoExacto = vertices.WritableVertex.Where(x => x.VertexType != VertexType.Hub && !x.Type.IsObject && x.IsMinType).Count();
            }
        }

        public virtual void SaveResults(string graphEvolutionFile, string internalProfileFile) { }

        public int cantidad_compatibles = 0;
        public int cantidad_compatibles_vuelta = 0;
        public int cantidad_no_compatibles = 0;
        public bool Compatibles(PtgVertex ptgVertex, ISlicerSymbol type)
        {
            // HACKKK (ESTO HAY QUE VER: TODO)
            //if (ptgVertex.VertexType == VertexType.Hub)
            //    return true;

            var returnValue = TypesUtils.Compatibles(type, ptgVertex.Type);

            // Si ya dio verdadera la primer comparación o bien el tipo del nodo que estamos comparando es mínimo ya salimos, sino tenemos que preguntar la vuelta
            if (!returnValue && !ptgVertex.IsMinType)
            {
                returnValue = TypesUtils.Compatibles(ptgVertex.Type, type);
                if (returnValue)
                    cantidad_compatibles_vuelta++;
            }

            if (returnValue)
                cantidad_compatibles++;
            else
                cantidad_no_compatibles++;

            return returnValue;
        }

        public bool Compatibles(PtgVertex ptgVertex, List<ETType> types, IDictionary<ETType, ISlicerSymbol> mapping)
        {
            return types.Any(y => Compatibles(ptgVertex, TypesUtils.GetTypeByETType(y, mapping)));
        }

        public abstract void FullBFS(ISet<PtgVertex> vertices, ISet<PtgVertex> visited);

        public void IsReachableNodesInGraph(PtgVertex w)
        {
            var vertexSet = SolverUtils.CreateReferenceComparedPTGHashSet();
            foreach (var ep in globalScope.EntryPointVerticesDict)
            {
                FullBFS(ep.Value.VertexSet, vertexSet);
                vertexSet.UnionWith(ep.Value.VertexSet);
            }
            foreach (var scope in scopeStack)
                foreach (var v in scope.LastDefDict.Where(x => !x.Value.IsTemporal))
                    if (scope.EntryPointVerticesDict.ContainsKey(v.Key.ToString()))
                    {
                        FullBFS(scope.EntryPointVerticesDict[v.Key.ToString()].VertexSet, vertexSet);
                        vertexSet.UnionWith(scope.EntryPointVerticesDict[v.Key.ToString()].VertexSet);
                    }

            var returnValue = vertexSet;

            if (returnValue.Contains(w))
            {
                Console.WriteLine("");
                foreach (var ep in globalScope.EntryPointVerticesDict)
                {
                    var t1 = SolverUtils.CreateReferenceComparedPTGHashSet();
                    FullBFS(ep.Value.VertexSet, t1);
                    if (t1.Contains(w) || ep.Value.VertexSet.Contains(w))
                        Console.WriteLine("");
                }
                foreach (var scope in scopeStack)
                    foreach (var v in scope.LastDefDict.Where(x => !x.Value.IsTemporal))
                        if (scope.EntryPointVerticesDict.ContainsKey(v.Key.ToString()))
                        {
                            var t1 = SolverUtils.CreateReferenceComparedPTGHashSet();
                            FullBFS(scope.EntryPointVerticesDict[v.Key.ToString()].VertexSet, t1);
                            if (t1.Contains(w) || scope.EntryPointVerticesDict[v.Key.ToString()].VertexSet.Contains(w))
                                Console.WriteLine("");
                        }
            }
        }

        public IEnumerable<PtgVertex> NullSetUnion(ISet<PtgVertex> a, ISet<PtgVertex> b)
        {
            if (a == null && b == null)
                return null;
            if (a != null && b != null)
                return a.Union(b);
            if (a != null)
                return a;
            return b;
        }
        #endregion
    }
}
