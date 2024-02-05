using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using QuikGraph;
using StaticModeKey = System.String;

namespace DynAbs
{
    public class BasicSolver : AbstractSolver, IAliasingSolver
    {
        #region Properties 
        public TimeSpan tapt = new TimeSpan(0);
        public TimeSpan tlambda_closure = new TimeSpan(0);
        public TimeSpan thavoc_uses = new TimeSpan(0);
        public TimeSpan thavoc_fullbfs = new TimeSpan(0);
        public TimeSpan thavoc_add_dg = new TimeSpan(0);
        public TimeSpan thavoc_item1 = new TimeSpan(0);
        public TimeSpan thavoc_item2 = new TimeSpan(0);
        public List<int> chavoc_dg = new List<int>();
        bool local_compression = false;
        bool global_compression = false;
        #endregion

        #region Public
        public BasicSolver(UserSliceConfiguration userSliceConfiguration)
        {
            Configuration = userSliceConfiguration;
        }

        /// <summary>
        /// Simula invocaciones donde los parámetros comunes no son modificados. Solo el this se modifica, utilizandolos.
        /// Los ejes lambda son solo de IDA hacia lo alcanzable de los parámetros
        /// Solo le agrega un SIGMA desde el THIS hacia el HUB simulando el comportamiento que podrían tener sus propiedades
        /// En parameters también se encuentra el @this
        /// </summary>
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

                    if (!reachable.FieldsLastDef.ContainsKey(Field.SIGMA_FIELD))
                        reachable.FieldsLastDef[Field.SIGMA_FIELD] = new HashSet<uint>();
                    var ld_added = reachable.FieldsLastDef[Field.SIGMA_FIELD].Add(lastDef);

                    if (((e2_added || ld_added) && (!StaticMode || StaticModeEnabledByCallbackOrRecursion || !StaticModeStack.Peek().Contains(reachable))) && CheckEqualsSet)
                        SetNotConverged(reachable.LoopDepthLevel);

                    if ((e1_added && (!StaticMode || StaticModeEnabledByCallbackOrRecursion || !StaticModeStack.Peek().Contains(hub))) && CheckEqualsSet)
                        SetNotConverged(hub.LoopDepthLevel);

