using DynAbs.Summaries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;
using StaticModeKey = System.String;

namespace DynAbs.Aliasing.CS
{
    class ClustersSolver : IAliasingSolver
    {
        #region Performance properties 
        public TimeSpan tfullbfs = new TimeSpan(0);
        public double cfullbfs = 0;

        #region Annotations
        public TimeSpan tannot_total_havoc = new TimeSpan(0);
        public TimeSpan tannot_fullbfs_ut = new TimeSpan(0);
        public TimeSpan tannot_apt = new TimeSpan(0);
        public TimeSpan tannot_apt_sub1 = new TimeSpan(0);
        public TimeSpan tannot_apt_sub2 = new TimeSpan(0);
        public TimeSpan tannot_apt_merge = new TimeSpan(0);

        public double cannot_total_havoc = 0;
        public double cannot_apt = 0;
        public double cannot_fullbfs_ut = 0;
        #endregion

        #region Speed
        public TimeSpan tspeed_total_havoc = new TimeSpan(0);
        public TimeSpan tspeed_havoc_uses = new TimeSpan(0);
        public TimeSpan tspeed_havoc_add_dg = new TimeSpan(0);
        public TimeSpan tspeed_unions = new TimeSpan(0);

        public double cspeed_total_havoc = 0;
        //public List<int> cspeed_havoc_dg = new List<int>();
        //public List<int> cspeed_dict_by_type_evol = new List<int>();
        #endregion
        #endregion

        #region StaticMode
        public bool StaticModeEnabled { get; set; }
        public Dictionary<StaticModeKey, PtgVertex> StaticModeAllocatedVertices = new Dictionary<StaticModeKey, PtgVertex>();
        public Dictionary<StaticModeKey, PtgVertex> StaticModeRVVertices = new Dictionary<StaticModeKey, PtgVertex>();
        public Dictionary<StaticModeKey, PtgVertex> StaticModeRegions = new Dictionary<StaticModeKey, PtgVertex>();
        public Dictionary<StaticModeKey, PtgVertex> StaticModeOtherNodes = new Dictionary<StaticModeKey, PtgVertex>();
        #endregion

        #region Properties
        UserSliceConfiguration Configuration;
        Stack<Scope> LocalStack = new Stack<Scope>();
        Scope Global = new Scope() { LastDefs = new Dictionary<Field, IntSetWithData>() { { new Field("AUXGLOBAL", ISlicerSymbol.CreateObjectSymbol()), new IntSetWithData(new HashSet<uint>(), 0, false) } } };
        Scope ScopeFor(Term t) => t.IsGlobal ? Global : LocalStack.Peek();
        #endregion

        public ClustersSolver(UserSliceConfiguration userSliceConfiguration)
        {
            Configuration = userSliceConfiguration;
        }

        public void EnterMethodAndBind(string methodSymbol, List<Term> actualParams, List<Term> formalParams, Term receiver, Term @this)
        {
            Scope previous = null, current = null;
            if (LocalStack.Count > 0)
                previous = LocalStack.Peek();

            current = new Scope();
            LocalStack.Push(current);
            current.Arguments = actualParams;
            current.Parameters = formalParams;

            for (var i = 0; i < formalParams.Count; i++)
                if (actualParams.Count > i && (actualParams[i] != null) && (!actualParams[i].IsScalar) && (!formalParams[i].IsScalar))
                {
                    var typeVertex = aPt(actualParams[i], actualParams[i].IsGlobal ? Global : previous);
                    if (formalParams[i].IsStruct)
                        typeVertex = StructCopy(typeVertex);

                    current.PointsTo[formalParams[i].First] = typeVertex;
                }

            if (receiver != null)
                current.PointsTo[@this.First] = aPt(receiver, receiver.IsGlobal ? Global : previous);
        }

        public ISet<uint> ExitMethodAndUnbind(Term term, Term returnExceptionTerm, Term exceptionTerm)
        {
            var lastScope = LocalStack.Pop();
            // Esta línea es para guardarnos el último scope, si no hacemos esto al final el DumpPtg pincha
            //lastPopedScope = lastScope;

            for (var i = 0; i < lastScope.Arguments.Count; i++)
            {
                var parameter = lastScope.Parameters[i];
                var argument = lastScope.Arguments[i].ReferencedTerm; // Usamos el original
                if (parameter.IsOutOrRef)
                {
                    Assign(argument, ScopeFor(argument), parameter, lastScope);
                    LastDef_Set(argument, (HashSet<uint>)LastDef_Get(parameter, lastScope));
                }
            }

            // Si no hay return value o no tenemos ningún término a quien devolver algo, retornamos
            ISet<uint> returnValue = null;
            if (term != null && lastScope.ReturnValue != null)
            {
                var returnValueScope = lastScope.ReturnValue.IsGlobal ? Global : lastScope;

                if (!term.IsScalar && !lastScope.ReturnValue.IsScalar)
                    Assign(term, ScopeFor(term), lastScope.ReturnValue, returnValueScope);

                returnValue = LastDef_Get(lastScope.ReturnValue, returnValueScope);
            }

            if (returnExceptionTerm != null && exceptionTerm != null)
            {
                var scopeForExceptionTerm = exceptionTerm.IsGlobal ? Global : lastScope;

                if (!returnExceptionTerm.IsScalar && !exceptionTerm.IsScalar)
                    Assign(returnExceptionTerm, ScopeFor(returnExceptionTerm), exceptionTerm, scopeForExceptionTerm);

                LastDef_Set(returnExceptionTerm, (HashSet<uint>)LastDef_Get(exceptionTerm, scopeForExceptionTerm));

                returnExceptionTerm.IsInitializedForException = true;
            }

            return returnValue;
        }

        public void AssignRV(Term term)
        {
            LocalStack.Peek().ReturnValue = term;
        }

        public void Alloc(Term term, bool @override = true, string kind = null)
        {
            StaticModeKey key = null;
            PtgVertex v = null;
            bool usingCachedVertex = false;

            if (StaticModeEnabled)
            {
                key = GetTermKey(term.Stmt);
                StaticModeAllocatedVertices.TryGetValue(key, out v);

                usingCachedVertex = v != null;
                if (v != null && v.Rep != null)
                    v.Rep.Single = false;
            }

            if (v == null)
                v = new PtgVertex(new TypeKind(term.Last.Symbol));

            var d = new Dictionary<TypeKind, HashSet<PtgVertex>>();
            d.Add(new TypeKind(v.TypeKind), CSUtils.CreateReferenceComparedPTGHashSet(v));

            var scope = ScopeFor(term);

            if (term.IsVar)
                scope.PointsTo[term.First] = d;
            else
            {
                // XXX: TODO: Hay un acceso a props o fields intermedias que se "usan" y no figuran en ningún lado. Hay que modificar bien.
                Assign(term, scope, d, false, null, true);
            }

            if (StaticModeEnabled && !usingCachedVertex)
                StaticModeAllocatedVertices[key] = v;
        }

        public void Assign(Term lhsTerm, Term rhsTerm)
        {
            var leftScope = ScopeFor(lhsTerm);
            var rightScope = ScopeFor(rhsTerm);
            Assign(lhsTerm, leftScope, rhsTerm, rightScope);
        }

        public void Assign(Term lhsTerm, Scope lhsScope, Term rhsTerm, Scope rhsScope)
        {
            var currentSet = aPt(rhsTerm, rhsScope) ?? new Dictionary<TypeKind, HashSet<PtgVertex>>();
            Assign(lhsTerm, lhsScope, currentSet);
        }

