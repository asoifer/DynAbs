using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using DynAbs.Summaries;
using Microsoft.CodeAnalysis;
using QuikGraph;
using StaticModeKey = System.String;

namespace DynAbs
{
    public class MixedSolver : AbstractSolver, IAliasingSolver
    {
        #region Performance properties 
        public TimeSpan thavoc_fullbfs = new TimeSpan(0);
        public TimeSpan tlambda_closure = new TimeSpan(0);
        public TimeSpan tapt = new TimeSpan(0);
        public double capt = 0;

        #region Annotations
        public TimeSpan tannot_total_havoc = new TimeSpan(0);
        public TimeSpan tannot_fullbfs_ut = new TimeSpan(0);
        public TimeSpan tannot_apt = new TimeSpan(0);

        public double cannot_total_havoc = 0;
        public double cannot_apt = 0;
        public double cannot_fullbfs_ut = 0;
        #endregion

        #region Speed
        public TimeSpan tspeed_total_havoc = new TimeSpan(0);
        public TimeSpan tspeed_havoc_uses = new TimeSpan(0);
        public TimeSpan tspeed_havoc_add_dg = new TimeSpan(0);
        public TimeSpan tspeed_union_by_type = new TimeSpan(0);
        public TimeSpan tspeed_havoc_item1 = new TimeSpan(0);
        public TimeSpan tspeed_havoc_item2 = new TimeSpan(0);

        public double cspeed_total_havoc = 0;
        public List<int> cspeed_havoc_dg = new List<int>();
        public List<int> cspeed_dict_by_type_evol = new List<int>();
        #endregion
        #endregion

        #region Public
        public MixedSolver(UserSliceConfiguration userSliceConfiguration)
        {
            Configuration = userSliceConfiguration;
        }

