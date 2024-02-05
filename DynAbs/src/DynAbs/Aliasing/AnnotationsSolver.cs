using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DynAbs.Summaries;
using StaticModeKey = System.String;

namespace DynAbs
{
    public class AnnotationsSolver : AbstractSolver, IAliasingSolver
    {
        #region Properties 
        public double cfull_bfs = 0;
        public double tfull_bfs = 0;
        public List<int> nfull_bfs = new List<int>();

        public double cfull_bfs_t = 0;
        public double tfull_bfs_t = 0;
        public List<int> nfull_bfs_t = new List<int>();

        public double capt = 0;
        public double tapt = 0;
        #endregion

        #region Public
        public AnnotationsSolver(UserSliceConfiguration userSliceConfiguration)
        {
            Configuration = userSliceConfiguration;
        }

        public void Havoc(Term receiver, List<Term> arguments, Term returnValue, 
            Func<Stmt, ISet<uint>, uint> GetDGNode, AnnotationWithData ad, Stmt invocationPoint)
        {
            var reads = GetR(receiver, arguments, returnValue, null, ad);
            var dgv = GetDGNode(invocationPoint, reads);
            var otherNodes = GetOtherNodes(dgv, ad, invocationPoint);
            SetReturnValue(receiver, arguments, otherNodes, dgv, returnValue, ad, invocationPoint);
            SetW(dgv, receiver, arguments, returnValue, otherNodes, ad);
            SetCN(dgv, receiver, arguments, returnValue, otherNodes, ad);
        }

        public override void SaveResults(string graphEvolutionFile, string internalProfileFile)
        {
            if (!string.IsNullOrWhiteSpace(graphEvolutionFile))
            {
                var sb = new StringBuilder();
                sb.AppendLine("Full BFS:");
                foreach (var n in nfull_bfs)
                    sb.AppendLine(n.ToString());
                sb.AppendLine("Full BFS UntilType:");
                foreach (var n in nfull_bfs_t)
                    sb.AppendLine(n.ToString());
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

                sb2.AppendLine("NEW:");
                sb2.AppendLine(string.Format("COUNT APT : {0}", capt));
                sb2.AppendLine(string.Format("TIME APT: {0}", tapt));
                sb2.AppendLine(string.Format("COUNT FULL: {0}", cfull_bfs));
                sb2.AppendLine(string.Format("TIME FULL: {0}", tfull_bfs));
                sb2.AppendLine(string.Format("COUNT FULL UNTIL TYPE: {0}", cfull_bfs_t));
                sb2.AppendLine(string.Format("TIME FULL UNTIL TYPE: {0}", tfull_bfs_t));

                System.IO.File.WriteAllText(internalProfileFile, sb2.ToString());
            }
        }
        #endregion

        #region Private
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

        ISet<PtgVertex> aPt(Term receiver, List<Term> arguments, Term returnValue, List<PtgVertex> otherNodes, List<ET> met, AnnotationWithData ad)
        {
            var startTime = DateTime.Now;
            capt++;

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

            tapt += DateTime.Now.Subtract(startTime).TotalMilliseconds;
            return returnSet;
        }

        ISet<PtgVertex> aPt(ISet<PtgVertex> toVisitParams, List<string> fields, List<ETType> untilType, List<ETType> ofType, List<string> ofKind, AnnotationWithData ad)
        {
            var visited = SolverUtils.CreateReferenceComparedPTGHashSet(toVisitParams);
            foreach (var f in fields)
            {
                if (f == "?")
                    visited = PtgPointsTo(visited, Field.SIGMA_FIELD);
                else if (f == "*")
                    visited = FullBFS(visited, untilType, ad.Mapping);
                else
                    visited = PtgPointsTo(visited, new Field(f, ISlicerSymbol.CreateObjectSymbol()));
            }

            if (ofType.Count > 0)
                visited = SolverUtils.CreateReferenceComparedPTGHashSet(visited.Where(x => Compatibles(x, ofType, ad.Mapping)));
            if (ofKind.Count > 0)
                visited = SolverUtils.CreateReferenceComparedPTGHashSet(visited.Where(x => ofKind.Contains(x.Kind)));

            return visited;
        }