        public void Assign(Term lhsTerm, Scope lhsScope, Dictionary<TypeKind, HashSet<PtgVertex>> rightSet, bool copyStructs = true, List<PtgVertex> exceptedStructs = null, bool strongUpdate = true)
        {
            if (copyStructs && lhsTerm.Last.Symbol.IsStruct)
                rightSet = StructCopy(rightSet, exceptedStructs);

            if (lhsTerm.IsVar)
            {
                if (strongUpdate)
                    lhsScope.PointsTo[lhsTerm.First] = rightSet;
                else
                {
                    if (lhsScope.PointsTo[lhsTerm.First] == null)
                        lhsScope.PointsTo[lhsTerm.First] = rightSet;
                    else
                        Merge(lhsScope.PointsTo[lhsTerm.First], rightSet);
                }
                return;
            }

            var lhsVertices = aPt(lhsTerm.DiscardLast(), lhsScope);
            if ((lhsVertices == null || lhsVertices.Count == 0 || (lhsVertices.Count == 1 && lhsVertices.Single().Value.Count == 0)) && lhsTerm.IsGlobal)
                throw new UninitializedTerm(lhsTerm.DiscardLast());

            // Strong update
            if (lhsTerm.Last != Field.SIGMA_OBJ &&
                lhsVertices.Count == 1 &&
                lhsVertices.Single().Value.Count == 1 &&
                lhsVertices.Single().Value.Single().Rep.Single &&
                strongUpdate)
            {
                if (!lhsVertices.Single().Value.Single().Rep.Targets.ContainsKey(lhsVertices.Single().Value.Single().TypeKind))
                    lhsVertices.Single().Value.Single().Rep.Targets[lhsVertices.Single().Value.Single().TypeKind] =
                        new Dictionary<Field, Dictionary<TypeKind, HashSet<PtgVertex>>>();

                var d = new Dictionary<TypeKind, HashSet<PtgVertex>>();
                foreach (var tn in rightSet)
                    d[tn.Key] = CSUtils.CreateReferenceComparedPTGHashSet(tn.Value);
                // En teoría tiene solo un tipo... (no quiero hacer bardo potencial escribiendo el que trajo aPt)
                lhsVertices.Single().Value.Single().Rep.Targets[lhsVertices.Single().Value.Single().TypeKind][lhsTerm.Last] = d;
            }
            else
                foreach (var ltv in lhsVertices)
                    foreach (var v in ltv.Value)
                    {
                        var d = new Dictionary<TypeKind, HashSet<PtgVertex>>();
                        foreach (var tn in rightSet)
                            d[tn.Key] = CSUtils.CreateReferenceComparedPTGHashSet(tn.Value);
                        v.Rep.Union(new Cluster(ltv.Key, lhsTerm.Last, d));
                    }
        }

        public ISet<uint> LastDef_Get(Term term)
        {
            if (term == null)
                ;
            return LastDef_Get(term, ScopeFor(term));
        }

        public ISet<uint> LastDef_Get(Term term, Scope scope)
        {
            if (term.IsGlobal && term.IsVar && !scope.LastDefs.ContainsKey(term.First))
                throw new UninitializedTerm(term);

            if (!scope.LastDefs.ContainsKey(term.First) &&
                !term.IsTemporal &&
                term.Stmt.CSharpSyntaxNode.GetContainer() is Microsoft.CodeAnalysis.CSharp.Syntax.LocalFunctionStatementSyntax)
                return LastDef_Get(term, LocalStack.ToList()[LocalStack.ToList().IndexOf(scope) + 1]);

            try
            {
                if (term.IsVar)
                    return new HashSet<uint>(scope.LastDefs[term.First].IntSet);
            }
            catch (Exception ex)
            {
                ;
            }

            try
            {
                var collectedNodes = new HashSet<uint>();

                var vertices = aPt(term.DiscardLast(), scope, Configuration.IncludeAllUses ? collectedNodes : null);
                if (vertices == null && term.IsGlobal)
                    throw new UninitializedTerm(term.DiscardLast());

                foreach (var tn in vertices)
                    foreach (var n in tn.Value)
                        collectedNodes.UnionWith(n.Rep.GetLastDefs(tn.Key, term.Last));

                return collectedNodes;
            }
            catch
            {
                ;
            }
            if (true)
                throw new SlicerException("Cannot discard (most common)");
        }

        public void LastDef_Set(Term term, uint lastDef)
        {
            LastDef_Set(term, new HashSet<uint>() { lastDef });
        }

        public void LastDef_Set(Term term, HashSet<uint> lastDef, bool weak = false)
        {
            var scope = ScopeFor(term);
            if (term.IsVar)
            {
                if (scope.LastDefs.ContainsKey(term.First))
                {
                    if (weak)
                        lastDef.UnionWith(scope.LastDefs[term.First].IntSet);
                    scope.LastDefs[term.First].IntSet = lastDef;
                }
                else
                    scope.LastDefs[term.First] = new IntSetWithData(lastDef, 0, term.IsTemporal);
                return;
            }

            var vertices = aPt(term.DiscardLast(), scope);
            if ((vertices == null || vertices.Count == 0 || (vertices.Count == 1 && vertices.Single().Value.Count == 0)) && term.IsGlobal)
                throw new UninitializedTerm(term.DiscardLast());

            // Strong update
            if (term.Last != Field.SIGMA_OBJ &&
                vertices.Count == 1 &&
                vertices.Single().Value.Count == 1 &&
                vertices.Single().Value.Single().Rep.Single)
            {
                if (!vertices.Single().Value.Single().Rep.LastDefs.ContainsKey(vertices.Single().Value.Single().TypeKind))
                    vertices.Single().Value.Single().Rep.LastDefs[vertices.Single().Value.Single().TypeKind] = new Dictionary<Field, HashSet<uint>>();
                // En teoría tiene solo un tipo... (no quiero hacer bardo potencial escribiendo el que trajo aPt)
                vertices.Single().Value.Single().Rep.LastDefs[vertices.Single().Value.Single().TypeKind][term.Last] = lastDef;
            }
            else
                foreach (var tn in vertices)
                    foreach (var n in tn.Value)
                        n.Rep.Union(new Cluster(tn.Key, term.Last, lastDef));
        }

        public void Havoc(Term receiver, List<Term> arguments, Term returnValue,
            Func<Stmt, ISet<uint>, uint> GetDGNode, AnnotationWithData ad, Stmt invocationPoint)
        {
            var st = DateTime.Now;
            if (ad == null)
            {
                if (Globals.optimize_types)
                    HavocOPT(receiver, arguments, returnValue, GetDGNode, invocationPoint);
                else
                    Havoc(receiver, arguments, returnValue, GetDGNode, invocationPoint);
                cspeed_total_havoc++;
                tspeed_total_havoc = tspeed_total_havoc.Add(DateTime.Now.Subtract(st));
            }
            else
            {
                var reads = GetR(receiver, arguments, returnValue, null, ad);
                var dgv = GetDGNode(invocationPoint, reads);
                var otherNodes = GetOtherNodes(dgv, ad, invocationPoint);
                SetReturnValue(receiver, arguments, otherNodes, dgv, returnValue, ad, invocationPoint);
                SetW(dgv, receiver, arguments, returnValue, otherNodes, ad);
                SetCN(dgv, receiver, arguments, returnValue, otherNodes, ad);
                cannot_total_havoc++;
                tannot_total_havoc = tannot_total_havoc.Add(DateTime.Now.Subtract(st));
            }
        }