        public void Havoc(Term receiver, List<Term> arguments, Term returnValue,
            Func<Stmt, ISet<uint>, uint> GetDGNode, AnnotationWithData ad, Stmt invocationPoint)
        {
            var st = DateTime.Now;
            if (ad == null)
            {
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

        public void Havoc(Term receiver, List<Term> arguments, Term returnValue, 
            Func<Stmt, ISet<uint>, uint> GetDGNode, Stmt invocationPoint)
        {
            var st = DateTime.Now;

            var allArgs = new List<Term>();
            if (receiver != null)
                allArgs.Add(receiver);
            allArgs.AddRange(arguments);

            var uses = new HashSet<uint>();
            // Last definitions:
            foreach (var p in allArgs)
                uses.UnionWith(LastDef_Get(p));

            // Obtenemos los reachable
            var reachableNodes = NodesReachableFrom(allArgs.ToArray());

            foreach (var t in reachableNodes.Item1.SelectMany(x => x.FieldsLastDef.SelectMany(y => y.Value)))
                uses.Add(t);
            foreach (var t in reachableNodes.Item2.SelectMany(x => x.FieldsLastDef.SelectMany(y => y.Value)))
                uses.Add(t);

            tspeed_havoc_uses = tspeed_havoc_uses.Add(DateTime.Now.Subtract(st));
            st = DateTime.Now;

            var lastDef = GetDGNode(invocationPoint, uses);
            cspeed_havoc_dg.Add(uses.Count);

            tspeed_havoc_add_dg = tspeed_havoc_add_dg.Add(DateTime.Now.Subtract(st));

            foreach (var p in allArgs.Where(x => x.ReferencedTerm != null && x.ReferencedTerm.IsOutOrRef))
                LastDef_Set(p.ReferencedTerm, lastDef);

            // Obtenemos el HUB
            var hub = CreateHub(returnValue, lastDef, ReturnVertexType.Default, invocationPoint);

            st = DateTime.Now;
            reachableNodes.Item2.Add(hub);
            var diccByType = UnionByType(reachableNodes.Item2);

            //foreach (var reachable in reachableNodes.Item2)
            //{
            //    // TODO: CHEQUEAR XXX NO ESTOY SEGURO
            //    if (((reachable.Find() != hub) && (!StaticMode || StaticModeEnabledByCallbackOrRecursion || !StaticModeStack.Peek().Contains(reachable))) && CheckEqualsSet)
            //        SetNotConverged(reachable.LoopDepthLevel);

            //    hub.Union(reachable, reachableNodes.Item2);
            //}

            tspeed_union_by_type = tspeed_union_by_type.Add(DateTime.Now.Subtract(st));
            st = DateTime.Now;

            // Parámetros
            foreach (var reachable in reachableNodes.Item1)
            {
                if (!reachableNodes.Item2.Contains(reachable))
                {
                    foreach(var kv in diccByType)
                    {
                        var e1_added = kv.Value.Find().AddVertex(EdgeType.Lambda, reachable);
                        var e2_added = reachable.AddVertex(EdgeType.Sigma, kv.Value.Find());
                        var ld_added = false;

                        if (Globals.clean_last_def)
                        {
                            ld_added = !reachable.FieldsLastDef.ContainsKey(Field.SIGMA_FIELD) || !reachable.FieldsLastDef[Field.SIGMA_FIELD].Contains(lastDef);
                            reachable.FieldsLastDef = new Dictionary<Field, ISet<uint>>();
                            reachable.FieldsLastDef[Field.SIGMA_FIELD] = new HashSet<uint>();
                            reachable.FieldsLastDef[Field.SIGMA_FIELD].Add(lastDef);
                        }
                        else
                        {
                            if (!reachable.FieldsLastDef.ContainsKey(Field.SIGMA_FIELD))
                                reachable.FieldsLastDef[Field.SIGMA_FIELD] = new HashSet<uint>();
                            ld_added = reachable.FieldsLastDef[Field.SIGMA_FIELD].Add(lastDef);
                        }

                        if (((e2_added || ld_added) && (!StaticMode || StaticModeEnabledByCallbackOrRecursion || !StaticModeStack.Peek().Contains(reachable))) && CheckEqualsSet)
                            SetNotConverged(reachable.LoopDepthLevel);

                        if ((e1_added && (!StaticMode || StaticModeEnabledByCallbackOrRecursion || !StaticModeStack.Peek().Contains(hub))) && CheckEqualsSet)
                            SetNotConverged(kv.Value.Find().LoopDepthLevel);

                        reachable.SigmaExcludedProperties = null;
                    }
                }
            }

            tspeed_havoc_item1 = tspeed_havoc_item1.Add(DateTime.Now.Subtract(st));
            st = DateTime.Now;

            cspeed_dict_by_type_evol.Add(diccByType.Count);
            foreach (var kv in diccByType)
            {
                var p = kv.Value.Find();

                p.AddVertex(EdgeType.Sigma, p);
                if (Globals.clean_last_def)
                    p.FieldsLastDef = new Dictionary<Field, ISet<uint>>();

                if (!p.FieldsLastDef.ContainsKey(Field.SIGMA_FIELD))
                    p.FieldsLastDef[Field.SIGMA_FIELD] = new HashSet<uint>();
                p.FieldsLastDef[Field.SIGMA_FIELD].Add(lastDef);

                foreach (var kv2 in diccByType)
                    if (kv2.Key != kv.Key)
                        kv.Value.AddVertex(EdgeType.Sigma, kv2.Value.Find());
            }

            tspeed_havoc_item2 = tspeed_havoc_item2.Add(DateTime.Now.Subtract(st));
        }

        public override void SaveResults(string graphEvolutionFile, string internalProfileFile)
        {
            if (!string.IsNullOrWhiteSpace(graphEvolutionFile))
            {
                var sb = new StringBuilder();
                sb.AppendLine("DICT BY TYPE");
                for (var i = 0; i < cspeed_dict_by_type_evol.Count; i++)
                    sb.AppendLine(cspeed_dict_by_type_evol[i].ToString());
                System.IO.File.WriteAllText(graphEvolutionFile, sb.ToString());
            }

            if (!string.IsNullOrWhiteSpace(internalProfileFile))
            {
                var sb2 = new StringBuilder();
                sb2.AppendLine(string.Format("TYPES: GET MIN: {0}", Globals.TypesTimeGetMin));
                sb2.AppendLine(string.Format("TYPES: GET FIELD SYMBOL: {0}", Globals.TypesTimeGetFieldSymbol));
                sb2.AppendLine(string.Format("TYPES: COMPATIBLE: {0}", Globals.TypesTimeCompatibles));
                sb2.AppendLine(string.Format("CANT. COMPATIBLES: {0}", cantidad_compatibles));
                sb2.AppendLine(string.Format("CANT. COMPATIBLES VUELTA: {0}", cantidad_compatibles_vuelta));
                sb2.AppendLine(string.Format("CANT. NO COMPATIBLES: {0}", cantidad_no_compatibles));
                sb2.AppendLine(string.Format("HAVOC FULL BFS: {0}", thavoc_fullbfs.TotalSeconds));
                sb2.AppendLine(string.Format("LAMBDA CLOSURE: {0}", tlambda_closure.TotalSeconds));
                sb2.AppendLine(string.Format("APT: {0}", tapt.TotalSeconds));

                sb2.AppendLine("ANNOTATIONS:");
                sb2.AppendLine(string.Format("TIME TOTAL HAVOC: {0}", tannot_total_havoc.TotalSeconds));
                sb2.AppendLine(string.Format("COUNT TOTAL HAVOC: {0}", cannot_total_havoc));
                sb2.AppendLine(string.Format("TIME BFS UNTIL TYPE : {0}", tannot_fullbfs_ut.TotalSeconds));
                sb2.AppendLine(string.Format("COUNT BFS UNTIL TYPE : {0}", cannot_fullbfs_ut));
                sb2.AppendLine(string.Format("TIME APT: {0}", tannot_apt.TotalSeconds));
                sb2.AppendLine(string.Format("COUNT APT: {0}", cannot_apt));

                sb2.AppendLine("SPEED:");
                sb2.AppendLine(string.Format("TOTAL HAVOC: {0}", tspeed_total_havoc.TotalSeconds));
                sb2.AppendLine(string.Format("COUNT HAVOC: {0}", cspeed_total_havoc));
                sb2.AppendLine(string.Format("HAVOC USES SECS: {0}", tspeed_havoc_uses.TotalSeconds));
                sb2.AppendLine(string.Format("HAVOC DG SECS: {0}", tspeed_havoc_add_dg.TotalSeconds));
                sb2.AppendLine(string.Format("UNION BY TYPE SECS: {0}", tspeed_union_by_type.TotalSeconds));
                sb2.AppendLine(string.Format("HAVOC ITEM 1 SECS: {0}", tspeed_havoc_item1.TotalSeconds));
                sb2.AppendLine(string.Format("HAVOC ITEM 2 SECS: {0}", tspeed_havoc_item2.TotalSeconds));

                System.IO.File.WriteAllText(internalProfileFile, sb2.ToString());
            }
        }
        #endregion

        #region Private
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
                        throw new NotImplementedException();
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

                    var nodes = aPt(receiver, arguments, returnValue, otherNodes, new List<ET>() { etf.et }, ad);
                    foreach (var node in nodes)
                    {
                        if (etf.field == "?")
                        {
                            foreach (var key in node.FieldsLastDef.Keys)
                                foreach (var v in node.FieldsLastDef[key])
                                    dependencies.Add(v);
                        }
                        else
                        {
                            // Si el vértice contiene el field, obtenemos sus last def, además nos movemos por SIGMA si este vértice no está excluido
                            if (node.FieldsLastDef.ContainsKey(field) && field != SIGMA_FIELD)
                                foreach (var t in node.FieldsLastDef[field])
                                    dependencies.Add(t);
                            if (!node.SigmaExcludedProperties.Contains(field) && node.FieldsLastDef.ContainsKey(SIGMA_FIELD))
                                foreach (var t in node.FieldsLastDef[SIGMA_FIELD])
                                    dependencies.Add(t);
                        }
                    }
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
                        throw new NotImplementedException();
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

                    // Para el caso que no es "?"
                    var field = new Field(etf.field, ISlicerSymbol.CreateObjectSymbol());
                    var lastDef = new HashSet<uint>() { dgv };

                    var nodes = aPt(receiver, arguments, returnValue, otherNodes, new List<ET>() { etf.et }, ad);
                    foreach (var node in nodes)
                    {
                        if (etf.field == "?")
                        {
                            if (!node.FieldsLastDef.ContainsKey(Field.SIGMA_FIELD))
                                node.FieldsLastDef[Field.SIGMA_FIELD] = new HashSet<uint>();
                            var ld_added = node.FieldsLastDef[Field.SIGMA_FIELD].Add(dgv);

                            if ((ld_added && (!StaticMode || StaticModeEnabledByCallbackOrRecursion || !StaticModeStack.Peek().Contains(node))) && CheckEqualsSet)
                                SetNotConverged(node.LoopDepthLevel);

                            node.SigmaExcludedProperties = null;
                        }
                        else
                        {
                            if (!node.FieldsLastDef.ContainsKey(field))
                                node.FieldsLastDef[field] = new HashSet<uint>();

                            if (CheckEqualsSet &&
                                (!StaticMode || StaticModeEnabledByCallbackOrRecursion || !StaticModeStack.Peek().Contains(node)) &&
                                !lastDef.IsSubsetOf(node.FieldsLastDef[field]))
                                SetNotConverged(node.LoopDepthLevel);
                            node.FieldsLastDef[field].UnionWith(lastDef);
                        }
                    }
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

                foreach (var s in sources)
                    foreach (var t in targets)
                    {
                        if (cn.field == "?")
                            s.AddVertex(EdgeType.Sigma, t);
                        else
                            s.AddVertex(EdgeType.Common, t, cn.field);
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
                    Assign(returnValue, ScopeForTerm(returnValue), nodes);
                }
            }
        }

