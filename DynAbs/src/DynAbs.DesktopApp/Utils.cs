﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace DynAbs.DesktopApp
{
    static class Utils
    {
        public static int InRange(this int x, int lo, int hi)
        {
            Debug.Assert(lo <= hi);
            return x < lo ? lo : (x > hi ? hi : x);
        }
        public static bool IsInRange(this int x, int lo, int hi)
        {
            return x >= lo && x <= hi;
        }
        public static Color HalfMix(this Color one, Color two)
        {
            return Color.FromArgb(
                (one.A + two.A) >> 1,
                (one.R + two.R) >> 1,
                (one.G + two.G) >> 1,
                (one.B + two.B) >> 1);
        }

        public static void HighlightCurrentAndParents(TreeNode treeNode, Action<TreeNode> action)
        {
            var current = treeNode;
            while (current != null)
            {
                action(current);
                current = current.Parent;
            }
        }

        public static void HighlightNode(TreeNode treeNode, string path, Action<TreeNode> action)
        {
            if (treeNode.ToolTipText == path)
                HighlightCurrentAndParents(treeNode, action);
            foreach (var node in treeNode.Nodes)
                HighlightNode((TreeNode)node, path, action);
        }

        public static void HighlightFiles(Dictionary<int, string> idToFiles, List<TreeNode> nodos, IEnumerable<Stmt> selectedStatements, Action<TreeNode> action)
        {
            var selectedFiles = selectedStatements.Select(x => x.FileId).Distinct();
            foreach (var selectedFile in selectedFiles)
            {
                var file = idToFiles[selectedFile];
                foreach (var nodo in nodos)
                    HighlightNode(nodo, file, action);
            }
        }

        public static void CleanForeColor(TreeNode treeNode)
        {
            treeNode.ForeColor = Color.Black;
            foreach (var node in treeNode.Nodes)
                CleanForeColor((TreeNode)node);
        }

        public static void RefreshMenuSlice(Dictionary<int, string> idToFiles, TreeView tv, ISet<Stmt> statements)
        {
            var tvNodes = new List<TreeNode>();
            var nodesEnumerator = tv.Nodes.GetEnumerator();
            while (nodesEnumerator.MoveNext())
            {
                CleanForeColor((TreeNode)nodesEnumerator.Current);
                tvNodes.Add((TreeNode)nodesEnumerator.Current);
            }
            HighlightFiles(idToFiles, tvNodes, statements, (x => x.ForeColor = Color.Red));
            tv.Refresh();
        }

        public static TreeNode CopyTreeNode(TreeNode treeNode)
        {
            var result = new TreeNode();
            result.Text = treeNode.Text;
            result.ToolTipText = treeNode.ToolTipText;
            foreach (var node in treeNode.Nodes)
                result.Nodes.Add(CopyTreeNode((TreeNode)node));
            return result;
        }

        public static void SetListBox(Dictionary<int, string> idToFiles, TreeView tv, ListBox lb, ISet<Stmt> statements)
        {
            var newItems = new List<CustomListBoxItem>();

            var currentFilePath = tv.SelectedNode.ToolTipText;
            if (!string.IsNullOrWhiteSpace(currentFilePath))
            {
                var currentFile = System.IO.File.ReadAllText(tv.SelectedNode.ToolTipText);

                if (idToFiles.Any(x => x.Value == tv.SelectedNode.ToolTipText))
                {
                    var idFile = idToFiles.Where(x => x.Value == tv.SelectedNode.ToolTipText).Single().Key;
                    foreach (var stmt in statements.Where(x => x.FileId == idFile))
                    {
                        var newItem = new CustomListBoxItem();
                        newItem.key = stmt.FileId + "." + stmt.Line;
                        newItem.value = currentFile.Substring(stmt.SpanStart, stmt.SpanEnd - stmt.SpanStart + 1);
                        newItems.Add(newItem);
                    }
                }
            }
            lb.DataSource = newItems;
            lb.ValueMember = "key";
            lb.DisplayMember = "value";
            lb.Refresh();
        }
    }

    public class CustomListBoxItem
    {
        public string key { get; set; }
        public string value { get; set; }
    }
}