        public void Havoc(Term receiver, List<Term> arguments, Term returnValue, Func<Stmt, ISet<uint>, uint> GetDGNode, Stmt invocationPoint)
        {
            var st = DateTime.Now;

            var allArgs = new List<Term>();
            if (receiver != null)
                allArgs.Add(receiver);
            allArgs.AddRange(arguments);

            var uses = new HashSet<uint>();
            foreach (var p in allArgs)
                uses.UnionWith(LastDef_Get(p));

            var reachableNodes = NodesReachableFrom(allArgs.ToArray());

            foreach (var t in reachableNodes.Item1.SelectMany(x => x.Value).SelectMany(x => x.Rep.LastDefs.SelectMany(y => y.Value)))
                uses.UnionWith(t.Value);
            foreach (var t in reachableNodes.Item2.SelectMany(x => x.Rep.LastDefs.SelectMany(y => y.Value)))
                uses.UnionWith(t.Value);

            tspeed_havoc_uses = tspeed_havoc_uses.Add(DateTime.Now.Subtract(st));
            st = DateTime.Now;

            var lastDef = GetDGNode(invocationPoint, uses);

            //cspeed_havoc_dg.Add(uses.Count);
            tspeed_havoc_add_dg = tspeed_havoc_add_dg.Add(DateTime.Now.Subtract(st));

            // DG
            var region = CreateRegion(invocationPoint);
            if (StaticModeEnabled)
                region = region.Find();

            var rv = SetExternalRV(returnValue, lastDef);
            var outRefParams = SetOutRefParams(allArgs.Where(x => x.ReferencedTerm != null && x.ReferencedTerm.IsOutOrRef));

            var ALL = CSUtils.CreateReferenceComparedPTGHashSet(reachableNodes.Item1.SelectMany(x => x.Value));
            ALL.UnionWith(outRefParams);
            ALL.UnionWith(reachableNodes.Item2);
            ALL.Add(region);
            if (rv != null)
                ALL.Add(rv);

            // Como en todo esto hay una región hay un múltiple, entonces todo quedaría en múltiple
            if (ALL.Count > 0)
            {
                st = DateTime.Now;

                var v = ALL.First();
                ALL.Remove(v);

                foreach (var w in ALL)
                    AddLambdaLink(v, w);

                var V = v.Find();
                var to = new Dictionary<TypeKind, HashSet<PtgVertex>>();
                var tov = CSUtils.CreateReferenceComparedPTGHashSet();
                tov.Add(v.Find()); ;
                to.Add(new TypeKind(ISlicerSymbol.CreateObjectSymbol()), tov);
                V.Rep.Union(new Cluster(new TypeKind(ISlicerSymbol.CreateObjectSymbol()), Field.SIGMA_OBJ, to));

                var ld = new HashSet<uint>();
                ld.Add(lastDef);
                if (DynAbs.Globals.clean_last_def)
                    V.Rep.LastDefs = new Dictionary<TypeKind, Dictionary<Field, HashSet<uint>>>();

                V.Rep.Union(new Cluster(new TypeKind(ISlicerSymbol.CreateObjectSymbol()), Field.SIGMA_OBJ, ld));

                tspeed_unions = tspeed_unions.Add(DateTime.Now.Subtract(st));
            }

            foreach (var p in allArgs.Where(x => x.ReferencedTerm != null && x.ReferencedTerm.IsOutOrRef))
            {
                LastDef_Set(p.ReferencedTerm, lastDef);
                if (!p.IsScalar)
                    Assign(p.ReferencedTerm, p);
            }
        }

        public void HavocOPT(Term receiver, List<Term> arguments, Term returnValue, Func<Stmt, ISet<uint>, uint> GetDGNode, Stmt invocationPoint)
        {
            var st = DateTime.Now;

            var allArgs = new List<Term>();
            if (receiver != null)
                allArgs.Add(receiver);
            allArgs.AddRange(arguments);

            var uses = new HashSet<uint>();
            foreach (var p in allArgs)
                uses.UnionWith(LastDef_Get(p));

            var reachableNodes = NodesReachableFrom(allArgs.ToArray());

            foreach (var t in reachableNodes.Item1.SelectMany(x => x.Value).SelectMany(x => x.Rep.LastDefs.SelectMany(y => y.Value)))
                uses.UnionWith(t.Value);
            foreach (var t in reachableNodes.Item2.SelectMany(x => x.Rep.LastDefs.SelectMany(y => y.Value)))
                uses.UnionWith(t.Value);

            tspeed_havoc_uses = tspeed_havoc_uses.Add(DateTime.Now.Subtract(st));
            st = DateTime.Now;

            var lastDef = GetDGNode(invocationPoint, uses);

            //cspeed_havoc_dg.Add(uses.Count);
            tspeed_havoc_add_dg = tspeed_havoc_add_dg.Add(DateTime.Now.Subtract(st));

            // DG
            var region = CreateRegion(invocationPoint);
            if (region == null)
                ;
            if (!StaticModeEnabled)
                region.Rep.CurrentHavocRegion = true;

            var V = region.Find();
            if (StaticModeEnabled)
            {
                if (V == null || V.Rep == null)
                    ;
                V.Rep.CurrentHavocRegion = true;
            }

            // Initialize region
            var to = new Dictionary<TypeKind, HashSet<PtgVertex>>();
            var tov = CSUtils.CreateReferenceComparedPTGHashSet();
            tov.Add(V); ;
            to.Add(new TypeKind(ISlicerSymbol.CreateObjectSymbol()), tov);
            //V.Rep.Union(new Cluster(new TypeKind(ISlicerSymbol.CreateObjectSymbol()), Field.SIGMA_OBJ, to));
            V.Rep.Targets = new Dictionary<TypeKind, Dictionary<Field, Dictionary<TypeKind, HashSet<PtgVertex>>>>();
            var dict = new Dictionary<Field, Dictionary<TypeKind, HashSet<PtgVertex>>>();
            dict.Add(Field.SIGMA_FIELD, to);
            V.Rep.Targets.Add(new TypeKind(ISlicerSymbol.CreateObjectSymbol()), dict);

            var ld = new HashSet<uint>();
            ld.Add(lastDef);
            //V.Rep.Union(new Cluster(new TypeKind(ISlicerSymbol.CreateObjectSymbol()), Field.SIGMA_OBJ, ld));
            V.Rep.LastDefs = new Dictionary<TypeKind, Dictionary<Field, HashSet<uint>>>();
            var dict2 = new Dictionary<Field, HashSet<uint>>();
            dict2.Add(Field.SIGMA_OBJ, ld);
            V.Rep.LastDefs.Add(new TypeKind(ISlicerSymbol.CreateObjectSymbol()), dict2);
            // End initialize region

            var rv = SetExternalRV(returnValue, lastDef);
            var outRefParams = SetOutRefParams(allArgs.Where(x => x.ReferencedTerm != null && x.ReferencedTerm.IsOutOrRef));

            var ALL = CSUtils.CreateReferenceComparedPTGHashSet(reachableNodes.Item1.SelectMany(x => x.Value));
            ALL.UnionWith(outRefParams);
            ALL.UnionWith(reachableNodes.Item2);
            //ALL.Add(region);
            if (rv != null)
                ALL.Add(rv);

            // Como en todo esto hay una región hay un múltiple, entonces todo quedaría en múltiple
            st = DateTime.Now;

            foreach (var w in ALL)
                AddLambdaLinkOPT(region, w);

            V = region.Find();
            if (!V.Rep.CurrentHavocRegion)
                ;
            V.Rep.CurrentHavocRegion = false;

            tspeed_unions = tspeed_unions.Add(DateTime.Now.Subtract(st));

            foreach (var p in allArgs.Where(x => x.ReferencedTerm != null && x.ReferencedTerm.IsOutOrRef))
            {
                LastDef_Set(p.ReferencedTerm, lastDef);
                if (!p.IsScalar)
                    Assign(p.ReferencedTerm, p);
            }
        }

