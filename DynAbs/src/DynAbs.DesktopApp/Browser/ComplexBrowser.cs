using ScintillaNET;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using QuikGraph;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;

namespace DynAbs.DesktopApp.Browser
{
    public partial class ComplexBrowser : Form
    {
        #region Properties
        Solution _currentSolution { get; set; }
        BrowsingData _browsingData { get; set; }
        AdjacencyGraph<string, Edge<string>> _reverseDependencyGraph { get; set; }
        Dictionary<int, CSharpSyntaxTree> _fileIdToSyntaxTree { get; set; }
        Dictionary<int, string> _idToFiles { get; set; }
        IEnumerable<Stmt> _executedStatements { get; set; }
        ISet<Stmt> _currentResult { get; set; }

        LineDetail _lineDetail;
        SourcesAndTargets _sourcesAndTargets;
        Scintilla _currentTextEditorControl { get { return (Scintilla)tcContainer.SelectedTab.Controls[0]; } }
        #endregion

        #region Loader
        public static void Run(Solution solution,
            BrowsingData reduced,
            Dictionary<int, CSharpSyntaxTree> fileIdToSyntaxTree,
            Dictionary<int, string> idToFiles,
            IEnumerable<Stmt> executedStatements)
        {
            var thread = new Thread(() =>
            {
                Thread.CurrentThread.IsBackground = true;
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new ComplexBrowser(solution, reduced, fileIdToSyntaxTree, idToFiles, executedStatements));
            });
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
        }

        public ComplexBrowser(Solution solution, 
            BrowsingData reducedDG,
            Dictionary<int, CSharpSyntaxTree> fileIdToSyntaxTree,
            Dictionary<int, string> idToFiles,
            IEnumerable<Stmt> executedStatements)
        {
            _currentSolution = solution;
            _browsingData = reducedDG;
            _fileIdToSyntaxTree = fileIdToSyntaxTree;
            _idToFiles = idToFiles;
            _executedStatements = executedStatements;

            AdjacencyGraph<string, Edge<string>> tempDG;
            GraphUtils.LoadReverseDependencyGraph(_browsingData.ReducedDG, out tempDG);
            _reverseDependencyGraph = tempDG;

            InitializeComponent();
        }

        private void ComplexBrowser_Load(object sender, EventArgs e)
        {
            var nodos = GetNodesTreeView();
            Utils.HighlightFiles(_idToFiles, nodos, _executedStatements, (x => x.BackColor = Color.Yellow));
            tvArchivos.Nodes.AddRange(nodos.ToArray());
            _currentResult = new HashSet<Stmt>();
            _lineDetail = new LineDetail(this, nodos, _idToFiles, _browsingData.RealDG, _browsingData.VertexToStatement);
            _sourcesAndTargets = new SourcesAndTargets(this, _idToFiles, nodos);
        }
        #endregion

        #region TreeView
        // Returns the projects and builds the tree
        List<TreeNode> GetNodesTreeView()
        {
            var result = new List<TreeNode>();
            foreach (var project in _currentSolution.Projects)
            {
                var projectNode = new TreeNode();
                var projectDirectory = Path.GetDirectoryName(project.FilePath);
                projectNode.Text = project.Name;
                foreach (var document in project.Documents)
                {
                    var documentNode = new TreeNode();
                    documentNode.Text = document.Name;
                    documentNode.ToolTipText = document.FilePath;

                    // Check if the document's path is part of the project's path
                    // If it is part of the project we build the intermediate nodes
                    // The goal is to reach the file from the project's path
                    var documentDirectory = Path.GetDirectoryName(document.FilePath);
                    if (!documentDirectory.Contains(projectDirectory))
                        projectNode.Nodes.Add(documentNode);
                    else
                    {
                        var difference = documentDirectory.Substring(projectDirectory.Length,
                            documentDirectory.Length - projectDirectory.Length);
                        var lastFolder = projectNode;
                        if (!string.IsNullOrWhiteSpace(difference))
                        {
                            var folders = difference.Substring(1, difference.Length - 1)
                                                    .Split(new[] { "\\" }, StringSplitOptions.None);
                            foreach (var folder in folders)
                            {
                                var nextNode = GetNodeByName(lastFolder.Nodes, folder);
                                if (nextNode == null)
                                {
                                    nextNode = new TreeNode();
                                    nextNode.Text = folder;
                                    lastFolder.Nodes.Add(nextNode);
                                }
                                lastFolder = nextNode;
                            }
                        }
                        lastFolder.Nodes.Add(documentNode);
                    }
                }
                result.Add(projectNode);
            }
            return result;
        }