                    reachable.SigmaExcludedProperties = null;
                }
            }

            thavoc_item1 = thavoc_item1.Add(DateTime.Now.Subtract(st));
            st = DateTime.Now;

            // Los elementos del 2do grupo tendrán lambda y vuelta, por lo tanto... 
            // Si ambos son compatibles se pueden comprimir, incluso si uno es tipo mínimo y el otro no,
            // Entonces no serán compatibles ida y vuelta... no se agruparán
            // En principio lo hacemos siempre y para todos los common. 
            // Podríamos ver si vale la pena hacerlo siempre porque no es barato...
            // EXTRA: En la convergencia no debería haber compresión de nodos del mismo tipo (el PTG se debe mantener inalterado)
            var compressingVertex = false;
            var l = reachableNodes.Item2.Where(x => !x.Immutable && x != hub && x.VertexType == VertexType.Common).ToList();
            var g = new List<Tuple<PtgVertex, ISet<PtgVertex>>>();
            for (var i = 0; i < l.Count; i++)
            {
                g.Add(new Tuple<PtgVertex, ISet<PtgVertex>>(l[i], SolverUtils.CreateReferenceComparedPTGHashSet()));
                for (var j = i + 1; j < l.Count; j++)
                    if (Compatibles(l[i], l[j].Type) && Compatibles(l[j], l[i].Type))
                    {
                        g[g.Count - 1].Item2.Add(l[j]);
                        if ((!StaticMode || StaticModeEnabledByCallbackOrRecursion || !StaticModeStack.Peek().Contains(l[i]) || !StaticModeStack.Peek().Contains(l[j])) && CheckEqualsSet)
                            SetNotConverged(Math.Min(l[i].LoopDepthLevel, l[j].LoopDepthLevel));
                        l.RemoveAt(j);
                    }
            }
            foreach (var s in g)
                if (s.Item2.Count > 0)
                    foreach (var w in s.Item2)
                    {
                        w.Multiple = true;
                        VertexCompression(w, s.Item1);
                    }

            // LO: Acá en realidad no se comprime, se agregan lambdas ida/vuelta... así que el chequeo se hace del otro lado
            foreach (var reachable in g)
                LambdaCompression(reachable.Item1, hub);

            // LO: Acá si se comprime por lo tanto se altera el PTG
            compressingVertex = false;
            foreach (var reachable in reachableNodes.Item2.Where(x => !x.Immutable && x != hub && x.VertexType == VertexType.Hub))
            {
                if ((!StaticMode || StaticModeEnabledByCallbackOrRecursion || !StaticModeStack.Peek().Contains(reachable) || !StaticModeStack.Peek().Contains(hub)) && CheckEqualsSet)
                    SetNotConverged(reachable.LoopDepthLevel);
                LambdaCompression(reachable, hub);
            }

            thavoc_item2 = thavoc_item2.Add(DateTime.Now.Subtract(st));
        }

        public override void SaveResults(string graphEvolutionFile, string internalProfileFile)
        {
            if (!string.IsNullOrWhiteSpace(graphEvolutionFile))
            {
                var sb = new StringBuilder();
                sb.AppendLine("HAVOC USES");
                for (var i = 0; i < chavoc_dg.Count; i++)
                    sb.AppendLine(chavoc_dg[i].ToString());
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
        public override void FullBFS(ISet<PtgVertex> vertices, ISet<PtgVertex> visited)
        {
            var st = DateTime.Now;
            var toVisit = SolverUtils.CreateReferenceComparedPTGHashSet();

            foreach (var v in vertices)
            {
                if (visited.Contains(v))
                    continue;

                foreach (var t in v.LambdaVertex)
                    toVisit.Add(t);
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

                foreach (var v in t.LambdaVertex)
                    toVisit.Add(v);
                foreach (var v in t.SigmaVertex)
                    toVisit.Add(v);
                foreach (var v2 in t.CommonVertex)
                    foreach (var v in v2.Value)
                        toVisit.Add(v);
            }

            thavoc_fullbfs = thavoc_fullbfs.Add(DateTime.Now.Subtract(st));
        }

        /// <summary>
        /// Devuelve el conjunto de vértices del grafo alcanzables por lambda desde los nodos pasados como parámetro. 
        /// En otras palabras: clausura por lambda
        /// NOTA: el conjunto devuelto NO incluye el conjunto pasado como parámetro
        /// </summary>
        ISet<PtgVertex> LambdaClosure(ISet<PtgVertex> toVisitParams,  ISlicerSymbol currentSymbol)
        {
            var st = DateTime.Now;
            var toVisit = SolverUtils.CreateReferenceComparedPTGHashSet();
            var localVisited = SolverUtils.CreateReferenceComparedPTGHashSet();

            foreach (var v in toVisitParams)
            {
                if (localVisited.Contains(v))
                    continue;

                foreach (var t in v.LambdaVertex)
                    toVisit.Add(t);
            }

            while (toVisit.Count > 0)
            {
                var v = toVisit.First();
                toVisit.Remove(v);
                if (localVisited.Contains(v))
                    continue;

                localVisited.Add(v);

                foreach (var t in v.LambdaVertex)
                    toVisit.Add(t);
            }

            var visited = Configuration.TypesOptimization ? SolverUtils.CreateReferenceComparedPTGHashSet(localVisited.Where(x => x.VertexType == VertexType.Hub || Compatibles(x, currentSymbol))) : localVisited;
            tlambda_closure = tlambda_closure.Add(DateTime.Now.Subtract(st));

            return visited;
        }

        /// <summary>
        /// Obtenemos los vértices del PTG que corresponden a cada término y en la segunda parte de la tupla todos los alcanzables
        /// </summary>
        Tuple<ISet<PtgVertex>, ISet<PtgVertex>> NodesReachableFrom(Term[] writableTerms)
        {
            var reachableVertexs = new Tuple<ISet<PtgVertex>, ISet<PtgVertex>>
                (SolverUtils.CreateReferenceComparedPTGHashSet(), SolverUtils.CreateReferenceComparedPTGHashSet());

            // Se devuelven 3 conjuntos porque no es lo mismo pertencer al term, pertenecer al reachable, o pertenecer al conjunto readonly
            foreach (var term in writableTerms)
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

            var lambdaSet = LambdaClosure(targets, currentSymbol);

            foreach (var t in targets)
                lambdaSet.Add(t);

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

            // Obtenemos la clausura lambda
            var closure = LambdaClosure(ev.VertexSet, ev.VertexType);
            closure.UnionWith(ev.VertexSet);

            // CASO 2: si el término tiene un solo campo, lo clausuramos por lambda y listo
            if (term.IsVar)
            {
                tapt = tapt.Add(DateTime.Now.Subtract(st));
                return closure;
            }

            // CASO 3: si el término tiene más de un campo nos vamos "moviendo" por cada parte
            var currentSet = closure;
            for (var i = 1; i < term.Parts.Count; i++)
                currentSet = PtgPointsTo(currentSet, term[i], term[i].Symbol, lastDefinitions);

            // Se retorna el último current set, que es el conjunto correspondiente al término que recibimos (a la última parte)
            tapt = tapt.Add(DateTime.Now.Subtract(st));
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

        void LambdaCompression(PtgVertex oldHub, PtgVertex newHub)
        {
            if (local_compression && (global_compression || (oldHub.VertexType == VertexType.Hub && newHub.VertexType == VertexType.Hub)))
                VertexCompression(oldHub, newHub);
            else
            {
                // Tratamiento normal
                var e1_added = newHub.AddVertex(EdgeType.Lambda, oldHub);
                var e2_added = oldHub.AddVertex(EdgeType.Lambda, newHub);

                if ((e1_added && (!StaticMode || StaticModeEnabledByCallbackOrRecursion || !StaticModeStack.Peek().Contains(newHub))) && (CheckEqualsSet))
                    SetNotConverged(newHub.LoopDepthLevel);

                if ((e2_added && (!StaticMode || StaticModeEnabledByCallbackOrRecursion || !StaticModeStack.Peek().Contains(oldHub))) && (CheckEqualsSet))
                    SetNotConverged(oldHub.LoopDepthLevel);
            }
        }

        void VertexCompression(PtgVertex v1, PtgVertex v2)
        {
            // 1. Update de los FieldLastDef
            foreach (var dicEntry in v1.FieldsLastDef)
                if (v2.FieldsLastDef.ContainsKey(dicEntry.Key))
                    v2.FieldsLastDef[dicEntry.Key].UnionWith(dicEntry.Value);
                else
                    v2.FieldsLastDef.Add(dicEntry.Key, dicEntry.Value);

            // VERTEX UNION:
            // 2. Cada vértice de salida de ese nodo lo agregás al hub
            // NOTA: Si sacamos los ejes de salida y por casualidad queda uno apuntando de otro scope cagamos
            foreach (var edge in v1.CommonVertex)
            {
                v1.RemoveVertexLambdaCompressionA(edge.Key, edge.Value);
                v2.AddVertex(edge.Key, edge.Value);
            }
            foreach (var edge in v1.SigmaVertex.Where(x => x != v1))
            {
                v2.AddVertex(EdgeType.Sigma, edge);
                v1.RemoveVertexLambdaCompressionA(EdgeType.Sigma, edge);
            }
            foreach (var edge in v1.LambdaVertex)
            {
                v2.AddVertex(EdgeType.Lambda, edge);
                v1.RemoveVertexLambdaCompressionA(EdgeType.Lambda, edge);
            }

            // 3. Por cada vértice que lo apunta, lo removés y apuntás al hub                        
            foreach (var cv in v1.CommonVertexToYou)
                foreach (var cv2 in cv.Value.Where(x => x.Vertex.IsAlive))
                {
                    ((PtgVertex)cv2.Vertex.Target).RemoveVertexLambdaCompressionB(EdgeType.Common, v1, cv.Key);
                    ((PtgVertex)cv2.Vertex.Target).AddVertex(EdgeType.Common, v2, cv.Key);
                }

            foreach (var sv in v1.SigmaVertexToYou.Where(x => x.Vertex.IsAlive))
            {
                ((PtgVertex)sv.Vertex.Target).RemoveVertexLambdaCompressionB(EdgeType.Sigma, v1);
                ((PtgVertex)sv.Vertex.Target).AddVertex(EdgeType.Sigma, v2);
            }
            foreach (var lv in v1.LambdaVertexToYou.Where(x => x.Vertex.IsAlive))
            {
                ((PtgVertex)lv.Vertex.Target).RemoveVertexLambdaCompressionB(EdgeType.Lambda, v1);
                ((PtgVertex)lv.Vertex.Target).AddVertex(EdgeType.Lambda, v2);
            }

            // 4. Cada EP que lo apunta ahora apunta a HUB. Hay que recorrer el diccionario
            foreach (var scopeAndKeys in v1.ScopesContainersToYou.Where(x => x.Key.Scope.IsAlive && ((ScopeContainer)x.Key.Scope.Target).Alive))
                foreach (var key in scopeAndKeys.Value)
                {
                    var entryType = ((ScopeContainer)scopeAndKeys.Key.Scope.Target).RemoveEntryPointValueLambdaCompression(key, v1);
                    // TODO: Sucede a veces que figura que te tienen ciertos scopes pero en realidad no... entonces retorna null type y no queremos agregarlo al nuevo hub
                    if (entryType != null)
                        ((ScopeContainer)scopeAndKeys.Key.Scope.Target).AddEntryPointValue(key, v2, entryType);
                }
            if (v1.GlobalScopeToYou != null)
            {
                var item2temp = v1.GlobalScopeToYou.Item2.ToList();
                foreach (var key in item2temp)
                {
                    var entryType = v1.GlobalScopeToYou.Item1.RemoveEntryPointValueLambdaCompression(key, v1);
                    if (entryType != null)
                        v1.GlobalScopeToYou.Item1.AddEntryPointValue(key, v2, entryType);
                }
            }
            v1.ClearVertex();

            // 5. Modo estático
            if (Configuration.StaticModeEnabled)
            {
                // El contenedor de nodos debe ser el del menor nivel
                if (v1.LoopDepthLevel < v2.LoopDepthLevel)
                {
                    // Ya es alcanzable desde afuera
                    v2.Multiple = true;

                    StaticModeStack.GetAt(v2.LoopDepthLevel - 1).Remove(v2);
                    if (v1.LoopDepthLevel > 0)
                        StaticModeStack.GetAt(v1.LoopDepthLevel - 1).Add(v2);
                }

                // Reemplazamos las entradas en los diccionarios
                if (v1.VertexType == VertexType.Hub)
                { 
                    foreach(var t in v1.StaticKey)
                    { 
                        if (StaticModeHUB.ContainsKey(t))
                            StaticModeHUB[t] = v2;
                        if (StaticModeCallbackRegion.ContainsKey(t))
                            StaticModeCallbackRegion[t] = v2;
                        v2.StaticKey.Add(t);
                    }
                }
                else
                {
                    foreach (var t in v1.StaticKey)
                    {
                        if (StaticModeVertex.ContainsKey(t))
                            StaticModeVertex[t] = v2;
                        if (StaticModeCallbackVertex.ContainsKey(t))
                            StaticModeCallbackVertex[t] = v2;
                        v2.StaticKey.Add(t);
                    }
                }
                if (v1.LoopDepthLevel > 0)
                    StaticModeStack.GetAt(v1.LoopDepthLevel - 1).Remove(v1);
                
                v2.Multiple = v2.Multiple || v1.Multiple;
                v2.LoopDepthLevel = Math.Min(v2.LoopDepthLevel, v1.LoopDepthLevel);
            }
        }
        #endregion
    }
}
