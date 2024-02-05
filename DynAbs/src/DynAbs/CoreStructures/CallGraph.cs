using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace DynAbs
{
    class CallGraph
    {
        int NextId = 1;
        List<Node> Headers { get; } = new List<Node>();
        Node LastNode = null;

        public void EnterMethod(Stmt statement)
        {
            var node = new Node(NextId++, LastNode, statement);
            if (LastNode == null)
                Headers.Add(node);
            LastNode = node;
        }

        public void ExitMethod(Stmt statement)
        {
            // TODO: chequeos
            var container = statement.CSharpSyntaxNode.GetContainer();
            var currentContainer = LastNode.Statement.CSharpSyntaxNode.GetContainer();
            if (!(LastNode.Statement.FileId == statement.FileId &&
                container.Span.Start == currentContainer.Span.Start &&
                container.Span.End == currentContainer.Span.End))
                throw new SlicerException("Exit method doesn't match with its method");
            var thisNode = LastNode;
            LastNode = LastNode.Parent;
            if (LastNode != null)
                LastNode.Edges.Add(thisNode);
        }

        public void PrintGraph(string path)
        {
            var sb = new StringBuilder();
            sb.AppendLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
            sb.AppendLine(string.Format("<DirectedGraph Title=\"{0}\" xmlns=\"http://schemas.microsoft.com/vs/2009/dgml\">", "DG"));

            var nodes = new List<Tuple<int, string>>();
            var edges = new List<Tuple<int, int>>();
            foreach(var h in Headers)
            {
                var stack = new Stack<Node>();
                stack.Push(h);
                while (stack.Count > 0)
                {
                    var next = stack.Pop();
                    
                    var label = "MAIN";
                    if (next.Statement != null)
                    { 
                        using var reader = new StringReader(next.Statement.CSharpSyntaxNode.ToString());
                        var methodHeader = reader.ReadLine();
                        label = next.Statement.FileName + ":" + next.Statement.Line + $" - ({methodHeader})";
                    }

                    nodes.Add(new Tuple<int, string>(next.Id, label));

                    foreach (var e in next.Edges)
                    { 
                        edges.Add(new Tuple<int, int>(next.Id, e.Id));
                        stack.Push(e);
                    }
                }
            }
            
            sb.AppendLine("<Nodes>");
            foreach (var n in nodes)
                sb.AppendLine(string.Format("<Node Id=\"{0}\" Label=\"{1}\" />", n.Item1, n.Item2)); 
            sb.AppendLine("</Nodes>");
            sb.AppendLine("<Links>");
            foreach (var e in edges)
                sb.AppendLine(string.Format("<Link Source=\"{0}\" Target=\"{1}\" />", e.Item1, e.Item2));
            sb.AppendLine("</Links>");
            sb.AppendLine("</DirectedGraph>");
            System.IO.File.WriteAllText(path, sb.ToString());
        }

        class Node
        {
            public int Id { get; set; }
            public Node Parent { get; set; }
            public Stmt Statement { get; set; }
            public List<Node> Edges { get; set; }
            public Node(int id, Node parent, Stmt statement)
            {
                Id = id;
                Parent = parent;
                Statement = statement;
                Edges = new List<Node>();
            }
        }
    }
}