        public void CleanTemporaryEntries()
        {
            var currentScope = LocalStack.Peek();
            var keys = currentScope.LastDefs.Where(x => x.Value.IsTemporal).Select(x => x.Key).ToList();
            foreach (var k in keys)
            {
                currentScope.PointsTo.Remove(k);
                currentScope.LastDefs.Remove(k);
            }
        }

        public void RedefineType(Term term)
        {
            var vertices = aPt(term);
            if (vertices.Count == 1 &&
                vertices.Single().Value.Count == 1 &&
                vertices.Single().Value.Single().Rep.Single &&
                !vertices.Single().Value.Single().IsMinType &&
                TypesUtils.GetMin(vertices.Single().Value.Single().TypeKind.Type, term.Last.Symbol) == term.Last.Symbol)
                vertices.Single().Value.Single().TypeKind = new TypeKind(term.Last.Symbol, vertices.Single().Value.Single().TypeKind.Kind);
        }

        public void SaveResults(string graphEvolutionFile, string internalProfileFile)
        {
            //if (!string.IsNullOrWhiteSpace(graphEvolutionFile))
            //{
            //    var sb = new StringBuilder();
            //    sb.AppendLine("BFS VisitCounter");
            //    for (var i = 0; i < bfs_toVisitCounter.Count; i++)
            //        sb.AppendLine(bfs_toVisitCounter[i].ToString());
            //    System.IO.File.WriteAllText(graphEvolutionFile, sb.ToString());
            //}

            if (!string.IsNullOrWhiteSpace(internalProfileFile))
            {
                var sb2 = new StringBuilder();
                sb2.AppendLine(string.Format("TYPES: GET MIN: {0}", Globals.TypesTimeGetMin));
                sb2.AppendLine(string.Format("TYPES: GET FIELD SYMBOL: {0}", Globals.TypesTimeGetFieldSymbol));
                sb2.AppendLine(string.Format("TYPES: COMPATIBLE: {0}", Globals.TypesTimeCompatibles));
                //sb2.AppendLine(string.Format("CANT. COMPATIBLES: {0}", cantidad_compatibles));
                //sb2.AppendLine(string.Format("CANT. COMPATIBLES VUELTA: {0}", cantidad_compatibles_vuelta));
                //sb2.AppendLine(string.Format("CANT. NO COMPATIBLES: {0}", cantidad_no_compatibles));
                sb2.AppendLine(string.Format("HAVOC FULL BFS: {0}", tfullbfs.TotalSeconds));
                sb2.AppendLine(string.Format("COUNT FULL BFS: {0}", cfullbfs));

                sb2.AppendLine("ANNOTATIONS:");
                sb2.AppendLine(string.Format("TIME TOTAL HAVOC: {0}", tannot_total_havoc.TotalSeconds));
                sb2.AppendLine(string.Format("COUNT TOTAL HAVOC: {0}", cannot_total_havoc));
                sb2.AppendLine(string.Format("TIME BFS UNTIL TYPE : {0}", tannot_fullbfs_ut.TotalSeconds));
                sb2.AppendLine(string.Format("COUNT BFS UNTIL TYPE : {0}", cannot_fullbfs_ut));
                sb2.AppendLine(string.Format("TIME APT: {0}", tannot_apt.TotalSeconds));
                sb2.AppendLine(string.Format("COUNT APT: {0}", cannot_apt));
                sb2.AppendLine(string.Format("TIME APT BASE: {0}", tannot_apt_sub1.TotalSeconds));
                sb2.AppendLine(string.Format("TIME APT FEATURES: {0}", tannot_apt_sub2.TotalSeconds));
                sb2.AppendLine(string.Format("TIME APT MERGE: {0}", tannot_apt_merge.TotalSeconds));

                sb2.AppendLine("SPEED:");
                sb2.AppendLine(string.Format("TOTAL HAVOC: {0}", tspeed_total_havoc.TotalSeconds));
                sb2.AppendLine(string.Format("COUNT HAVOC: {0}", cspeed_total_havoc));
                sb2.AppendLine(string.Format("HAVOC USES SECS: {0}", tspeed_havoc_uses.TotalSeconds));
                sb2.AppendLine(string.Format("HAVOC DG SECS: {0}", tspeed_havoc_add_dg.TotalSeconds));
                sb2.AppendLine(string.Format("UNION BY TYPE SECS: {0}", tspeed_unions.TotalSeconds));

                System.IO.File.WriteAllText(internalProfileFile, sb2.ToString());
            }
        }

        public void DumpPTG(string path = @"C:\temp\PointsTo.dot", string label = null, bool getGlobalScope = false, string key = null)
        {
            // TODO: Last poped scope
            Scope scope = null;
            if (!getGlobalScope)
            {
                if (LocalStack.Count > 0)
                    scope = LocalStack.Peek();
            }
            else
                scope = Global;

            if (scope == null)
                return;

            if (key != null)
                throw new NotImplementedException();

            System.IO.File.WriteAllText(path.Replace(".dot", ".dgml"), CSUtils.GenerateDGML(scope, label));
        }

        #region StaticMode
        public bool StaticMode => false;

        public bool Converged()
        {
            return false;
        }
        public bool EnterStaticMode(bool EnterByCallbacks = false)
        {
            return false;
        }

        public void ExitStaticMode()
        {

        }

        public void EnterLoop()
        {
        }

        public void NextLoopIteration()
        {
        }

        public void ExitLoop()
        {
        }
        #endregion

        #region Navegation
        Dictionary<TypeKind, HashSet<PtgVertex>> aPt(Term t, HashSet<uint> lastDefinitions = null)
        {
            var scope = ScopeFor(t);
            try
            {
                return aPt(t, scope, lastDefinitions);
            }
            catch (UninitializedTerm)
            {
                throw;
            }
            catch (Exception ex)
            {
                if (!scope.LastDefs.ContainsKey(t.First) &&
                    !t.IsTemporal &&
                    t.Stmt.CSharpSyntaxNode.GetContainer() is Microsoft.CodeAnalysis.CSharp.Syntax.LocalFunctionStatementSyntax)
                    return aPt(t, LocalStack.ToList()[1], lastDefinitions);
            }

            return null;
        }

        Dictionary<TypeKind, HashSet<PtgVertex>> aPt(Term t, Scope scope, HashSet<uint> lastDefinitions = null)
        {
            var toReturn = new Dictionary<TypeKind, HashSet<PtgVertex>>();

            if (lastDefinitions != null && scope.PointsTo.ContainsKey(t.First))
                lastDefinitions.UnionWith(scope.LastDefs[t.First].IntSet);

            if (scope.PointsTo.ContainsKey(t.First))
            {
                // TODO: se podría utilizar el tipo del t.First.Symbol... TODOTK
                foreach (var tks in scope.PointsTo[t.First])
                    toReturn.Add(tks.Key, CSUtils.CreateReferenceComparedPTGHashSet(tks.Value.Select(x => x.Find())));

                for (var i = 1; i < t.Parts.Count; i++)
                    toReturn = Move(toReturn, t.Parts[i], lastDefinitions);
            }

            return toReturn;
        }

