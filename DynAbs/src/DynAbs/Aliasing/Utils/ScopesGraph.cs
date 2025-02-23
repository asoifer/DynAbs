using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynAbs
{
    public class ScopesGraph
    {
        IDictionary<string, StackNode> Nodes;
        Stack<StackEdge> StackEdges;

        public ScopesGraph()
        {
            Nodes = new Dictionary<string, StackNode>();
            StackEdges = new Stack<StackEdge>();
        }

        // O(Add del Dictionary) = O(1) amortizado
        public string PushMethod(string MethodSymbol, out bool existingMethod)
        {
            var firstNode = Nodes.Count == 0;
            var firstEdge = Nodes.Count > 0 && StackEdges.Count == 0;
            
            StackNode otherNode = null;
            if (Nodes.Count == 1)
                otherNode = Nodes[Nodes.Keys.Single()];
            
            StackNode node;
            existingMethod = Nodes.ContainsKey(MethodSymbol);
            if (existingMethod)
                node = Nodes[MethodSymbol];
            else
            {
                node = GetStackNode(MethodSymbol);
                Nodes[MethodSymbol] = node;
            }

            // Si es el 1er nodo entonces tiene que tener 1 entry edge de más porque considero que es llamado desde el "start" invicible
            // Sucede que si otro nodo lo apunta y deja de apuntar como tendría entry edges en 0 lo removería rompiendo todo
            node.EntryEdges++;

            if (!firstNode)
            {
                var newEdge = new StackEdge();
                newEdge.Source = firstEdge ? otherNode : StackEdges.Peek().Target;
                newEdge.Target = node;
                newEdge.Weight = StackEdges.Count + 1;

                if (!existingMethod)
                {
                    node.MinPath = new List<StackNode>(newEdge.Source.MinPath);
                    node.MinPath.Add(newEdge.Source);
                }

                StackEdges.Push(newEdge);
            }
            else
                node.MinPath = new List<StackNode>();

            return node.Key;
        }

        // O(Remove del Dictionary) = O(1) amortizado (creo)
        public void PopMethod()
        {
            if (StackEdges.Count > 0)
            {
                var lastEdge = StackEdges.Pop();
                lastEdge.Target.EntryEdges--;
                // Si removiendo este eje el grafo deja de ser conexo elimino el nodo
                if (lastEdge.Target.EntryEdges == 0)
                    Nodes.Remove(lastEdge.Target.MethodSymbol);
            }
            else
                Nodes.Clear();
        }

        StackNode GetStackNode(string MethodSymbol)
        {
            var returnValue = new StackNode();
            returnValue.MethodSymbol = MethodSymbol;
            return returnValue;
        }
        
        public class StackNode
        {
            public List<StackNode> MinPath;
            public string MethodSymbol;
            public int EntryEdges;
            string key = null;
            public string Key
            {
                get
                {
                    if (key == null)
                        SetInternalKey();
                    return key;
                }
            }

            void SetInternalKey()
            {
                // Before (TODO): key = (MinPath.Count > 0 ? MinPath[MinPath.Count - 1].Key : "") + MethodSymbol.GetHashCode().ToString() + "|";
                key = (MinPath.Count > 0 ? MinPath[MinPath.Count - 1].Key : "") + MethodSymbol.ToString() + "|";
            }
        }

        public class StackEdge
        {
            public StackNode Source;
            public StackNode Target;
            public int Weight;
        }
    }
}
