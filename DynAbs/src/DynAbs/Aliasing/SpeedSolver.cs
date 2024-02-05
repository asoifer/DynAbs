using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using StaticModeKey = System.String;

namespace DynAbs
{
    public class SpeedSolver : AbstractSolver, IAliasingSolver
    {
        #region Properties
        public TimeSpan tapt = new TimeSpan(0);
        public TimeSpan tlambda_closure = new TimeSpan(0);
        public TimeSpan thavoc_uses = new TimeSpan(0);
        public TimeSpan thavoc_fullbfs = new TimeSpan(0);
        public TimeSpan thavoc_add_dg = new TimeSpan(0);
        public TimeSpan thavoc_item1 = new TimeSpan(0);
        public TimeSpan thavoc_item2 = new TimeSpan(0);

        //public List<int> capt_dg = new List<int>();
        public List<int> chavoc_dg = new List<int>();
        #endregion

        #region Public
        public SpeedSolver(UserSliceConfiguration userSliceConfiguration)
        {
            Configuration = userSliceConfiguration;
        }

        public void Havoc_EjesInvertidos(Term receiver, List<Term> arguments, Term returnValue, Func<Stmt, ISet<uint>, uint> GetDGNode, AnnotationWithData ad, Stmt invocationPoint)
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

            thavoc_uses = thavoc_uses.Add(DateTime.Now.Subtract(st));
            st = DateTime.Now;

            var lastDef = GetDGNode(invocationPoint, uses);
            chavoc_dg.Add(uses.Count);

            thavoc_add_dg = thavoc_add_dg.Add(DateTime.Now.Subtract(st));

            foreach (var p in allArgs.Where(x => x.ReferencedTerm != null && x.ReferencedTerm.IsOutOrRef))
                LastDef_Set(p.ReferencedTerm, lastDef);

            // Obtenemos el HUB
            var hub = CreateHub(returnValue, lastDef, ReturnVertexType.Default, invocationPoint);

            st = DateTime.Now;
            foreach (var reachable in reachableNodes.Item2)
            {
                // TODO: CHEQUEAR XXX NO ESTOY SEGURO
                if (((reachable.Find() != hub) && (!StaticMode || StaticModeEnabledByCallbackOrRecursion || !StaticModeStack.Peek().Contains(reachable))) && CheckEqualsSet)
                    SetNotConverged(reachable.LoopDepthLevel);

                hub.Union(reachable, reachableNodes.Item2);
            }

            thavoc_item2 = thavoc_item2.Add(DateTime.Now.Subtract(st));
            st = DateTime.Now;

            // Parámetros
            var parent = hub.Find();
            foreach (var reachable in reachableNodes.Item1)
            {
                if (!reachableNodes.Item2.Contains(reachable))
                {
                    var e1_added = parent.AddVertex(EdgeType.Lambda, reachable);
                    var e2_added = reachable.AddVertex(EdgeType.Sigma, parent);

                    if (!reachable.FieldsLastDef.ContainsKey(Field.SIGMA_FIELD))
                        reachable.FieldsLastDef[Field.SIGMA_FIELD] = new HashSet<uint>();
                    var ld_added = reachable.FieldsLastDef[Field.SIGMA_FIELD].Add(lastDef);

                    if (((e2_added || ld_added) && (!StaticMode || StaticModeEnabledByCallbackOrRecursion || !StaticModeStack.Peek().Contains(reachable))) && CheckEqualsSet)
                        SetNotConverged(reachable.LoopDepthLevel);

                    if ((e1_added && (!StaticMode || StaticModeEnabledByCallbackOrRecursion || !StaticModeStack.Peek().Contains(parent))) && CheckEqualsSet)
                        SetNotConverged(parent.LoopDepthLevel);

                    reachable.SigmaExcludedProperties = null;
                }
            }

            thavoc_item1 = thavoc_item1.Add(DateTime.Now.Subtract(st));

            // Actualizamos el SIGMA de la componente conexa
            if (!Globals.optimize_union_find_set && Globals.clean_last_def && parent != hub)
            {
                parent.AddVertex(EdgeType.Sigma, parent);
                parent.FieldsLastDef = new Dictionary<Field, ISet<uint>>();
                parent.FieldsLastDef[Field.SIGMA_FIELD] = new HashSet<uint>();
                parent.FieldsLastDef[Field.SIGMA_FIELD].Add(lastDef);
            }
        }

        public void Havoc(Term receiver, List<Term> arguments, Term returnValue, Func<Stmt, ISet<uint>, uint> GetDGNode, AnnotationWithData ad, Stmt invocationPoint)
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

            thavoc_uses = thavoc_uses.Add(DateTime.Now.Subtract(st));
            st = DateTime.Now;

            var lastDef = GetDGNode(invocationPoint, uses);
            chavoc_dg.Add(uses.Count);

            thavoc_add_dg = thavoc_add_dg.Add(DateTime.Now.Subtract(st));
            
            foreach (var p in allArgs.Where(x => x.ReferencedTerm != null && x.ReferencedTerm.IsOutOrRef))
                LastDef_Set(p.ReferencedTerm, lastDef);

            // Obtenemos el HUB
            var hub = CreateHub(returnValue, lastDef, ReturnVertexType.Default, invocationPoint);

            st = DateTime.Now;

            // Parámetros
            foreach (var reachable in reachableNodes.Item1)
            {
                if (!reachableNodes.Item2.Contains(reachable))
                {
                    var e1_added = hub.AddVertex(EdgeType.Lambda, reachable);
                    var e2_added = reachable.AddVertex(EdgeType.Sigma, hub);
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
                        SetNotConverged(hub.LoopDepthLevel);

                    reachable.SigmaExcludedProperties = null;
                }
            }