        List<PtgVertex> GetOtherNodes(uint lastDef, AnnotationWithData ad, Stmt invocationPoint)
        {
            var otherNodes = new List<PtgVertex>();
            foreach (var ro in ad.Annotation.RO)
            {
                StaticModeKey key = "";
                PtgVertex hub = null;
                if (StaticMode)
                {
                    key = GetTermKey(invocationPoint);
                    MaySetVertexValue(ref hub, StaticModeHUB, key);
                }

                // Creamos el HUB
                if (hub == null)
                {
                    hub = new PtgVertex(Configuration, "Hub " + (HubUniqueId++), false, ro.ROTypes.Count == 0 ? ISlicerSymbol.CreateObjectSymbol() : TypesUtils.GetTypeByETType(ro.ROTypes.First(), ad.Mapping),
                        false, ro.Type == ROType.Many ? VertexType.Hub : VertexType.Common, ro.Kind != "" ? ro.Kind : PtgVertex.DefaultKind, false, StaticModeStack.Count, key);

                    if (StaticMode)
                    {
                        StaticModeHUB[key] = hub;
                        if (StaticModeStack.Count > 0)
                            StaticModeStack.Peek().Add(hub);
                    }

                    // Con HUBS nuevos ya el PTG se modificó (no existía previamente)
                    if (CheckEqualsSet && (!StaticMode || StaticModeEnabledByCallbackOrRecursion || !StaticModeStack.Peek().Contains(hub)))
                        SetNotConverged(hub.LoopDepthLevel);
                }
                otherNodes.Add(hub);
            }
            return otherNodes;
        }

