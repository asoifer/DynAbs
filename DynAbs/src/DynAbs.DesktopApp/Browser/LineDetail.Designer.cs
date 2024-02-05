namespace DynAbs.DesktopApp.Browser
{
    partial class LineDetail
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.lblCurrentLine = new System.Windows.Forms.Label();
            this.lbStatements = new System.Windows.Forms.ListBox();
            this.tvDependencies = new System.Windows.Forms.TreeView();
            this.lbDependencies = new System.Windows.Forms.ListBox();
            this.SuspendLayout();
            // 
            // lblCurrentLine
            // 
            this.lblCurrentLine.AutoSize = true;
            this.lblCurrentLine.Font = new System.Drawing.Font("Arial Narrow", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblCurrentLine.Location = new System.Drawing.Point(12, 9);
            this.lblCurrentLine.Name = "lblCurrentLine";
            this.lblCurrentLine.Size = new System.Drawing.Size(65, 29);
            this.lblCurrentLine.TabIndex = 0;
            this.lblCurrentLine.Text = "Line: ";
            // 
            // lbStatements
            // 
            this.lbStatements.FormattingEnabled = true;
            this.lbStatements.ItemHeight = 20;
            this.lbStatements.Location = new System.Drawing.Point(17, 54);
            this.lbStatements.Name = "lbStatements";
            this.lbStatements.Size = new System.Drawing.Size(637, 624);
            this.lbStatements.TabIndex = 1;
            this.lbStatements.SelectedIndexChanged += new System.EventHandler(this.lbStatements_SelectedIndexChanged);
            // 
            // tvDependencies
            // 
            this.tvDependencies.Location = new System.Drawing.Point(660, 54);
            this.tvDependencies.Name = "tvDependencies";
            this.tvDependencies.Size = new System.Drawing.Size(250, 624);
            this.tvDependencies.TabIndex = 2;
            this.tvDependencies.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.tvDependencies_AfterSelect);
            // 
            // lbDependencies
            // 
            this.lbDependencies.FormattingEnabled = true;
            this.lbDependencies.ItemHeight = 20;
            this.lbDependencies.Location = new System.Drawing.Point(916, 54);
            this.lbDependencies.Name = "lbDependencies";
            this.lbDependencies.Size = new System.Drawing.Size(592, 624);
            this.lbDependencies.TabIndex = 3;
            this.lbDependencies.SelectedIndexChanged += new System.EventHandler(this.lbDependencies_SelectedIndexChanged);
            // 
            // LineDetail
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.ClientSize = new System.Drawing.Size(1513, 686);
            this.Controls.Add(this.tvDependencies);
            this.Controls.Add(this.lbDependencies);
            this.Controls.Add(this.lbStatements);
            this.Controls.Add(this.lblCurrentLine);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "LineDetail";
            this.Text = "Details";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.LineDetail_FormClosing);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblCurrentLine;
        private System.Windows.Forms.ListBox lbStatements;
        private System.Windows.Forms.TreeView tvDependencies;
        private System.Windows.Forms.ListBox lbDependencies;
    }
}