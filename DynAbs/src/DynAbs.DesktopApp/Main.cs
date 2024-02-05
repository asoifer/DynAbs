using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Serialization;

namespace DynAbs.DesktopApp
{
    public partial class Main : Form
    {
        Orchestrator orchestrator;
        string[] StatusText = { "Waiting for a job", "Loading solution", "Executing", "Done", "Failed" };

        public Main()
        {
            InitializeComponent();
        }

        private void Main_Load(object sender, EventArgs e)
        {
            var appSettings = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .Build();
            // TODO-NETCORE ver que pasa si hay más de uno
            var defaultConfigPaths = appSettings["defaultConfigPaths:0"];

            if (!string.IsNullOrWhiteSpace(defaultConfigPaths))
                txtConfigFile.Text = defaultConfigPaths;

            btnNavigateSlicedDG.Enabled = false;
            btnClean.Enabled = false;
            lblStatus.Text = StatusText[0];
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            if (txtConfigFile.Text.Length == 0)
                return;

            try
            {
                btnStart.Enabled = false;
                btnStart.Refresh();
                lblStatus.Text = StatusText[1];
                lblStatus.Refresh();

                var path = txtConfigFile.Text;
                var stream = System.IO.File.OpenRead(path.Trim());
                var serializer = new XmlSerializer(typeof(UserConfiguration));
                var userConfiguration = (UserConfiguration)serializer.Deserialize(stream);

                orchestrator = new Orchestrator(userConfiguration);
                orchestrator.UserInteraction = false;

                lblStatus.Text = StatusText[2];
                lblStatus.Refresh();
                orchestrator.Orchestrate();

                lblStatus.Text = StatusText[3];
            }
            catch (Exception ex)
            {
                lblStatus.Text = StatusText[4];
            }
            finally
            {
                btnClean.Enabled = true;
                btnNavigateSlicedDG.Enabled = true;
            }
        }

        private void btnNavigateSlicedDG_Click(object sender, EventArgs e)
        {
            var reducedSlicedDG = orchestrator.GetReducedSliceDependencyGraph();
            if (reducedSlicedDG != null)
            {
                var result = new Browser.ComplexBrowser(orchestrator.UserSolution, reducedSlicedDG, orchestrator.InstrumentationResult.fileIdToSyntaxTree,
                  orchestrator.InstrumentationResult.IdToFileDictionary, orchestrator.ExecutedStmts);
                result.Show();
            }
            else
                MessageBox.Show("There is not sliced dependency graph loaded");
        }

        private void btnClean_Click(object sender, EventArgs e)
        {
            orchestrator = null;
            btnStart.Enabled = true;
            btnClean.Enabled = false;
            btnNavigateSlicedDG.Enabled = false;
        }
    }
}