        TreeNode GetNodeByName(TreeNodeCollection nodeCollection, string name)
        {
            var enumerator = nodeCollection.GetEnumerator();
            while (enumerator.MoveNext())
                if (((TreeNode)enumerator.Current).Text == name)
                    return ((TreeNode)enumerator.Current);
            return null;
        }
        #endregion

        #region MarcarEditor
        void RefreshSlice()
        {
            var tabEnumerator = tcContainer.Controls.GetEnumerator();
            while (tabEnumerator.MoveNext())
                if (tabEnumerator.Current is TabPage)
                    RefreshSlice((Scintilla)((TabPage)tabEnumerator.Current).Controls[0]);
        }
        
        void RefreshSlice(Scintilla editor)
        {
            // Remove previous slice
            foreach (var line in editor.Lines)
                line.MarkerDelete(SLICED_MARKER);
            
            // Refresh current slice
            var fileId = getCurrentDocumentId(editor);
            var lineasActuales = _currentResult.GroupBy(x => x.FileId).FirstOrDefault(x => x.Key == fileId);
            if (lineasActuales != null)
            {
                foreach (var linea in lineasActuales)
                    editor.Lines[linea.Line-1].MarkerAdd(SLICED_MARKER);
            }
        }

        void RefreshExecutedLines(Scintilla editor)
        {
            var fileId = getCurrentDocumentId(editor);
            foreach (var linea in _executedStatements.Where(x => x.FileId == fileId))
                editor.Lines[linea.Line-1].MarkerAdd(EXECUTED_MARKER);
        }
        #endregion

        #region General
        void LoadDocument(string filePath)
        {
            if (!string.IsNullOrWhiteSpace(filePath))
            {
                // We don't want to add the same tab twice
                var tabEnumerator = tcContainer.Controls.GetEnumerator();
                while (tabEnumerator.MoveNext())
                    if (tabEnumerator.Current is TabPage && ((TabPage)tabEnumerator.Current).Name == "tab:" + filePath)
                    {
                        tcContainer.SelectedTab = (TabPage)tabEnumerator.Current;
                        return;
                    }

                var newTab = new TabPage(Path.GetFileName(filePath.Split('.')[0]));
                newTab.Name = "tab:" + filePath;

                var newEditor = InitTextEditor(filePath);
                RefreshExecutedLines(newEditor);
                RefreshSlice(newEditor);

                newTab.Controls.Add(newEditor);
                tcContainer.Controls.Add(newTab);
                tcContainer.SelectedTab = tcContainer.TabPages[tcContainer.TabPages.Count - 1];
            }
        }

        public void LoadDocumentByVertex(string vertex)
        {
            var document = _idToFiles[Convert.ToInt32(vertex.Split('.')[0])];
            LoadDocument(document);

            // TODO-NETCORE (highlight the line)
            //((Scintilla)tcContainer.SelectedTab.Controls[0]).ActiveTextAreaControl.Caret.Line 
            //    = Convert.ToInt32(vertex.Split('.')[1]) - 1;
        }

        private void tvFiles_AfterSelect(object sender, TreeViewEventArgs e)
        {
            LoadDocument(tvArchivos.SelectedNode.ToolTipText);
        }