        ISet<PtgVertex> FullBFS_UntilType(ISet<PtgVertex> vertices, List<ETType> untilType, IDictionary<ETType, ISlicerSymbol> mapping)
        {
            var startTime = DateTime.Now;
            cfull_bfs_t++;

            var visited = SolverUtils.CreateReferenceComparedPTGHashSet();
            var toVisit = SolverUtils.CreateReferenceComparedPTGHashSet();
            var toVisit_ofType = SolverUtils.CreateReferenceComparedPTGHashSet();

            foreach (var v in vertices)
            {
                if (visited.Contains(v))
                    continue;

                foreach (var t in v.SigmaVertex)
                    if (Compatibles(t, untilType, mapping))
                        toVisit_ofType.Add(t);
                    else
                        toVisit.Add(t);
                foreach (var t2 in v.CommonVertex)
                    foreach (var t in t2.Value)
                        if (Compatibles(t, untilType, mapping))
                            toVisit_ofType.Add(t);
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

                foreach (var v in t.SigmaVertex)
                    if (Compatibles(v, untilType, mapping))
                        toVisit_ofType.Add(v);
                    else
                        toVisit.Add(v);
                foreach (var v2 in t.CommonVertex)
                    foreach (var v in v2.Value)
                        if (Compatibles(v, untilType, mapping))
                            toVisit_ofType.Add(v);
                        else
                            toVisit.Add(v);
            }

            while (toVisit_ofType.Count > 0)
            {
                var t = toVisit_ofType.First();
                toVisit_ofType.Remove(t);
                if (visited.Contains(t))
                    continue;
                visited.Add(t);

                foreach (var v in t.SigmaVertex)
                    if (Compatibles(v, untilType, mapping))
                        toVisit_ofType.Add(v);
                foreach (var v2 in t.CommonVertex)
                    foreach (var v in v2.Value)
                        if (Compatibles(v, untilType, mapping))
                            toVisit_ofType.Add(v);
            }

            tfull_bfs_t += DateTime.Now.Subtract(startTime).TotalMilliseconds;
            nfull_bfs_t.Add(visited.Count);
            return visited;
        }

        ISet<PtgVertex> FullBFS(ISet<PtgVertex> vertices, List<ETType> untilType, IDictionary<ETType, ISlicerSymbol> mapping)
        {
            if (untilType != null && untilType.Count > 0)
                return FullBFS_UntilType(vertices, untilType, mapping);

            var startTime = DateTime.Now;
            cfull_bfs++;

            var visited = SolverUtils.CreateReferenceComparedPTGHashSet();
            var toVisit = SolverUtils.CreateReferenceComparedPTGHashSet();

            foreach (var v in vertices)
            {
                if (visited.Contains(v))
                    continue;

                foreach (var t in v.SigmaVertex)
                    toVisit.Add(t);
                foreach (var t2 in v.CommonVertex)
                    foreach (var t in t2.Value)
                        toVisit.Add(t);
            }

            while (toVisit.Count > 0)
            {
                var t = toVisit.First();
                toVisit.Remove(t);
                if (visited.Contains(t))
                    continue;
                visited.Add(t);
                
                foreach (var v in t.SigmaVertex)
                    toVisit.Add(v);
                foreach (var v2 in t.CommonVertex)
                    foreach (var v in v2.Value)
                        toVisit.Add(v);
            }

            tfull_bfs += DateTime.Now.Subtract(startTime).TotalMilliseconds;
            nfull_bfs.Add(visited.Count);
            return visited;
        }

        public override void FullBFS(ISet<PtgVertex> vertices, ISet<PtgVertex> visited)
        {
            var toVisit = SolverUtils.CreateReferenceComparedPTGHashSet();

            foreach (var v in vertices)
            {
                if (visited.Contains(v))
                    continue;

                foreach (var t in v.SigmaVertex)
                    toVisit.Add(t);
                foreach (var t2 in v.CommonVertex)
                    foreach (var t in t2.Value)
                        toVisit.Add(t);
            }

            while (toVisit.Count > 0)
            {
                var t = toVisit.First();
                toVisit.Remove(t);
                if (visited.Contains(t))
                    continue;
                visited.Add(t);

                foreach (var v in t.SigmaVertex)
                    toVisit.Add(v);
                foreach (var v2 in t.CommonVertex)
                    foreach (var v in v2.Value)
                        toVisit.Add(v);
            }
        }