            thavoc_item1 = thavoc_item1.Add(DateTime.Now.Subtract(st));
            st = DateTime.Now;

            foreach(var reachable in reachableNodes.Item2)
            {
                // TODO: CHEQUEAR XXX NO ESTOY SEGURO
                if (((reachable.Find() != hub) && (!StaticMode || StaticModeEnabledByCallbackOrRecursion || !StaticModeStack.Peek().Contains(reachable))) && CheckEqualsSet)
                    SetNotConverged(reachable.LoopDepthLevel);

                hub.Union(reachable, reachableNodes.Item2);
            }

            thavoc_item2 = thavoc_item2.Add(DateTime.Now.Subtract(st));

            // Actualizamos el SIGMA de la componente conexa
            if (!Globals.optimize_union_find_set && Globals.clean_last_def) 
            { 
                var parent = hub.Find();
                if (parent != hub)
                {
                    parent.AddVertex(EdgeType.Sigma, parent);
                    parent.FieldsLastDef = new Dictionary<Field, ISet<uint>>();
                    parent.FieldsLastDef[Field.SIGMA_FIELD] = new HashSet<uint>();
                    parent.FieldsLastDef[Field.SIGMA_FIELD].Add(lastDef);
                }
            }
        }

        public override void SaveResults(string graphEvolutionFile, string internalProfileFile)
        {
            if (!string.IsNullOrWhiteSpace(graphEvolutionFile))
            {
                var sb = new StringBuilder();
                sb.AppendLine("HAVOC USES");
                for (var i = 0; i < chavoc_dg.Count; i++)
                    sb.AppendLine(chavoc_dg[i].ToString());
                //sb.AppendLine("APT COUNT");
                //for (var i = 0; i < capt_dg.Count; i++)
                //    sb.AppendLine(capt_dg[i].ToString());
                System.IO.File.WriteAllText(graphEvolutionFile, sb.ToString());
            }

            if (!string.IsNullOrWhiteSpace(internalProfileFile))
            {
                var sb2 = new StringBuilder();
                sb2.AppendLine(string.Format("APT SECS: {0}", tapt.TotalSeconds));
                sb2.AppendLine(string.Format("LAMBDA SECS: {0}", tlambda_closure.TotalSeconds));
                sb2.AppendLine(string.Format("HAVOC USES SECS: {0}", thavoc_uses.TotalSeconds));
                sb2.AppendLine(string.Format("HAVOC BFS SECS: {0}", thavoc_fullbfs.TotalSeconds));
                sb2.AppendLine(string.Format("HAVOC DG SECS: {0}", thavoc_add_dg.TotalSeconds));
                sb2.AppendLine(string.Format("HAVOC ITEM 1 SECS: {0}", thavoc_item1.TotalSeconds));
                sb2.AppendLine(string.Format("HAVOC ITEM 2 SECS: {0}", thavoc_item2.TotalSeconds));

                //sb2.AppendLine(string.Format("TYPES: GET MIN: {0}", TypesUtils.ttipes_get_min));
                //sb2.AppendLine(string.Format("TYPES: GET FIELD SYMBOL: {0}", TypesUtils.ttipes_get_field_symbol));
                //sb2.AppendLine(string.Format("TYPES: COMPATIBLE: {0}", TypesUtils.ttipes_compatibles));
                //sb2.AppendLine(string.Format("CANT. COMPATIBLES: {0}", cantidad_compatibles));
                //sb2.AppendLine(string.Format("CANT. COMPATIBLES VUELTA: {0}", cantidad_compatibles_vuelta));
                //sb2.AppendLine(string.Format("CANT. NO COMPATIBLES: {0}", cantidad_no_compatibles));
                //sb2.AppendLine(string.Format("CANT. HUBS: {0}", cantidad_hubs));

                System.IO.File.WriteAllText(internalProfileFile, sb2.ToString());
            }
        }
        #endregion

        #region Private
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
                (SolverUtils.CreateReferenceComparedPTGHashSet(),  SolverUtils.CreateReferenceComparedPTGHashSet());

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
                    foreach (var t in vertex.CommonVertex.SelectMany(x => x.Value))
                        targets.Add(t);

                    if (lastDefinitions != null)
                        lastDefinitions.UnionWith(vertex.FieldsLastDef.SelectMany(x => x.Value));
                }
                else
                {
                    if (vertex.CommonVertex.ContainsKey(edgeName.ToString()))
                        foreach (var t in vertex.CommonVertex[edgeName.ToString()])
                        {
                            targets.Add(t);

                            if (lastDefinitions != null)
                                lastDefinitions.UnionWith(vertex.FieldsLastDef[new Field(edgeName.ToString(), null)]);
                        }
                }

                if (!vertex.SigmaExcludedProperties.Contains(edgeName))
                    foreach (var t in vertex.SigmaVertex)
                        targets.Add(t);
            }

            // Se devuelven los representantes (nunca los nodos originales puros)
            var lambdaSet = SolverUtils.CreateReferenceComparedPTGHashSet(targets.Select(x => x.Find()));
            var closure = SolverUtils.CreateReferenceComparedPTGHashSet();
            LambdaClosure(lambdaSet, closure);
            lambdaSet.UnionWith(closure);

            return lambdaSet;
        }

        /// <summary>
        /// Devuelve el conjunto de vértices que pertenecen a un término
        /// Clausura por lambda
        /// </summary>
        public override ISet<PtgVertex> aPt(ScopeContainer scope, Term term, ISet<uint> lastDefinitions = null)
        {
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
                tapt = tapt.Add(DateTime.Now.Subtract(st));
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
                hub.AddVertex(EdgeType.Sigma, hub);
                hub.FieldsLastDef[Field.SIGMA_FIELD] = new HashSet<uint>() { lastDef };

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
    }
}