        ISet<PtgVertex> aPt(Term receiver, List<Term> arguments, Term returnValue, List<PtgVertex> otherNodes, List<ET> met, AnnotationWithData ad)
        {
            var startTime = DateTime.Now;
            cannot_apt++;

            var returnSet = SolverUtils.CreateReferenceComparedPTGHashSet();

            foreach (var et in met)
            {
                var initNodes = SolverUtils.CreateReferenceComparedPTGHashSet();
                if (et.@base == BaseET.RO && otherNodes != null)
                {
                    if (et.ParamIndex.HasValue && et.ParamIndex.Value < otherNodes.Count && otherNodes[et.ParamIndex.Value] != null)
                        initNodes.Add(otherNodes[et.ParamIndex.Value]);
                    else if (!et.ParamIndex.HasValue)
                        initNodes.UnionWith(otherNodes);
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
                        throw new NotImplementedException();
                    else if (et.@base == BaseET.RV && returnValue != null)
                        terms.Add(returnValue);
                    foreach (var t in terms)
                    {
                        // 1. Obtenemos los nodos del grafo a los cuales hace referencia nuestro término
                        var tNodes = aPt(t);
                        if (tNodes != null)
                            initNodes.UnionWith(tNodes);
                    }
                }

                // 2: Si hay que moverse, nos movemos por todo lo que hay que moverse
                var nodes = aPt(initNodes, et.chainOfFields, et.untilType, et.ofType, et.ofKind, ad);
                returnSet.UnionWith(nodes);
            }

            tannot_apt = tannot_apt.Add(DateTime.Now.Subtract(startTime));
            return returnSet;
        }

        ISet<PtgVertex> aPt(ISet<PtgVertex> toVisitParams, List<string> fields, List<ETType> untilType, List<ETType> ofType, List<string> ofKind, AnnotationWithData ad)
        {
            var visited = SolverUtils.CreateReferenceComparedPTGHashSet(toVisitParams);
            foreach (var f in fields)
            {
                if (f == "?")
                    visited = PtgPointsTo(visited, Field.SIGMA_FIELD, ISlicerSymbol.CreateObjectSymbol(), null);
                else if (f == "*")
                    visited = FullBFS(visited, untilType, ad.Mapping);
                else
                    visited = PtgPointsTo(visited, new Field(f, ISlicerSymbol.CreateObjectSymbol()), ISlicerSymbol.CreateObjectSymbol(), null);
            }

            if (ofType.Count > 0)
                visited = SolverUtils.CreateReferenceComparedPTGHashSet(visited.Where(x => !Configuration.TypesOptimization || Compatibles(x, ofType, ad.Mapping)));
            if (ofKind.Count > 0)
                visited = SolverUtils.CreateReferenceComparedPTGHashSet(visited.Where(x => !Configuration.TypesOptimization || ofKind.Contains(x.Kind)));

            return visited;
        }