        Dictionary<TypeKind, HashSet<PtgVertex>> Move(Dictionary<TypeKind, HashSet<PtgVertex>> from, Field f, HashSet<uint> lastDefinitions = null)
        {
            var toReturn = new Dictionary<TypeKind, HashSet<PtgVertex>>();
            foreach (var tn in from)
            {
                foreach (var n in tn.Value)
                {
                    foreach (var target in n.Rep.Targets)
                    {
                        if (tn.Key.Compatibles(target.Key))
                        {
                            foreach (var field in target.Value)
                            {
                                if (field.Key == Field.SIGMA_OBJ || f == Field.SIGMA_OBJ || field.Key.Value == f.Value)
                                {
                                    foreach (var t2 in field.Value)
                                    {
                                        if (t2.Key.Compatibles(f.Symbol))
                                        {
                                            if (!toReturn.ContainsKey(t2.Key))
                                                toReturn[t2.Key] = CSUtils.CreateReferenceComparedPTGHashSet();
                                            toReturn[t2.Key].UnionWith(t2.Value.Select(x => x.Find()));

                                            if (lastDefinitions != null)
                                                lastDefinitions.UnionWith(n.Rep.GetLastDefs(tn.Key, field.Key));
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return toReturn;
        }
        #endregion

        #region Havoc Methods
        Tuple<Dictionary<TypeKind, HashSet<PtgVertex>>, HashSet<PtgVertex>> NodesReachableFrom(Term[] writableTerms)
        {
            var reachableVertexs = new Tuple<Dictionary<TypeKind, HashSet<PtgVertex>>, HashSet<PtgVertex>>
                (new Dictionary<TypeKind, HashSet<PtgVertex>>(), CSUtils.CreateReferenceComparedPTGHashSet());

            foreach (var term in writableTerms)
            {
                var temp = aPt(term);
                if (temp != null)
                {
                    BFS(temp.SelectMany(x => x.Value), reachableVertexs.Item2);

                    if (term.ReferencedTerm != null && term.ReferencedTerm.IsRef)
                        foreach (var kv in temp)
                            reachableVertexs.Item2.UnionWith(kv.Value);
                    else
                        foreach (var kv in temp)
                        {
                            if (!reachableVertexs.Item1.ContainsKey(kv.Key))
                                reachableVertexs.Item1[kv.Key] = kv.Value;
                            else
                                reachableVertexs.Item1[kv.Key].UnionWith(kv.Value);
                        }
                }
            }

            return reachableVertexs;
        }

        HashSet<PtgVertex> BFS(IEnumerable<PtgVertex> from, HashSet<PtgVertex> visited)
        {
            var st = DateTime.Now;
            cfullbfs++;

            var toVisit = CSUtils.CreateReferenceComparedPTGHashSet();

            foreach (var v in from)
            {
                if (visited.Contains(v))
                    continue;

                foreach (var t in v.Rep.Targets)
                    foreach (var f in t.Value)
                        foreach (var tn in f.Value)
                            foreach (var n in tn.Value.Select(x => x.Find()).Where(x => x != v))
                                toVisit.Add(n);
            }

            while (toVisit.Count > 0)
            {
                var v = toVisit.First();
                toVisit.Remove(v);
                if (visited.Contains(v))
                    continue;
                visited.Add(v);

                foreach (var t in v.Rep.Targets)
                    foreach (var f in t.Value)
                        foreach (var tn in f.Value)
                            foreach (var n in tn.Value.Select(x => x.Find()).Where(x => x != v))
                                toVisit.Add(n);
            }

            tfullbfs = tfullbfs.Add(DateTime.Now.Subtract(st));
            return visited;
        }

        PtgVertex SetExternalRV(Term t, uint s)
        {
            if (t != null)
            {
                LastDef_Set(t, s);

                if (!t.IsScalar)
                {
                    StaticModeKey key = null;
                    PtgVertex v = null;
                    bool usingCachedVertex = false;

                    if (StaticModeEnabled)
                    {
                        key = GetTermKey(t.Stmt);
                        StaticModeRVVertices.TryGetValue(key, out v);

                        usingCachedVertex = v != null;
                        if (v != null && v.Rep != null)
                            v.Rep.Single = false;
                    }

                    if (v == null)
                        v = new PtgVertex(new TypeKind(t.Last.Symbol));

                    var dic = new Dictionary<TypeKind, HashSet<PtgVertex>>();
                    dic[v.TypeKind] = new HashSet<PtgVertex>();
                    dic[v.TypeKind].Add(v);
                    Assign(t, ScopeFor(t), dic, false);

                    if (StaticModeEnabled && !usingCachedVertex)
                        StaticModeRVVertices[key] = v;

                    return v;
                }
            }
            return null;
        }

        List<PtgVertex> SetOutRefParams(IEnumerable<Term> outParams)
        {
            var newVertex = new List<PtgVertex>();
            if (outParams != null)
            {
                foreach (var p in outParams)
                    if (!p.IsScalar)
                    {
                        var tk = new TypeKind(p.Last.Symbol);
                        var v = new PtgVertex(tk);
                        var dic = new Dictionary<TypeKind, HashSet<PtgVertex>>();
                        dic[tk] = new HashSet<PtgVertex>();
                        dic[tk].Add(v);
                        Assign(p, ScopeFor(p), dic, false, null, false);
                        newVertex.Add(v);
                    }
            }
            return newVertex;
        }

        PtgVertex CreateRegion(Stmt invocationPoint)
        {
            StaticModeKey key = null;
            PtgVertex region = null;
            bool usingCachedVertex = false;

            if (StaticModeEnabled)
            {
                key = GetTermKey(invocationPoint);
                StaticModeRegions.TryGetValue(key, out region);
                usingCachedVertex = region != null;
            }

            if (region == null)
            {
                region = new PtgVertex(new TypeKind(ISlicerSymbol.CreateObjectSymbol()), true);
                region.Rep.IsHavocRegion = true;
            }

            if (StaticModeEnabled && !usingCachedVertex)
                StaticModeRegions[key] = region;

            return region;
        }

        void AddLambdaLink(PtgVertex v, PtgVertex w)
        {
            PtgVertex child, parent;

            // Traditional algorithm
            var repv = v.Find(); //finding representative of x
            var repw = w.Find(); //finding representative of y

            // Si ambos tienen mismo ranking elijo al this como padre
            if (repv.Rank == repw.Rank)
            {
                repv.Rank++; // Incrementing rank of y
                child = repw;
                parent = repv;
            }
            // Making parent which is having higher rank
            else if (repv.Rank > repw.Rank)
            {
                child = repw;
                parent = repv;
            }
            else
            {
                child = repv;
                parent = repw;
            }

            child.Parent = parent;
            parent.Rep.Union(child.Rep);
            parent.Rep.Single = false;
            child.Rep = null;
        }

        void AddLambdaLinkOPT(PtgVertex v, PtgVertex w)
        {
            PtgVertex child, parent;

            // Traditional algorithm
            var repv = v.Find(); //finding representative of x
            var repw = w.Find(); //finding representative of y

            // Si ambos tienen mismo ranking elijo al this como padre
            if (repv.Rank == repw.Rank)
            {
                repv.Rank++; // Incrementing rank of y
                child = repw;
                parent = repv;
            }
            // Making parent which is having higher rank
            else if (repv.Rank > repw.Rank)
            {
                child = repw;
                parent = repv;
            }
            else
            {
                child = repv;
                parent = repw;
            }

            child.Parent = parent;
            parent.Rep.Single = false;
            var xRep = parent.Rep;
            var yRep = child.Rep;
            child.Rep = null;

            if (Globals.optimize_types)
            {
                if (xRep.CurrentHavocRegion)
                    xRep.UnionOPT(yRep);
                else if (yRep.CurrentHavocRegion)
                {
                    yRep.UnionOPT(xRep);
                    xRep = yRep;
                }
                else
                    xRep.Union(yRep);
            }
            else
                xRep.Union(yRep);

            parent.Rep = xRep;
        }
        #endregion

        #region Annotations
        ISet<uint> GetR(Term receiver, List<Term> arguments, Term returnValue, List<PtgVertex> otherNodes, AnnotationWithData ad)
        {
            var metf = ad.Annotation.R;
            var dependencies = new HashSet<uint>();
            foreach (var etf in metf)
            {
                if (etf.field == "")
                {
                    var terms = new List<Term>();
                    if (etf.et.@base == BaseET.R && receiver != null)
                        terms.Add(receiver);
                    else if (etf.et.@base == BaseET.P)
                    {
                        if (etf.et.ParamIndex.HasValue && etf.et.ParamIndex.Value < arguments.Count && arguments[etf.et.ParamIndex.Value] != null)
                            terms.Add(arguments[etf.et.ParamIndex.Value]);
                        else if (!etf.et.ParamIndex.HasValue)
                            terms.AddRange(arguments);
                    }
                    else if (etf.et.@base == BaseET.G)
                        terms.AddRange(Global.LastDefs.Select(x => TermFactory.Create(new List<Field>() { new Field(x.Key.Value, x.Key.Symbol) }, true, x.Value.IsTemporal, false)));
                    else if (etf.et.@base == BaseET.RV)
                        throw new NotImplementedException();
                    else if (etf.et.@base == BaseET.RO)
                        throw new NotImplementedException();

                    foreach (var t in terms)
                        dependencies.UnionWith(LastDef_Get(t));
                }
                else
                {
                    if (etf.field[0] == '@')
                    {
                        if (ad.FieldsParameters.ContainsKey(etf.field.Substring(1)) &&
                            ad.FieldsParameters[etf.field.Substring(1)] == "#")
                            continue;

                        etf.field = (ad.FieldsParameters.ContainsKey(etf.field.Substring(1))) ?
                            ad.FieldsParameters[etf.field.Substring(1)] : "?";
                    }

                    var field = new Field(etf.field, ISlicerSymbol.CreateObjectSymbol());

                    var typeKindNodes = aPt(receiver, arguments, returnValue, otherNodes, new List<ET>() { etf.et }, ad);
                    foreach (var tkn in typeKindNodes)
                        foreach (var node in tkn.Value)
                            dependencies.UnionWith(node.Rep.GetLastDefs(tkn.Key, etf.field == "?" ? Field.SIGMA_OBJ : field));
                }
            }
            return dependencies;
        }

        void SetW(uint dgv, Term receiver, List<Term> arguments, Term returnValue, List<PtgVertex> otherNodes, AnnotationWithData ad)
        {
            var metf = ad.Annotation.W;
            foreach (var etf in metf)
            {
                if (etf.field == "")
                {
                    var terms = new List<Term>();
                    if (etf.et.@base == BaseET.R && receiver != null)
                        terms.Add(receiver);
                    else if (etf.et.@base == BaseET.P)
                    {
                        if (etf.et.ParamIndex.HasValue && etf.et.ParamIndex.Value < arguments.Count && arguments[etf.et.ParamIndex.Value] != null
                            && arguments[etf.et.ParamIndex.Value].ReferencedTerm != null && arguments[etf.et.ParamIndex.Value].ReferencedTerm.IsOutOrRef)
                            terms.Add(arguments[etf.et.ParamIndex.Value].ReferencedTerm);
                        else if (!etf.et.ParamIndex.HasValue)
                            terms.AddRange(arguments.Where(x => x.ReferencedTerm != null && x.ReferencedTerm.IsOutOrRef).Select(x => x.ReferencedTerm));
                    }
                    else if (etf.et.@base == BaseET.G)
                        terms.AddRange(Global.LastDefs.Select(x => TermFactory.Create(new List<Field>() { new Field(x.Key.Value, x.Key.Symbol) }, true, x.Value.IsTemporal, false)));
                    else if (etf.et.@base == BaseET.RV && returnValue != null)
                        terms.Add(returnValue);
                    else if (etf.et.@base == BaseET.RO)
                        throw new NotImplementedException();

                    foreach (var t in terms)
                        LastDef_Set(t, new HashSet<uint>() { dgv }, true);
                }
                else
                {
                    if (etf.field[0] == '@')
                    {
                        if (ad.FieldsParameters.ContainsKey(etf.field.Substring(1)) &&
                            ad.FieldsParameters[etf.field.Substring(1)] == "#")
                            continue;

                        etf.field = (ad.FieldsParameters.ContainsKey(etf.field.Substring(1))) ?
                            ad.FieldsParameters[etf.field.Substring(1)] : "?";
                    }

                    // Para el caso que no es "?" (TODOTK: El type estaría bueno..)
                    var field = new Field(etf.field, ISlicerSymbol.CreateObjectSymbol());
                    var lastDef = new HashSet<uint>() { dgv };

                    var nodes = aPt(receiver, arguments, returnValue, otherNodes, new List<ET>() { etf.et }, ad);
                    foreach (var tn in nodes)
                        foreach (var n in tn.Value)
                            if (etf.field == "?")
                                n.Rep.Union(new Cluster(tn.Key, Field.SIGMA_OBJ, lastDef));
                            else
                                n.Rep.Union(new Cluster(tn.Key, field, lastDef));
                }
            }
        }

        void SetCN(uint dgv, Term receiver, List<Term> arguments, Term returnValue, List<PtgVertex> otherNodes, AnnotationWithData ad)
        {
            var cns = ad.Annotation.CN;
            foreach (var cn in cns)
            {
                var sources = aPt(receiver, arguments, returnValue, otherNodes, cn.met1, ad);
                var targets = aPt(receiver, arguments, returnValue, otherNodes, cn.met2, ad);

                if (cn.field[0] == '@')
                {
                    if (ad.FieldsParameters.ContainsKey(cn.field.Substring(0)) &&
                        ad.FieldsParameters[cn.field.Substring(0)] == "#")
                        continue;

                    cn.field = (ad.FieldsParameters.ContainsKey(cn.field.Substring(0))) ?
                        ad.FieldsParameters[cn.field.Substring(0)] : "?";
                }

                // Para el caso que no es "?" (TODOTK: El type estaría bueno..)
                var field = new Field(cn.field, ISlicerSymbol.CreateObjectSymbol());

                foreach (var ltv in sources)
                    foreach (var v in ltv.Value)
                    {
                        var d = new Dictionary<TypeKind, HashSet<PtgVertex>>();
                        foreach (var tn in targets)
                            d[tn.Key] = CSUtils.CreateReferenceComparedPTGHashSet(tn.Value);

                        if (cn.field == "?")
                            v.Rep.Union(new Cluster(ltv.Key, Field.SIGMA_OBJ, d));
                        else
                            v.Rep.Union(new Cluster(ltv.Key, field, d));
                    }
            }
        }

        void SetReturnValue(Term receiver, List<Term> arguments, List<PtgVertex> otherNodes,
            uint dgv, Term returnValue, AnnotationWithData ad, Stmt invocationPoint)
        {
            if (returnValue == null)
                return;

            LastDef_Set(returnValue, dgv);
            if (ad.Annotation.RV == RVType.Null || returnValue == null)
                return;
            if (!returnValue.IsScalar)
            {
                if (ad.Annotation.RV == RVType.Fresh)
                    Alloc(returnValue, true, ad.Annotation.RVKind != "" ? ad.Annotation.RVKind : null);
                else
                {
                    var nodes = aPt(receiver, arguments, null, otherNodes, ad.Annotation.RVMET, ad);
                    Assign(returnValue, ScopeFor(returnValue), nodes, true, otherNodes);
                }
            }
        }

        List<PtgVertex> GetOtherNodes(uint lastDef, AnnotationWithData ad, Stmt invocationPoint)
        {
            var otherNodes = new List<PtgVertex>();

            if (StaticModeEnabled)
            {
                var key = GetTermKey(invocationPoint);
                if (!StaticModeOtherNodes.TryGetValue(key, out PtgVertex region))
                {
                    region = new PtgVertex(new TypeKind(ISlicerSymbol.CreateObjectSymbol()), true);
                    StaticModeOtherNodes[key] = region;
                }
                otherNodes.Add(region);
                // TODO: maybe, we should add one region per annotated node (or something even better)
            }
            else   
                foreach (var ro in ad.Annotation.RO)
                {
                    // TODO STATIC
                    var tk = new TypeKind(ro.ROTypes.Count == 0 ? ISlicerSymbol.CreateObjectSymbol() : TypesUtils.GetTypeByETType(ro.ROTypes.First(), ad.Mapping), ro.Kind != "" ? ro.Kind : Globals.DefaultKind);
                    var hub = new PtgVertex(tk, ro.Type == ROType.Many);
                    otherNodes.Add(hub);
                }
            return otherNodes;
        }

        Dictionary<TypeKind, HashSet<PtgVertex>> aPt(Term receiver, List<Term> arguments, Term returnValue, List<PtgVertex> otherNodes, List<ET> met, AnnotationWithData ad)
        {
            var startTime = DateTime.Now;
            cannot_apt++;
            var returnSet = new Dictionary<TypeKind, HashSet<PtgVertex>>();
            foreach (var et in met)
            {
                var initNodes = new Dictionary<TypeKind, HashSet<PtgVertex>>();
                if (et.@base == BaseET.RO && otherNodes != null)
                {
                    if (et.ParamIndex.HasValue && et.ParamIndex.Value < otherNodes.Count && otherNodes[et.ParamIndex.Value] != null)
                    {
                        var tk = new TypeKind(otherNodes[et.ParamIndex.Value].TypeKind);
                        if (!initNodes.ContainsKey(tk))
                            initNodes[tk] = CSUtils.CreateReferenceComparedPTGHashSet();
                        initNodes[tk].Add(otherNodes[et.ParamIndex.Value]);
                    }
                    else if (!et.ParamIndex.HasValue)
                        foreach (var ot in otherNodes)
                        {
                            var tk = new TypeKind(ot.TypeKind);
                            if (!initNodes.ContainsKey(tk))
                                initNodes[tk] = CSUtils.CreateReferenceComparedPTGHashSet();
                            initNodes[tk].Add(ot);
                        }
                }
                else
                {
                    var terms = new List<Term>();
                    if (et.@base == BaseET.R && receiver != null)
                        terms.Add(receiver);
                    else if (et.@base == BaseET.P)
                    {
                        if (et.ParamIndex.HasValue && et.ParamIndex.Value < arguments.Count && arguments[et.ParamIndex.Value] != null)
                            terms.Add(arguments[et.ParamIndex.Value]);
                        else if (!et.ParamIndex.HasValue)
                            terms.AddRange(arguments);
                    }
                    else if (et.@base == BaseET.G)
                        terms.AddRange(Global.LastDefs.Select(x => TermFactory.Create(new List<Field>() { new Field(x.Key.Value, x.Key.Symbol) }, true, x.Value.IsTemporal, false)));
                    else if (et.@base == BaseET.RV && returnValue != null)
                        terms.Add(returnValue);
                    foreach (var t in terms)
                    {
                        // 1. Obtenemos los nodos del grafo a los cuales hace referencia nuestro término
                        var start_apt1 = DateTime.Now;
                        var tNodes = aPt(t);
                        tannot_apt_sub1 = tannot_apt_sub1.Add(DateTime.Now.Subtract(start_apt1));

                        if (tNodes != null)
                        {
                            start_apt1 = DateTime.Now;
                            Merge(initNodes, tNodes);
                            tannot_apt_merge = tannot_apt_merge.Add(DateTime.Now.Subtract(start_apt1));
                        }
                    }
                }

                // 2: Si hay que moverse, nos movemos por todo lo que hay que moverse
                var start_apt2 = DateTime.Now;
                var nodes = aPt(initNodes, et.chainOfFields, et.untilType, et.ofType, et.ofKind, ad);
                tannot_apt_sub2 = tannot_apt_sub2.Add(DateTime.Now.Subtract(start_apt2));

                start_apt2 = DateTime.Now;
                Merge(returnSet, nodes);
                tannot_apt_merge = tannot_apt_merge.Add(DateTime.Now.Subtract(start_apt2));
            }

            tannot_apt = tannot_apt.Add(DateTime.Now.Subtract(startTime));
            return returnSet;
        }

        Dictionary<TypeKind, HashSet<PtgVertex>> aPt(Dictionary<TypeKind, HashSet<PtgVertex>> toVisitParams, List<string> fields, List<ETType> untilType, List<ETType> ofType, List<string> ofKind, AnnotationWithData ad)
        {
            var visited = toVisitParams;

            foreach (var f in fields)
            {
                if (f == "?")
                    visited = Move(visited, Field.SIGMA_OBJ);
                else if (f == "*")
                {
                    var nodes = FullBFS(visited, untilType, ad.Mapping);

                    var allTypes = new HashSet<ISlicerSymbol>();

                    if (ofType.Count > 0)
                        allTypes.UnionWith(ofType.Select(y => TypesUtils.GetTypeByETType(y, ad.Mapping)));
                    else
                        allTypes.Add(ISlicerSymbol.CreateObjectSymbol());


                    var allKinds = new HashSet<string>();

                    if (ofKind.Count > 0)
                        allKinds.UnionWith(ofKind);
                    else
                        allKinds.Add(Globals.TopKind);

                    var allTypeKinds = new HashSet<TypeKind>();
                    foreach (var t in allTypes)
                        foreach (var k in allKinds)
                            allTypeKinds.Add(new TypeKind(t, k));

                    visited = new Dictionary<TypeKind, HashSet<PtgVertex>>();
                    foreach (var tk in allTypeKinds)
                        visited.Add(tk, CSUtils.CreateReferenceComparedPTGHashSet(nodes.Where(n => Compatibles(n, tk))));

                    return visited;
                }
                else
                    visited = Move(visited, new Field(f, ISlicerSymbol.CreateObjectSymbol()));
            }

            // TODOTK
            if (ofType.Count > 0)
            {
                var temp = new Dictionary<TypeKind, HashSet<PtgVertex>>();
                foreach (var tkn in visited)
                {
                    if (Compatibles(tkn.Key, ofType, ad.Mapping))
                    {
                        if (!temp.ContainsKey(tkn.Key))
                            temp[tkn.Key] = CSUtils.CreateReferenceComparedPTGHashSet();
                        temp[tkn.Key].UnionWith(tkn.Value);
                    }
                }
                visited = temp;
            }
            if (ofKind.Count > 0)
            {
                var temp = new Dictionary<TypeKind, HashSet<PtgVertex>>();
                foreach (var tkn in visited)
                {
                    if (Compatibles(tkn.Key, ofKind))
                    {
                        if (!temp.ContainsKey(tkn.Key))
                            temp[tkn.Key] = CSUtils.CreateReferenceComparedPTGHashSet();
                        temp[tkn.Key].UnionWith(tkn.Value);
                    }
                }
                visited = temp;
            }
            return visited;
        }

        HashSet<PtgVertex> FullBFS(Dictionary<TypeKind, HashSet<PtgVertex>> vertices, List<ETType> untilType, IDictionary<ETType, ISlicerSymbol> mapping)
        {
            HashSet<PtgVertex> toReturn;
            if (Configuration.TypesOptimization && untilType != null && untilType.Count > 0)
                toReturn = FullBFS_UntilType(vertices.SelectMany(x => x.Value), untilType, mapping);
            else
            {
                var visited = CSUtils.CreateReferenceComparedPTGHashSet();
                BFS(vertices.SelectMany(x => x.Value), visited);
                toReturn = visited;
            }
            return toReturn;
        }

        HashSet<PtgVertex> FullBFS_UntilType(IEnumerable<PtgVertex> from, List<ETType> untilType, IDictionary<ETType, ISlicerSymbol> mapping)
        {
            var startTime = DateTime.Now;
            cannot_fullbfs_ut++;

            var visited = CSUtils.CreateReferenceComparedPTGHashSet();
            var toVisit = CSUtils.CreateReferenceComparedPTGHashSet();
            var toVisit_ofType = CSUtils.CreateReferenceComparedPTGHashSet();

            foreach (var v in from)
            {
                if (visited.Contains(v))
                    continue;

                foreach (var t in v.Rep.Targets)
                    foreach (var f in t.Value)
                        foreach (var tn in f.Value)
                            foreach (var n in tn.Value.Select(x => x.Find()))
                                if (Compatibles(n, untilType, mapping))
                                    toVisit_ofType.Add(n);
                                else
                                    toVisit.Add(n);
            }

            while (toVisit.Count > 0)
            {
                var v = toVisit.First();
                toVisit.Remove(v);
                if (visited.Contains(v))
                    continue;
                visited.Add(v);

                foreach (var t in v.Rep.Targets)
                    foreach (var f in t.Value)
                        foreach (var tn in f.Value)
                            foreach (var n in tn.Value.Select(x => x.Find()).Where(x => x != v))
                                if (Compatibles(n, untilType, mapping))
                                    toVisit_ofType.Add(n);
                                else
                                    toVisit.Add(n);
            }

            while (toVisit_ofType.Count > 0)
            {
                var v = toVisit_ofType.First();
                toVisit_ofType.Remove(v);
                if (visited.Contains(v))
                    continue;
                visited.Add(v);

                foreach (var t in v.Rep.Targets)
                    foreach (var f in t.Value)
                        foreach (var tn in f.Value)
                            foreach (var n in tn.Value.Select(x => x.Find()).Where(x => x != v))
                                if (Compatibles(n, untilType, mapping))
                                    toVisit_ofType.Add(n);
            }

            tannot_fullbfs_ut = tannot_fullbfs_ut.Add(DateTime.Now.Subtract(startTime));
            return visited;
        }

        bool Compatibles(PtgVertex from, List<ETType> types, IDictionary<ETType, ISlicerSymbol> mapping)
        {
            return from.Rep.TypeKinds.Any(x => types.Any(y => x.Compatibles(TypesUtils.GetTypeByETType(y, mapping))));
        }

        bool Compatibles(PtgVertex from, TypeKind typeKind)
        {
            return from.Rep.TypeKinds.Any(x => x.Compatibles(typeKind));
        }

        bool Compatibles(TypeKind from, List<ETType> types, IDictionary<ETType, ISlicerSymbol> mapping)
        {
            return types.Any(x => from.Compatibles(TypesUtils.GetTypeByETType(x, mapping)));
        }

        bool Compatibles(TypeKind from, List<string> ofKind)
        {
            return ofKind.Any(y => from.Compatibles(y));
        }
        #endregion

        #region Extras
        public Dictionary<TypeKind, HashSet<PtgVertex>> StructCopy(Dictionary<TypeKind, HashSet<PtgVertex>> initNodes, List<PtgVertex> exceptedStructs = null)
        {
            // TODO STATIC (Just return the same but as multiple)
            var toUpdate = new Dictionary<PtgVertex, PtgVertex>();
            var alreadyCopied = new Dictionary<PtgVertex, PtgVertex>();
            var init = new Dictionary<TypeKind, HashSet<PtgVertex>>();

            foreach (var tks in initNodes)
            {
                if (!tks.Key.Type.IsStruct && !tks.Key.Type.IsObject)
                    continue;

                if (!init.ContainsKey(tks.Key))
                    init.Add(tks.Key, CSUtils.CreateReferenceComparedPTGHashSet());

                foreach (var v in tks.Value)
                {
                    if (exceptedStructs != null && exceptedStructs.Any(x => x.ID == v.ID))
                    {
                        init[tks.Key].Add(v);
                        continue;
                    }

                    if (v.Rep.TypeKinds.Any(x => x.Type.IsStruct || x.Type.IsObject))
                    {
                        var vc = new PtgVertex(v);
                        toUpdate.Add(v, vc);
                        init[tks.Key].Add(vc);
                    }
                    //else
                    //    init[tks.Key].Add(v);
                }

                // Keep the "Find" in toUpdate
                while (toUpdate.Count > 0)
                {
                    var kv = toUpdate.First();
                    var v = kv.Key;
                    var vc = kv.Value;

                    foreach (var tftv in v.Rep.Targets)
                    {
                        foreach (var ftv in tftv.Value)
                        {
                            foreach (var tv in ftv.Value)
                            {
                                foreach (var w in tv.Value.Select(x => x.Find()))
                                {
                                    PtgVertex wc = null;
                                    if (tv.Key.Type.IsStruct)
                                    {
                                        if (!alreadyCopied.ContainsKey(w))
                                        {
                                            if (!toUpdate.ContainsKey(w))
                                            {
                                                wc = new PtgVertex(w);
                                                toUpdate.Add(w, wc);
                                            }
                                            else
                                                wc = toUpdate[w];
                                        }
                                        else
                                            wc = alreadyCopied[w];
                                    }
                                    else
                                        wc = w;

                                    if (!vc.Rep.Targets.ContainsKey(tftv.Key))
                                        vc.Rep.Targets[tftv.Key] = new Dictionary<Field, Dictionary<TypeKind, HashSet<PtgVertex>>>();

                                    if (!vc.Rep.Targets[tftv.Key].ContainsKey(ftv.Key))
                                        vc.Rep.Targets[tftv.Key][ftv.Key] = new Dictionary<TypeKind, HashSet<PtgVertex>>();

                                    if (!vc.Rep.Targets[tftv.Key][ftv.Key].ContainsKey(tv.Key))
                                        vc.Rep.Targets[tftv.Key][ftv.Key][tv.Key] = CSUtils.CreateReferenceComparedPTGHashSet();

                                    vc.Rep.Targets[tftv.Key][ftv.Key][tv.Key].Add(wc);
                                }
                            }
                        }
                    }

                    foreach (var tld in v.Rep.LastDefs)
                    {
                        if (!vc.Rep.LastDefs.ContainsKey(tld.Key))
                            vc.Rep.LastDefs[tld.Key] = new Dictionary<Field, HashSet<uint>>();
                        foreach (var fld in tld.Value)
                            vc.Rep.LastDefs[tld.Key][fld.Key] = new HashSet<uint>(fld.Value);
                    }

                    toUpdate.Remove(v);
                    alreadyCopied[v] = vc;
                }
            }
            return init;
        }

        void Merge(Dictionary<TypeKind, HashSet<PtgVertex>> d1, Dictionary<TypeKind, HashSet<PtgVertex>> d2)
        {
            if (d2 != null)
                foreach (var tk2 in d2)
                    if (!d1.ContainsKey(tk2.Key))
                        d1.Add(tk2.Key, CSUtils.CreateReferenceComparedPTGHashSet(tk2.Value));
                    else
                        d1[tk2.Key].UnionWith(tk2.Value);
        }

        #region StaticMode Extras
        public StaticModeKey GetTermKey(Stmt stmt)
        {
            return stmt.GetHashCode().ToString();
        }
        #endregion
        #endregion
    }
}
