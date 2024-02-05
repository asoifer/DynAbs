using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows.Forms;

namespace DynAbs.DesktopApp.Browser
{
    public partial class SourcesAndTargets : Form
    {
        Dictionary<int, string> _idToFiles;
        ISet<Stmt> _currentSources;
        ISet<Stmt> _currentTargets;
        ComplexBrowser _complexBrowser;

        public SourcesAndTargets(ComplexBrowser complexBrowser, Dictionary<int, string> idToFiles, List<TreeNode> solutionFiles)
        {
            _idToFiles = idToFiles;
            _complexBrowser = complexBrowser;

            InitializeComponent();

            // Armamos el árbol desde este lado
            tvSource.Nodes.AddRange(solutionFiles.Select(x => Utils.CopyTreeNode(x)).ToArray());
            tvTarget.Nodes.AddRange(solutionFiles.Select(x => Utils.CopyTreeNode(x)).ToArray());
        }

        private void SourcesAndTargets_Load(object sender, EventArgs e)
        {
            
        }

        public void ShowFor(string currentLine, ISet<Stmt> sources, ISet<Stmt> targets)
        {
            _currentSources = sources;
            _currentTargets = targets;

            Utils.RefrescarMenuSlice(_idToFiles, tvSource, sources);
            Utils.RefrescarMenuSlice(_idToFiles, tvTarget, targets);

            lbSource.DataSource = null;
            lbSource.Refresh();
            lbTarget.DataSource = null;
            lbTarget.Refresh();

            lblCurrentLine.Text = "Analizando: " + (currentLine.Length > 80 ? currentLine.Substring(0, 80) + "..." : currentLine);

            this.Show();
        }

        private void tvSource_AfterSelect(object sender, TreeViewEventArgs e)
        {
            Utils.SetListBox(_idToFiles, tvSource, lbSource, _currentSources);
        }

        private void tvTarget_AfterSelect(object sender, TreeViewEventArgs e)
        {
            Utils.SetListBox(_idToFiles, tvTarget, lbTarget, _currentTargets);
        }

        private void sourcesAndTargets_FormClosing(object sender, FormClosingEventArgs e)
        {	
            // Prevent dispose, as this form can be re-used
            if (e.CloseReason != CloseReason.FormOwnerClosing)
            {
                _complexBrowser.Select();

                e.Cancel = true;
                Hide();
            }
        }

        private void lbSource_SelectedIndexChanged(object sender, EventArgs e)
        {
            var selectedItem = (CustomListBoxItem)lbSource.SelectedItem;
            if (selectedItem != null)
                _complexBrowser.CargarDocumentoByVertex(selectedItem.key);
        }

        private void lbTarget_SelectedIndexChanged(object sender, EventArgs e)
        {
            var selectedItem = (CustomListBoxItem)lbTarget.SelectedItem;
            if (selectedItem != null)
                _complexBrowser.CargarDocumentoByVertex(selectedItem.key);
        }
    }
}