        ISet<PtgVertex> FullBFS(ISet<PtgVertex> vertices, List<ETType> untilType, IDictionary<ETType, ISlicerSymbol> mapping)
        {
            if (Configuration.TypesOptimization && untilType != null && untilType.Count > 0)
                return FullBFS_UntilType(vertices, untilType, mapping);

            var visited = SolverUtils.CreateReferenceComparedPTGHashSet();
            FullBFS(vertices, visited);
            return visited;
        }

        ISet<PtgVertex> FullBFS_UntilType(ISet<PtgVertex> vertices, List<ETType> untilType, IDictionary<ETType, ISlicerSymbol> mapping)
        {
            var startTime = DateTime.Now;
            cannot_fullbfs_ut++;

            var visited = SolverUtils.CreateReferenceComparedPTGHashSet();
            var toVisit = SolverUtils.CreateReferenceComparedPTGHashSet();
            var toVisit_ofType = SolverUtils.CreateReferenceComparedPTGHashSet();
            ISet<PtgVertex> closure, to = null;

            foreach (var v in vertices)
            {
                if (visited.Contains(v))
                    continue;

                closure = SolverUtils.CreateReferenceComparedPTGHashSet();
                to = SolverUtils.CreateReferenceComparedPTGHashSet(v.SigmaVertex.Select(x => x.Find()));
                LambdaClosure(to, closure);
                closure.UnionWith(to);

                foreach (var t in closure)
                {
                    if (Compatibles(t, untilType, mapping))
                        toVisit_ofType.Add(t);
                    else
                        toVisit.Add(t);
                }

                foreach (var t2 in v.CommonVertex)
                {
                    closure = SolverUtils.CreateReferenceComparedPTGHashSet();
                    to = SolverUtils.CreateReferenceComparedPTGHashSet(t2.Value.Select(x => x.Find()));
                    LambdaClosure(to, closure);
                    closure.UnionWith(to);

                    foreach (var t in closure)
                        if (Compatibles(t, untilType, mapping))
                            toVisit_ofType.Add(t);
                        else
                            toVisit.Add(t);
                }
            }

            while (toVisit.Count > 0)
            {
                var t = toVisit.First();
                toVisit.Remove(t);
                if (visited.Contains(t))
                    continue;
                visited.Add(t);

                closure = SolverUtils.CreateReferenceComparedPTGHashSet();
                to = SolverUtils.CreateReferenceComparedPTGHashSet(t.SigmaVertex.Select(x => x.Find()));
                LambdaClosure(to, closure);
                closure.UnionWith(to);

                foreach (var v in closure)
                    if (Compatibles(v, untilType, mapping))
                        toVisit_ofType.Add(v);
                    else
                        toVisit.Add(v);

                foreach (var v2 in t.CommonVertex)
                {
                    closure = SolverUtils.CreateReferenceComparedPTGHashSet();
                    to = SolverUtils.CreateReferenceComparedPTGHashSet(v2.Value.Select(x => x.Find()));
                    LambdaClosure(to, closure);
                    closure.UnionWith(to);

                    foreach (var v in closure)
                        if (Compatibles(v, untilType, mapping))
                            toVisit_ofType.Add(v);
                        else
                            toVisit.Add(v);
                }
            }

            while (toVisit_ofType.Count > 0)
            {
                var t = toVisit_ofType.First();
                toVisit_ofType.Remove(t);
                if (visited.Contains(t))
                    continue;
                visited.Add(t);

                closure = SolverUtils.CreateReferenceComparedPTGHashSet();
                to = SolverUtils.CreateReferenceComparedPTGHashSet(t.SigmaVertex.Select(x => x.Find()));
                LambdaClosure(to, closure);
                closure.UnionWith(to);

                foreach (var v in closure)
                    if (Compatibles(v, untilType, mapping))
                        toVisit_ofType.Add(v);

                foreach (var v2 in t.CommonVertex)
                {
                    closure = SolverUtils.CreateReferenceComparedPTGHashSet();
                    to = SolverUtils.CreateReferenceComparedPTGHashSet(v2.Value.Select(x => x.Find()));
                    LambdaClosure(to, closure);
                    closure.UnionWith(to);

                    foreach (var v in closure)
                        if (Compatibles(v, untilType, mapping))
                            toVisit_ofType.Add(v);
                }
            }

            tannot_fullbfs_ut = tannot_fullbfs_ut.Add(DateTime.Now.Subtract(startTime));
            return visited;
        }
        #endregion