        private void backwardSlicingToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var currentLine = getCurrentLine();
            _currentResult = GetByFileIdAndLine(_browsingData.ReducedDG, getCurrentDocumentId(), currentLine);
            RefreshSlice();
            Utils.RefreshMenuSlice(_idToFiles, tvArchivos, _currentResult);
        }

        private void forwardSliceToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var currentLine = getCurrentLine();
            _currentResult = GetByFileIdAndLine(_reverseDependencyGraph, getCurrentDocumentId(), currentLine);
            RefreshSlice();
            Utils.RefreshMenuSlice(_idToFiles, tvArchivos, _currentResult);
        }

        private void showDependenciesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var currentLine = getCurrentLine();
            var currentDocumentId = getCurrentDocumentId();
            var vertex = currentDocumentId + "." + currentLine;

            if (_browsingData.LineSpans.ContainsKey(vertex))
            {
                ISet<Stmt> sourcesSet;
                ISet<Stmt> targetsSet;
                if (_browsingData.ReducedDG.Vertices.Contains(vertex))
                {
                    var sources = _browsingData.ReducedDG.OutEdges(vertex);
                    var targets = _reverseDependencyGraph.OutEdges(vertex);

                    sourcesSet = GetStmtFromVertices(sources.Select(x => x.Target));
                    targetsSet = GetStmtFromVertices(targets.Select(x => x.Target));
                }
                else
                {
                    sourcesSet = new HashSet<Stmt>();
                    targetsSet = new HashSet<Stmt>();
                }

                var nombreArchivo =
                    Path.GetFileName(_currentTextEditorControl.Name.Substring(4, _currentTextEditorControl.Name.Length - 4));
                var data = _browsingData.LineSpans[vertex];
                var textoLinea = _currentTextEditorControl.Text.Substring(data.Item1, data.Item2 - data.Item1 + 1);
                var textoAMostrar = nombreArchivo + " - " + textoLinea;

                _sourcesAndTargets.ShowFor(textoAMostrar, sourcesSet, targetsSet);
            }
        }

        private void showDetailsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var currentLine = getCurrentLine();
            var currentDocumentId = getCurrentDocumentId();
            var vertex = currentDocumentId + "." + currentLine;

            if (_browsingData.LineSpans.ContainsKey(vertex))
            {
                var nombreArchivo =
                    Path.GetFileName(_currentTextEditorControl.Name.Substring(4, _currentTextEditorControl.Name.Length - 4));
                var data = _browsingData.LineSpans[vertex];
                var textoLinea = _currentTextEditorControl.Text.Substring(data.Item1, data.Item2 - data.Item1 + 1);
                var textoAMostrar = nombreArchivo + " - " + textoLinea;

                var statementsToShow = _browsingData.LineStatements[vertex];

                _lineDetail.ShowFor(textoAMostrar, statementsToShow);
            }
        }

        private void closeTabToolStripMenuItem_Click(object sender, EventArgs e)
        {
            tcContainer.TabPages.RemoveAt(tcContainer.SelectedIndex);
        }
        #endregion

        #region Result
        ISet<Stmt> GetByFileIdAndLine(AdjacencyGraph<string, Edge<string>> graph, int fileId, int line)
        {
            var vertice = fileId + "." + line;
            if (!graph.Vertices.Contains(vertice))
                return new HashSet<Stmt>();

            var edges = GraphUtils.BFS(graph, vertice);
            return GetStmtFromVertices(edges);
        }

        ISet<Stmt> GetStmtFromVertices(IEnumerable<string> vertices)
        {
            var returnSet = new HashSet<Stmt>();
            foreach(var vertex in vertices)
            { 
                var data = _browsingData.LineSpans[vertex];
                var splitResult = vertex.Split('.');
                var tempStmt = new Stmt();
                tempStmt.FileId = Convert.ToInt32(splitResult[0]);
                tempStmt.Line = Convert.ToInt32(splitResult[1]);
                tempStmt.SpanStart = data.Item1;
                tempStmt.SpanEnd = data.Item2;
                returnSet.Add(tempStmt);
            }
            return returnSet;
        }
        #endregion

        #region Helpers
        private int getCurrentLine()
        {
            return _currentTextEditorControl.CurrentLine + 1;
            //return _currentTextEditorControl.ActiveTextAreaControl.Caret.Line + 1;
        }

        int getCurrentDocumentId(Scintilla textEditorControl = null)
        {
            var currentEC = textEditorControl ?? _currentTextEditorControl;
            var entry = _idToFiles.Where(x => "tec:" + x.Value == currentEC.Name);
            if (entry.Count() == 0)
                return 0;
            return entry.Single().Key;
        }

        private void ComplexBrowser_FormClosing(object sender, FormClosingEventArgs e)
        {
            _sourcesAndTargets.Close();
        }
        #endregion

        #region Configure Editor
        Scintilla InitTextEditor(string filePath)
        {
            var newEditor = new Scintilla();
            newEditor.Name = "tec:" + filePath;
            newEditor.Dock = System.Windows.Forms.DockStyle.Fill;
            //newEditor.ReadOnly = true;
            newEditor.ContextMenuStrip = cmTextEditorMenu;
            //newEditor.SetHighlighting("C#");
            //newEditor.ShowTabs = true;
            //newEditor.LoadFile(filePath);
            
            newEditor.Text = File.ReadAllText(filePath);
            InitSyntaxColoring(newEditor);
            InitNumberMargin(newEditor);
            InitMarkersStyles(newEditor);
            return newEditor;
        }

        private void InitSyntaxColoring(Scintilla TextArea)
        {
            // Configure the default style
            TextArea.StyleResetDefault();
            TextArea.Styles[Style.Default].Font = "Consolas";
            TextArea.Styles[Style.Default].Size = 10;
            //TextArea.Styles[Style.Default].BackColor = IntToColor(0x212121);
            //TextArea.Styles[Style.Default].ForeColor = IntToColor(0xFFFFFF);
            TextArea.Styles[Style.Default].BackColor = Color.White;
            TextArea.Styles[Style.Default].ForeColor = Color.Black;
            TextArea.StyleClearAll();

            // Configure the CPP (C#) lexer styles
            //TextArea.Styles[Style.Cpp.Identifier].ForeColor = IntToColor(0xD0DAE2);
            //TextArea.Styles[Style.Cpp.Comment].ForeColor = IntToColor(0xBD758B);
            //TextArea.Styles[Style.Cpp.CommentLine].ForeColor = IntToColor(0x40BF57);
            //TextArea.Styles[Style.Cpp.CommentDoc].ForeColor = IntToColor(0x2FAE35);
            //TextArea.Styles[Style.Cpp.Number].ForeColor = IntToColor(0xFFFF00);
            //TextArea.Styles[Style.Cpp.String].ForeColor = IntToColor(0xFFFF00);
            //TextArea.Styles[Style.Cpp.Character].ForeColor = IntToColor(0xE95454);
            //TextArea.Styles[Style.Cpp.Preprocessor].ForeColor = IntToColor(0x8AAFEE);
            //TextArea.Styles[Style.Cpp.Operator].ForeColor = IntToColor(0xE0E0E0);
            //TextArea.Styles[Style.Cpp.Regex].ForeColor = IntToColor(0xff00ff);
            //TextArea.Styles[Style.Cpp.CommentLineDoc].ForeColor = IntToColor(0x77A7DB);
            //TextArea.Styles[Style.Cpp.Word].ForeColor = IntToColor(0x48A8EE);
            //TextArea.Styles[Style.Cpp.Word2].ForeColor = IntToColor(0xF98906);
            //TextArea.Styles[Style.Cpp.CommentDocKeyword].ForeColor = IntToColor(0xB3D991);
            //TextArea.Styles[Style.Cpp.CommentDocKeywordError].ForeColor = IntToColor(0xFF0000);
            //TextArea.Styles[Style.Cpp.GlobalClass].ForeColor = IntToColor(0x48A8EE);

            TextArea.Styles[Style.Cpp.Identifier].ForeColor = Color.Black;
            TextArea.Styles[Style.Cpp.Comment].ForeColor = Color.Green;
            TextArea.Styles[Style.Cpp.CommentLine].ForeColor = Color.Green;
            TextArea.Styles[Style.Cpp.CommentDoc].ForeColor = Color.Green;
            TextArea.Styles[Style.Cpp.Number].ForeColor = Color.Black;
            TextArea.Styles[Style.Cpp.String].ForeColor = Color.Red;
            TextArea.Styles[Style.Cpp.Character].ForeColor = Color.Red;
            TextArea.Styles[Style.Cpp.Preprocessor].ForeColor = Color.DarkGray;
            TextArea.Styles[Style.Cpp.Operator].ForeColor = Color.Black;
            TextArea.Styles[Style.Cpp.Regex].ForeColor = Color.Red;
            TextArea.Styles[Style.Cpp.CommentLineDoc].ForeColor = Color.Green;
            TextArea.Styles[Style.Cpp.Word].ForeColor = Color.Blue;
            TextArea.Styles[Style.Cpp.Word2].ForeColor = Color.Blue;
            TextArea.Styles[Style.Cpp.CommentDocKeyword].ForeColor = Color.Green;
            TextArea.Styles[Style.Cpp.CommentDocKeywordError].ForeColor = Color.Green;
            TextArea.Styles[Style.Cpp.GlobalClass].ForeColor = Color.Blue;

            TextArea.Lexer = Lexer.Cpp;

            TextArea.SetKeywords(0, "class extends implements import interface new case do while else if for in switch throw get set function var try catch finally while with default break continue delete return each const namespace package include use is as instanceof typeof author copy default deprecated eventType example exampleText exception haxe inheritDoc internal link mtasc mxmlc param private return see serial serialData serialField since throws usage version langversion playerversion productversion dynamic private public partial static intrinsic internal native override protected AS3 final super this arguments null Infinity NaN undefined true false abstract as base bool break by byte case catch char checked class const continue decimal default delegate do double descending explicit event extern else enum false finally fixed float for foreach from goto group if implicit in int interface internal into is lock long new null namespace object operator out override orderby params private protected public readonly ref return switch struct sbyte sealed short sizeof stackalloc static string select this throw true try typeof uint ulong unchecked unsafe ushort using var virtual volatile void while where yield");
            TextArea.SetKeywords(1, "void Null ArgumentError arguments Array Boolean Class Date DefinitionError Error EvalError Function int Math Namespace Number Object RangeError ReferenceError RegExp SecurityError String SyntaxError TypeError uint XML XMLList Boolean Byte Char DateTime Decimal Double Int16 Int32 Int64 IntPtr SByte Single UInt16 UInt32 UInt64 UIntPtr Void Path File System Windows Forms ScintillaNET");

        }

        /// <summary>
		/// the background color of the text area
		/// </summary>
		private const int BACK_COLOR = 0x2A211C;

        /// <summary>
        /// default text color of the text area
        /// </summary>
        private const int FORE_COLOR = 0xB7B7B7;

        private const int NUMBER_MARGIN = 1;
        private void InitNumberMargin(Scintilla TextArea)
        {
            TextArea.Styles[Style.LineNumber].BackColor = IntToColor(BACK_COLOR);
            TextArea.Styles[Style.LineNumber].ForeColor = IntToColor(FORE_COLOR);
            TextArea.Styles[Style.IndentGuide].ForeColor = IntToColor(FORE_COLOR);
            TextArea.Styles[Style.IndentGuide].BackColor = IntToColor(BACK_COLOR);

            var nums = TextArea.Margins[NUMBER_MARGIN];
            nums.Width = 30;
            nums.Type = MarginType.Number;
            nums.Sensitive = true;
            nums.Mask = 0;
        }

        private const int EXECUTED_MARKER = 1;
        private const int SLICED_MARKER = 2;
        private const int LIGHT_YELLOW = (int)0xFEFEBE;
        private const int LIGHT_PINK = (int)0xFFE6EE;

        private void InitMarkersStyles(Scintilla TextArea)
        {
            var execMarker = TextArea.Markers[EXECUTED_MARKER];
            execMarker.SetBackColor(IntToColor(LIGHT_YELLOW));

            var slicedMarker = TextArea.Markers[SLICED_MARKER];
            slicedMarker.SetBackColor(IntToColor(LIGHT_PINK));
        }

        private Color IntToColor(int rgb)
        {
            return Color.FromArgb(255, (byte)(rgb >> 16), (byte)(rgb >> 8), (byte)rgb);
        }
        #endregion
    }
}
