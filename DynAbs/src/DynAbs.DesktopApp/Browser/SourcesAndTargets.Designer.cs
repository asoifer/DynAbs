namespace DynAbs.DesktopApp.Browser
{
    partial class SourcesAndTargets
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
            this.tvSource = new System.Windows.Forms.TreeView();
            this.lbSource = new System.Windows.Forms.ListBox();
            this.tvTarget = new System.Windows.Forms.TreeView();
            this.lbTarget = new System.Windows.Forms.ListBox();
            this.panel1 = new System.Windows.Forms.Panel();
            this.label1 = new System.Windows.Forms.Label();
            this.panel2 = new System.Windows.Forms.Panel();
            this.label2 = new System.Windows.Forms.Label();
            this.lblCurrentLine = new System.Windows.Forms.Label();
            this.panel1.SuspendLayout();
            this.panel2.SuspendLayout();
            this.SuspendLayout();
            // 
            // tvSource
            // 
            this.tvSource.Location = new System.Drawing.Point(12, 27);
            this.tvSource.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.tvSource.Name = "tvSource";
            this.tvSource.Size = new System.Drawing.Size(195, 469);
            this.tvSource.TabIndex = 0;
            this.tvSource.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.tvSource_AfterSelect);
            // 
            // lbSource
            // 
            this.lbSource.FormattingEnabled = true;
            this.lbSource.ItemHeight = 15;
            this.lbSource.Location = new System.Drawing.Point(211, 27);
            this.lbSource.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.lbSource.Name = "lbSource";
            this.lbSource.Size = new System.Drawing.Size(312, 469);
            this.lbSource.TabIndex = 1;
            this.lbSource.SelectedIndexChanged += new System.EventHandler(this.lbSource_SelectedIndexChanged);
            // 
            // tvTarget
            // 
            this.tvTarget.Location = new System.Drawing.Point(12, 27);
            this.tvTarget.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.tvTarget.Name = "tvTarget";
            this.tvTarget.Size = new System.Drawing.Size(195, 469);
            this.tvTarget.TabIndex = 2;
            this.tvTarget.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.tvTarget_AfterSelect);
            // 
            // lbTarget
            // 
            this.lbTarget.FormattingEnabled = true;
            this.lbTarget.ItemHeight = 15;
            this.lbTarget.Location = new System.Drawing.Point(212, 27);
            this.lbTarget.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.lbTarget.Name = "lbTarget";
            this.lbTarget.Size = new System.Drawing.Size(312, 469);
            this.lbTarget.TabIndex = 3;
            this.lbTarget.SelectedIndexChanged += new System.EventHandler(this.lbTarget_SelectedIndexChanged);
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.label1);
            this.panel1.Controls.Add(this.tvSource);
            this.panel1.Controls.Add(this.lbSource);
            this.panel1.Location = new System.Drawing.Point(9, 34);
            this.panel1.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(551, 505);
            this.panel1.TabIndex = 4;
            this.panel1.Tag = "";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Arial", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.label1.Location = new System.Drawing.Point(9, 3);
            this.label1.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(114, 19);
            this.label1.TabIndex = 2;
            this.label1.Text = "Depends on...";
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.label2);
            this.panel2.Controls.Add(this.lbTarget);
            this.panel2.Controls.Add(this.tvTarget);
            this.panel2.Location = new System.Drawing.Point(566, 34);
            this.panel2.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(574, 505);
            this.panel2.TabIndex = 5;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Arial", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.label2.Location = new System.Drawing.Point(9, 3);
            this.label2.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(161, 19);
            this.label2.TabIndex = 3;
            this.label2.Text = "It\'s dependency of...";
            // 
            // lblCurrentLine
            // 
            this.lblCurrentLine.AutoSize = true;
            this.lblCurrentLine.Font = new System.Drawing.Font("Arial", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.lblCurrentLine.Location = new System.Drawing.Point(10, 8);
            this.lblCurrentLine.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblCurrentLine.Name = "lblCurrentLine";
            this.lblCurrentLine.Size = new System.Drawing.Size(91, 19);
            this.lblCurrentLine.TabIndex = 6;
            this.lblCurrentLine.Text = "Analyzing:";
            // 
            // SourcesAndTargets
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.ClientSize = new System.Drawing.Size(1305, 548);
            this.Controls.Add(this.lblCurrentLine);
            this.Controls.Add(this.panel2);
            this.Controls.Add(this.panel1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "SourcesAndTargets";
            this.Text = "Edges From | Edges To";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.sourcesAndTargets_FormClosing);
            this.Load += new System.EventHandler(this.SourcesAndTargets_Load);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TreeView tvSource;
        private System.Windows.Forms.ListBox lbSource;
        private System.Windows.Forms.TreeView tvTarget;
        private System.Windows.Forms.ListBox lbTarget;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label lblCurrentLine;       
    }
}