        #region Speed
        IDictionary<ISlicerSymbol, PtgVertex> UnionByType(ISet<PtgVertex> currentSet)
        {
            var toReturn = new Dictionary<ISlicerSymbol, PtgVertex>();
            foreach(var v in currentSet)
            {
                if (!toReturn.ContainsKey(v.Type))
                    toReturn.Add(v.Type, v);
                else
                {
                    // TODO: CHEQUEAR XXX NO ESTOY SEGURO
                    if (((v.Find() != toReturn[v.Type]) && (!StaticMode || StaticModeEnabledByCallbackOrRecursion || !StaticModeStack.Peek().Contains(v))) && CheckEqualsSet)
                        SetNotConverged(v.LoopDepthLevel);

                    toReturn[v.Type].Union(v, currentSet);
                }
            }
            return toReturn;
        }

        void LambdaClosure(ISet<PtgVertex> vertices, ISet<PtgVertex> visited)
        {
            var st = DateTime.Now;
            var toVisit = SolverUtils.CreateReferenceComparedPTGHashSet();

            foreach (var v in vertices)
            {
                if (visited.Contains(v))
                    continue;

                foreach (var t in v.LambdaVertex.Where(x => x.Find() != v))
                    toVisit.Add(t.Find());
            }

            while (toVisit.Count > 0)
            {
                var t = toVisit.First();
                toVisit.Remove(t);
                if (visited.Contains(t))
                    continue;
                visited.Add(t);

                foreach (var v in t.LambdaVertex.Where(x => x.Find() != t))
                    toVisit.Add(v.Find());
            }
            tlambda_closure = tlambda_closure.Add(DateTime.Now.Subtract(st));
        }

        public override void FullBFS(ISet<PtgVertex> vertices, ISet<PtgVertex> visited)
        {
            var st = DateTime.Now;
            var toVisit = SolverUtils.CreateReferenceComparedPTGHashSet();

            foreach (var v in vertices)
            {
                if (visited.Contains(v))
                    continue;

                foreach (var t in v.LambdaVertex)
                    toVisit.Add(t.Find());
                foreach (var t in v.SigmaVertex)
                    toVisit.Add(t.Find());
                foreach (var t2 in v.CommonVertex)
                    foreach (var t in t2.Value)
                        toVisit.Add(t.Find());
            }

            while (toVisit.Count > 0)
            {
                var t = toVisit.First();
                toVisit.Remove(t);
                if (visited.Contains(t))
                    continue;
                visited.Add(t);

                foreach (var v in t.LambdaVertex)
                    toVisit.Add(v.Find());
                foreach (var v in t.SigmaVertex)
                    toVisit.Add(v.Find());
                foreach (var v2 in t.CommonVertex)
                    foreach (var v in v2.Value)
                        toVisit.Add(v.Find());
            }

            thavoc_fullbfs = thavoc_fullbfs.Add(DateTime.Now.Subtract(st));
        }

        /// <summary>
        /// Obtenemos los vértices del PTG que corresponden a cada término y en la segunda parte de la tupla todos los alcanzables
        /// </summary>
        Tuple<ISet<PtgVertex>, ISet<PtgVertex>> NodesReachableFrom(Term[] terms)
        {
            var reachableVertexs = new Tuple<ISet<PtgVertex>, ISet<PtgVertex>>
                (SolverUtils.CreateReferenceComparedPTGHashSet(), SolverUtils.CreateReferenceComparedPTGHashSet());

            // Se devuelven 2 conjuntos porque no es lo mismo pertencer al term, pertenecer al reachable
            foreach (var term in terms)
            {
                var initialTermNodes = aPt(term);

                // XXX: HOY PINCHA CUANDO: A UN STRING LE QUERÉS SACAR UN CARACTER: "hola"[0]
                if (initialTermNodes != null)
                {
                    FullBFS(initialTermNodes, reachableVertexs.Item2);

                    if (term.ReferencedTerm != null && term.ReferencedTerm.IsRef)
                        foreach (var t in initialTermNodes)
                            reachableVertexs.Item2.Add(t);
                    else
                        foreach (var t in initialTermNodes)
                            reachableVertexs.Item1.Add(t);
                }
            }

            return reachableVertexs;
        }

