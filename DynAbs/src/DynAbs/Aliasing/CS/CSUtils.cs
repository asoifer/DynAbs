using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynAbs.Aliasing.CS
{
    class CSUtils
    {
        public static HashSet<PtgVertex> CreateReferenceComparedPTGHashSet()
        {
            return new HashSet<PtgVertex>(new ObjectReferenceEqualityComparer<PtgVertex>());
        }

        public static HashSet<PtgVertex> CreateReferenceComparedPTGHashSet(IEnumerable<PtgVertex> nodos)
        {
            return new HashSet<PtgVertex>(nodos, new ObjectReferenceEqualityComparer<PtgVertex>());
        }

        public static HashSet<PtgVertex> CreateReferenceComparedPTGHashSet(PtgVertex node)
        {
            var toReturn = new HashSet<PtgVertex>(new ObjectReferenceEqualityComparer<PtgVertex>());
            toReturn.Add(node);
            return toReturn;
        }

        public static bool Compatibles(string kind, string otherKind)
        {
            return kind == otherKind || kind == Globals.TopKind || otherKind == Globals.TopKind;
        }

        public static string GenerateDGML(Scope scope, string title = null)
        {
            //var regexNombre = new System.Text.RegularExpressions.Regex(@"^[0-9]*/[0-9]*\-[0-9]*$");
            var nextNodeId = 1;
            var fieldIds = new Dictionary<Field, int>();
            var vertexIds = new Dictionary<PtgVertex, int>();
            var clusterIds = new Dictionary<Cluster, int>();

            // BFS Structures
            var v_toVisit = CSUtils.CreateReferenceComparedPTGHashSet();
            var c_toVisit = new HashSet<Cluster>();
            var v_visited = CSUtils.CreateReferenceComparedPTGHashSet();
            var c_visited = new HashSet<Cluster>();

            // Header
            var sb = new StringBuilder();
            sb.AppendLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
            sb.AppendLine(string.Format("<DirectedGraph Title=\"{0}\" xmlns=\"http://schemas.microsoft.com/vs/2009/dgml\">", (title ?? "").Replace("\"", "'")));

            // Init vertex and edges
            var sbNodes = new StringBuilder();
            var sbEdges = new StringBuilder();
            sbNodes.AppendLine("<Nodes>");
            sbEdges.AppendLine("<Links>");

            foreach (var fpt in scope.PointsTo)
            {
                var varId = nextNodeId++;
                fieldIds.Add(fpt.Key, varId);

                // Add the entry point
                sbNodes.AppendLine(string.Format("<Node Id=\"{0}\" Label=\"{1}\" Category=\"{2}\" Style=\"{3}\" />", 
                    varId, fpt.Key.ToString(), "EntryPoint", "Plain"));

                foreach (var tkn in fpt.Value)
                {
                    foreach(var n in tkn.Value)
                    {
                        if (!vertexIds.ContainsKey(n))
                        {
                            // Si no está lo agrega
                            vertexIds.Add(n, nextNodeId++);
                            v_toVisit.Add(n);
                        }
                        var nId = vertexIds[n];

                        sbEdges.AppendLine(string.Format("<Link Source=\"{0}\" Target=\"{1}\" Label=\"{2}\" />", 
                            varId, nId, tkn.Key.ToString()));
                    }
                }
            }

            while (v_toVisit.Count > 0 || c_toVisit.Count > 0)
            {
                if (v_toVisit.Count > 0)
                {
                    var v = v_toVisit.First();
                    v_toVisit.Remove(v);
                    if (v_visited.Contains(v))
                        continue;
                    v_visited.Add(v);

                    var vId = vertexIds[v];
                    sbNodes.AppendLine(string.Format("<Node Id=\"{0}\" Label=\"{1}\" Category=\"{2}\" Style=\"{3}\" />",
                    vId, v.ID + " - " + v.TypeKind.ToString(), "Vertex", "Plain"));

                    var r = v.Find();
                    if (r != v.Find())
                    {
                        // Si es otro: A) si no está agregado lo hace, B) lo agrega a to_visit, C) agrega eje
                        if (!vertexIds.ContainsKey(r))
                            vertexIds.Add(r, nextNodeId++);

                        var rId = vertexIds[r];
                        v_toVisit.Add(r);

                        sbEdges.AppendLine(string.Format("<Link Source=\"{0}\" Target=\"{1}\" Label=\"{2}\" />",
                            vId, rId, "REP"));
                    }
                    else
                    {
                        // Si es otro: A) si no está agregado lo hace, B) lo agrega a to_visit, C) agrega eje (con el cluster)
                        if (!clusterIds.ContainsKey(r.Rep))
                            clusterIds.Add(r.Rep, nextNodeId++);

                        var cId = clusterIds[r.Rep];
                        c_toVisit.Add(r.Rep);

                        sbEdges.AppendLine(string.Format("<Link Source=\"{0}\" Target=\"{1}\" Label=\"{2}\" />",
                            vId, cId, "Cluster"));
                    }
                }
                if (c_toVisit.Count > 0)
                {
                    var c = c_toVisit.First();
                    c_toVisit.Remove(c);
                    if (c_visited.Contains(c))
                        continue;
                    c_visited.Add(c);

                    var cId = clusterIds[c];
                    sbNodes.AppendLine(string.Format("<Node Id=\"{0}\" Label=\"{1}\" Category=\"{2}\" Style=\"{3}\" />",
                    cId, c.ID, "Cluster", "Plain"));

                    foreach (var tkF in c.Targets)
                    {
                        foreach (var ftk in tkF.Value)
                        {
                            foreach (var tkT in ftk.Value)
                            {
                                foreach (var n2 in tkT.Value)
                                {
                                    if (!vertexIds.ContainsKey(n2))
                                        vertexIds.Add(n2, nextNodeId++);

                                    var nId2 = vertexIds[n2];
                                    v_toVisit.Add(n2);

                                    sbEdges.AppendLine(string.Format("<Link Source=\"{0}\" Target=\"{1}\" Label=\"{2}\" />",
                                        cId, nId2, tkF.Key + " | " + ftk.Key.ToString() + " | " + tkT.Key));
                                }
                            }
                        }
                    }
                }
            }

            // Finish vertex and edges
            sbNodes.AppendLine("</Nodes>");
            sbEdges.AppendLine("</Links>");

            sb.Append(sbNodes);
            sb.Append(sbEdges);
            sb.AppendLine("<Categories>");
            
            // Me gustaría: 
            // Negro: entry points
            // Gris: vertex
            // Blanco: cluster

            sb.AppendLine("<Category Id=\"EntryPoint\" Background=\"Black\" />");
            sb.AppendLine("<Category Id=\"Vertex\" Background=\"Grey\" />");
            sb.AppendLine("<Category Id=\"Cluster\" Background=\"White\" />");
            //sb.AppendLine("<Category Id=\"HUB\" Background=\"White\" />");
            //sb.AppendLine("<Category Id=\"CommonMinType\" Background=\"Yellow\" />");
            // ejemplo eje: <Category Id="PassedTest" Label="Passed" Stroke="Black" Background="Green" />  
            // ejemplo eje que hereda: <Category Id="FailedTest" Label="Failed" BasedOn="PassedTest" Background="Red" />  
            // ejemplo nodo: <Category Id="Person" Background="Orange" />  
            sb.AppendLine("</Categories>");
            
            sb.AppendLine("</DirectedGraph>");
            return sb.ToString();


            // OLD:
            //var i = 0;
            //foreach (var vertex in graph.VisitedGraph.Vertices)
            //    vertexIds.Add(vertex, i++);

            //foreach (var vid in vertexIds)
            //{
            //    var label = regexNombre.IsMatch(vid.Key.Description) ?
            //        TermFactory.GetFileTextBySyntaxNodeName(vid.Key.Description).Replace('"', '\'') : Utils.CleanupDotNodeLabel(vid.Key.Description);
            //    var texture = vid.Key.Multiple ? "Glass" : "Plain";

            //    string category = "";
            //    switch (vid.Key.VertexType)
            //    {
            //        case VertexType.Hub:
            //            category = "HUB";
            //            break;
            //        case VertexType.EntryPoint:
            //            category = "EntryPoint";
            //            break;
            //        case VertexType.Common:
            //            category = vid.Key.IsMinType ? "CommonMinType" : "Common";
            //            break;
            //    }

            //    sb.AppendLine(string.Format("<Node Id=\"{0}\" Label=\"{1} : {3}\" Category=\"{2}\" Style=\"{4}\" />", vid.Value, label, category, vid.Key.Type.ToString(), texture));

            //    //var shape = "";
            //    //var color = "white";
            //    //var fontColor = "black";
            //    //var bold = false;

            //    //var label = string.Format(bold ? "{0} : {1}" : "{0} : {1}", , vid.Key.Type.ToString());
            //    //sb.AppendLine(string.Format("{0} [shape={2}, style=filled, label=\"{1}\", fontcolor={4}, fillcolor={3}];", vid.Value, label, shape, color, fontColor));

            //}
            //sb.AppendLine("</Nodes>");
            //sb.AppendLine("<Links>");

            //foreach (var e in graph.VisitedGraph.Edges)
            //{
            //    var edgeLabel = "";
            //    if (e.EdgeType == EdgeType.Lambda)
            //        edgeLabel = "L";
            //    else if (e.EdgeType == EdgeType.Sigma)
            //        edgeLabel = "S";
            //    else if (e.EdgeType == EdgeType.Common)
            //        edgeLabel = e.EdgeName.Value;

            //    sb.AppendLine(string.Format("<Link Source=\"{0}\" Target=\"{1}\" Label=\"{2}\" />",
            //        vertexIds[e.Source], vertexIds[e.Target], edgeLabel)); // Category="PassedTest"
            //}
            //sb.AppendLine("</Links>");
        }
    }
}