        ISet<PtgVertex> PtgPointsTo(ISet<PtgVertex> vertices, Field edgeName)
        {
            // 1. Instanciamos la variable "acum"
            var targets = SolverUtils.CreateReferenceComparedPTGHashSet();

            // 2. Por cada vértices obtenemos los ejes sigma o con nuestro término. Luego nos quedamos con el target 
            foreach (var vertex in vertices)
            {
                if (edgeName.FieldType == FieldType.Sigma)
                {
                    foreach (var t in vertex.CommonVertex.SelectMany(x => x.Value)/*.Where(x => Compatibles(x, currentSymbol))*/)
                        targets.Add(t);

                    //if (lastDefinitions != null)
                    //    lastDefinitions.UnionWith(vertex.FieldsLastDef.SelectMany(x => x.Value));
                }
                else
                {
                    if (vertex.CommonVertex.ContainsKey(edgeName.ToString()))
                        foreach (var t in vertex.CommonVertex[edgeName.ToString()]/*.Where(x => Compatibles(x, currentSymbol))*/)
                        {
                            targets.Add(t);

                            //if (lastDefinitions != null)
                            //    lastDefinitions.UnionWith(vertex.FieldsLastDef[new Field(edgeName.ToString(), null)]);
                        }
                }

                if (!vertex.SigmaExcludedProperties.Contains(edgeName))
                    foreach (var t in vertex.SigmaVertex)
                        targets.Add(t);
            }

            return targets;
        }

        /// <summary>
        /// Dado un conjunto de vértices, devuelve los vértices alcanzables que corresponden al field pasado como parámetro
        /// </summary>
        ISet<PtgVertex> PtgPointsTo(ISet<PtgVertex> vertices, Field edgeName, ISlicerSymbol currentSymbol, ISet<uint> lastDefinitions)
        {
            // 1. Instanciamos la variable "acum"
            var targets = SolverUtils.CreateReferenceComparedPTGHashSet();

            // 2. Por cada vértices obtenemos los ejes sigma o con nuestro término. Luego nos quedamos con el target
            foreach (var vertex in vertices)
            {
                if (edgeName.FieldType == FieldType.Sigma)
                {
                    foreach (var t in vertex.CommonVertex.SelectMany(x => x.Value).Where(x => !Configuration.TypesOptimization || Compatibles(x, currentSymbol)))
                        targets.Add(t);

                    if (lastDefinitions != null)
                        lastDefinitions.UnionWith(vertex.FieldsLastDef.SelectMany(x => x.Value));
                }
                else
                {
                    if (vertex.CommonVertex.ContainsKey(edgeName.ToString()))
                        foreach (var t in vertex.CommonVertex[edgeName.ToString()].Where(x => !Configuration.TypesOptimization || Compatibles(x, currentSymbol)))
                        {
                            targets.Add(t);

                            if (lastDefinitions != null)
                                lastDefinitions.UnionWith(vertex.FieldsLastDef[new Field(edgeName.ToString(), null)]);
                        }
                }

                if (!vertex.SigmaExcludedProperties.Contains(edgeName))
                    foreach (var t in vertex.SigmaVertex.Where(x => !Configuration.TypesOptimization || Compatibles(x, currentSymbol)))
                        targets.Add(t);
            }

            return targets;
        }

        /// <summary>
        /// Devuelve el conjunto de vértices que pertenecen a un término
        /// </summary>
        public override ISet<PtgVertex> aPt(ScopeContainer scope, Term term, ISet<uint> lastDefinitions = null)
        {
            // 1. Obtenemos los nodos del grafo a los cuales hace referencia nuestro término
            var ev = EntryVertexForTerm(scope, term, lastDefinitions);
            if (ev == null)
                return null;

            var tempSet = ev.VertexSet;
            
            // CASO 2: si el término tiene un solo campo, ok
            if (term.IsVar)
                return tempSet;

            // CASO 3: si el término tiene más de un campo nos vamos "moviendo" por cada parte
            var currentSet = tempSet;
            for (var i = 1; i < term.Parts.Count; i++)
                currentSet = PtgPointsTo(currentSet, term[i], term[i].Symbol, lastDefinitions);

            // Se retorna el último current set, que es el conjunto correspondiente al término que recibimos (a la última parte)
            return currentSet;
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
        #endregion
    }
}
