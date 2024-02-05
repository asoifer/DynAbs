using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using QuikGraph;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using DynAbs;

namespace DynAbs.DesktopApp.Browser
{
    public partial class LineDetail : Form
    {
        ISet<Stmt> _currentStatements;
        AdjacencyGraph<string, Edge<string>> _statementsDependencyGraph;
        Dictionary<int, string> _idToFiles;
        Dictionary<string, Stmt> _vertexToStatement;
        ComplexBrowser _complexBrowser;

        public LineDetail(ComplexBrowser complexBrowser, List<TreeNode> solutionFiles, Dictionary<int, string> idToFiles,
            AdjacencyGraph<string, Edge<string>> statementsDependencyGraph, Dictionary<string, Stmt> vertexToStatement)
        {
            _complexBrowser = complexBrowser;
            _idToFiles = idToFiles;
            _statementsDependencyGraph = statementsDependencyGraph;
            _vertexToStatement = vertexToStatement;

            InitializeComponent();

            tvDependencies.Nodes.AddRange(solutionFiles.Select(x => Utils.CopyTreeNode(x)).ToArray());
        }

        public void ShowFor(string currentLine, ISet<string> edges)
        {
            lbStatements.DataSource = edges.Select(x => new CustomListBoxItem() { key = x, value = GetTextForSyntaxNode((_vertexToStatement[x].CSharpSyntaxNode)) }).ToList();
            lbStatements.DisplayMember = "value";
            lbStatements.ValueMember = "key";
            lbStatements.Refresh();

            lbDependencies.DataSource = null;
            lbDependencies.Refresh();

            lblCurrentLine.Text = "Line: " + (currentLine.Length > 80 ? currentLine.Substring(0, 80) + "..." : currentLine);

            this.Show();
        }

        public string GetTextForSyntaxNode(CSharpSyntaxNode node)
        {
            if (node is SwitchStatementSyntax)
                node = ((SwitchStatementSyntax)node).Expression;
            if (node is IfStatementSyntax)
                node = ((IfStatementSyntax)node).Condition;
            if (node is WhileStatementSyntax)
                node = ((WhileStatementSyntax)node).Condition;
            if (node is ForStatementSyntax)
                node = ((ForStatementSyntax)node).Condition;
            if (node is DoStatementSyntax)
                node = ((DoStatementSyntax)node).Condition;
            if (node is ForEachStatementSyntax)
                node = ((ForEachStatementSyntax)node).Expression;
            if (node is MethodDeclarationSyntax)
                node = ((MethodDeclarationSyntax)node).ParameterList;
            if (node is ConstructorDeclarationSyntax)
                node = ((ConstructorDeclarationSyntax)node).ParameterList;
            if (node is LockStatementSyntax)
                node = ((LockStatementSyntax)node).Expression;
            if (node is AccessorDeclarationSyntax)
                return ((PropertyDeclarationSyntax)node.Parent.Parent).Identifier.Text.Trim();
            // TODO: No sé porque están estos nodos
            if (node is ClassDeclarationSyntax)
                return ((ClassDeclarationSyntax)node).Identifier.Text.Trim();

            return node.GetText().ToString().Trim();
        }

        private void lbStatements_SelectedIndexChanged(object sender, EventArgs e)
        {
            var selectedItem = (CustomListBoxItem)lbStatements.SelectedItem;
            var edges = _statementsDependencyGraph.OutEdges(selectedItem.key);
            _currentStatements = new HashSet<Stmt>(edges.Select(x => _vertexToStatement[x.Target]));

            Utils.RefrescarMenuSlice(_idToFiles, tvDependencies, _currentStatements);
        }

        private void tvDependencies_AfterSelect(object sender, TreeViewEventArgs e)
        {
            Utils.SetListBox(_idToFiles, tvDependencies, lbDependencies, _currentStatements);
        }

        private void lbDependencies_SelectedIndexChanged(object sender, EventArgs e)
        {
            var selectedItem = (CustomListBoxItem)lbDependencies.SelectedItem;
            if (selectedItem != null)
                _complexBrowser.CargarDocumentoByVertex(selectedItem.key);
        }

        private void LineDetail_FormClosing(object sender, FormClosingEventArgs e)
        {
            // Prevent dispose, as this form can be re-used
            if (e.CloseReason != CloseReason.FormOwnerClosing)
            {
                _complexBrowser.Select();

                e.Cancel = true;
                Hide();
            }
        }
    }
}