        /// <summary>
        /// Dado un conjunto de vértices, devuelve los vértices alcanzables que corresponden al field pasado como parámetro
        /// Complejidad temporal: Muy alta. Especialmente por las clausuras lambda de más. Clausurando menos calcularemos la complejidad correctamente.
        /// Complejdiad espacial: O(vértices*alcanzables por lambda)
        /// </summary>
        ISet<PtgVertex> PtgPointsTo(ISet<PtgVertex> vertices, Field edgeName, ISlicerSymbol currentSymbol, ISet<uint> lastDefinitions)
        {
            // 1. Instanciamos la variable "acum"
            var targets = SolverUtils.CreateReferenceComparedPTGHashSet();

            // 2. Por cada vértices obtenemos los ejes sigma o con nuestro término. Luego nos quedamos con el target y clausuramos por lambda
            foreach (var vertex in vertices)
            {
                if (edgeName.FieldType == FieldType.Sigma)
                {
                    foreach (var t in vertex.CommonVertex.SelectMany(x => x.Value).Where(x => !Configuration.TypesOptimization || (currentSymbol != null && Compatibles(x, currentSymbol))))
                        targets.Add(t);

                    if (lastDefinitions != null)
                        lastDefinitions.UnionWith(vertex.FieldsLastDef.SelectMany(x => x.Value));
                }
                else
                {
                    if (vertex.CommonVertex.ContainsKey(edgeName.ToString()))
                        foreach (var t in vertex.CommonVertex[edgeName.ToString()].Where(x => !Configuration.TypesOptimization || (currentSymbol != null && Compatibles(x, currentSymbol))))
                        {
                            targets.Add(t);

                            if (lastDefinitions != null)
                                lastDefinitions.UnionWith(vertex.FieldsLastDef[new Field(edgeName.ToString(), null)]);
                        }
                }

                if (!vertex.SigmaExcludedProperties.Contains(edgeName))
                    foreach (var t in vertex.SigmaVertex.Where(x => !Configuration.TypesOptimization || (currentSymbol != null && Compatibles(x, currentSymbol))))
                        targets.Add(t);
            }

            // Se devuelven los representantes (nunca los nodos originales puros)
            var lambdaSet = SolverUtils.CreateReferenceComparedPTGHashSet(targets.Select(x => x.Find()));
            var closure = SolverUtils.CreateReferenceComparedPTGHashSet();
            LambdaClosure(lambdaSet, closure); // TODOMIX
            lambdaSet.UnionWith(closure);

            return lambdaSet;
        }

        /// <summary>
        /// Devuelve el conjunto de vértices que pertenecen a un término
        /// Clausura por lambda
        /// </summary>
        public override ISet<PtgVertex> aPt(ScopeContainer scope, Term term, ISet<uint> lastDefinitions = null)
        {
            capt++;
            var st = DateTime.Now;
            // 1. Obtenemos los nodos del grafo a los cuales hace referencia nuestro término
            var ev = EntryVertexForTerm(scope, term, lastDefinitions);
            if (ev == null)
                return null;

            // 2. Obtenemos los representantes (equivale a clausuar por lambda cuando hay ida/vuelta)
            var init = SolverUtils.CreateReferenceComparedPTGHashSet(ev.VertexSet.Select(x => x.Find()));
            var closure = SolverUtils.CreateReferenceComparedPTGHashSet();
            LambdaClosure(init, closure);
            closure.UnionWith(init);

            // CASO A: si el término tiene un solo campo, lo clausuramos por lambda y listo
            if (term.IsVar)
            {
                tannot_apt = tannot_apt.Add(DateTime.Now.Subtract(st));
                //capt_dg.Add(closure.Count);
                return closure;
            }

            // CASO B: si el término tiene más de un campo nos vamos "moviendo" por cada parte
            var currentSet = closure;
            for (var i = 1; i < term.Parts.Count; i++)
                currentSet = PtgPointsTo(currentSet, term[i], term[i].Symbol, lastDefinitions);

            // Se retorna el último current set, que es el conjunto correspondiente al término que recibimos (a la última parte)
            tapt = tapt.Add(DateTime.Now.Subtract(st));
            //capt_dg.Add(closure.Count);
            return currentSet;
        }

        PtgVertex CreateHub(Term resultParam, uint lastDef, ReturnVertexType returnVertexType, Stmt invocationPoint)
        {
            StaticModeKey key = "";
            PtgVertex hub = null;
            if (StaticMode)
            {
                key = GetTermKey(invocationPoint);
                // Se supone que es región si es IsCallbackAlloc y viceversa, TODO: se podría poner un assert
                if (returnVertexType == ReturnVertexType.ReturnRegion && resultParam.IsCallbackAlloc)
                    MaySetVertexValue(ref hub, StaticModeCallbackRegion, key);
                else
                    MaySetVertexValue(ref hub, StaticModeHUB, key);
            }

            // Creamos el HUB con un eje Sigma hacia si mismo
            if (hub == null)
            {
                hub = new PtgVertex(Configuration, "Hub " + (HubUniqueId++), false, ISlicerSymbol.CreateObjectSymbol(), false, VertexType.Hub, PtgVertex.DefaultKind, false, StaticModeStack.Count, key);
                //hub.AddVertex(EdgeType.Sigma, hub);
                //hub.FieldsLastDef[Field.SIGMA_FIELD] = new HashSet<uint>() { lastDef };

                if (StaticMode)
                {
                    if (returnVertexType == ReturnVertexType.ReturnRegion && resultParam.IsCallbackAlloc)
                        StaticModeCallbackRegion[key] = hub;
                    else
                        StaticModeHUB[key] = hub;
                    if (StaticModeStack.Count > 0)
                        StaticModeStack.Peek().Add(hub);
                }

                // Con HUBS nuevos ya el PTG se modificó (no existía previamente)
                if (CheckEqualsSet && (!StaticMode || StaticModeEnabledByCallbackOrRecursion || !StaticModeStack.Peek().Contains(hub)))
                    SetNotConverged(hub.LoopDepthLevel);
            }
            else
            {
                var added = hub.FieldsLastDef[Field.SIGMA_FIELD].Add(lastDef);

                if (CheckEqualsSet && (!StaticMode || StaticModeEnabledByCallbackOrRecursion || !StaticModeStack.Peek().Contains(hub)) && added)
                    SetNotConverged(hub.LoopDepthLevel);
            }

            if (resultParam != null)
            {
                if (resultParam.IsScalar)
                    LastDef_Set(resultParam, lastDef);
                else if (returnVertexType == ReturnVertexType.ReturnRegion)
                    SaveVertex(resultParam, hub, lastDef);
                else
                {
                    PtgVertex vertex = null;
                    if (StaticMode)
                    {
                        key = GetTermKey(invocationPoint);
                        MaySetVertexValue(ref vertex, StaticModeVertex, key);
                    }

                    var newVertex = false;
                    if (vertex == null)
                    {
                        vertex = new PtgVertex(Configuration, resultParam.Last.ToString(), false, resultParam.Last.Symbol,
                        false, VertexType.Common, PtgVertex.DefaultKind, false, StaticModeStack.Count, key);
                        vertex.FieldsLastDef[Field.SIGMA_FIELD] = new HashSet<uint>();

                        if (StaticMode)
                        {
                            StaticModeVertex[key] = vertex;
                            if (StaticModeStack.Count > 0)
                                StaticModeStack.Peek().Add(vertex);
                        }

                        newVertex = true;
                    }

                    var e1_added = vertex.AddVertex((returnVertexType == ReturnVertexType.FreshVar) ? EdgeType.Sigma : EdgeType.Lambda, hub);
                    var e2_added = hub.AddVertex(EdgeType.Lambda, vertex);
                    var e3_added = vertex.FieldsLastDef[Field.SIGMA_FIELD].Add(lastDef);

                    // Si se realizó alguna especie de modificación ya no converge
                    if (((newVertex || e1_added || e3_added) && (!StaticMode || StaticModeEnabledByCallbackOrRecursion || !StaticModeStack.Peek().Contains(vertex))) && (CheckEqualsSet))
                        SetNotConverged(vertex.LoopDepthLevel);

                    if ((e2_added && (!StaticMode || StaticModeEnabledByCallbackOrRecursion || !StaticModeStack.Peek().Contains(hub))) && (CheckEqualsSet))
                        SetNotConverged(hub.LoopDepthLevel);

                    SaveVertex(resultParam, vertex, lastDef);
                }
            }

            return hub;
        }

        void SaveVertex(Term resultParam, PtgVertex vertex, uint lastDef)
        {
            // Se chequea modificación del PTG del otro lado
            LastDef_Set(resultParam, lastDef);

            var currentScope = ScopeForTerm(resultParam);

            var set = SolverUtils.CreateReferenceComparedPTGHashSet();
            set.Add(vertex);

            if (resultParam.IsVar)
            {
                // No hace falta chequear acá modificación del PTG porque si era vértice nuevo ya fue verificado, y si existía esto no cambia porque overraidea
                currentScope.OverrideEntryPointValues(resultParam.First.ToString(), set, vertex.Type);
                return;
            }
            var lhsVertices = aPt(currentScope, resultParam.DiscardLast());

            // Agregamos por cada vértice que nos apunta del lado izquierdo un eje hacia cada vértice del lado derecho
            // Parte strong
            if (lhsVertices.Count == 1 && lhsVertices.Single().VertexType != VertexType.Hub && resultParam.Last.FieldType != FieldType.Sigma && !lhsVertices.Single().Immutable && !lhsVertices.Single().Multiple)
                lhsVertices.Single().RemoveVertex(resultParam.Last.ToString());

            foreach (var lhsVertex in lhsVertices)
                lhsVertex.AddVertex(EdgeType.Common, vertex, resultParam.Last.ToString());
        }
        #endregion
        #endregion
    }
}
