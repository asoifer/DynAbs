
namespace DynAbs.DesktopApp
{
    partial class Main
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.lblConfigFile = new System.Windows.Forms.Label();
            this.txtConfigFile = new System.Windows.Forms.TextBox();
            this.btnStart = new System.Windows.Forms.Button();
            this.btnNavigateSlicedDG = new System.Windows.Forms.Button();
            this.btnClean = new System.Windows.Forms.Button();
            this.lblStatus = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // lblConfigFile
            // 
            this.lblConfigFile.AutoSize = true;
            this.lblConfigFile.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.lblConfigFile.Location = new System.Drawing.Point(12, 15);
            this.lblConfigFile.Name = "lblConfigFile";
            this.lblConfigFile.Size = new System.Drawing.Size(122, 17);
            this.lblConfigFile.TabIndex = 0;
            this.lblConfigFile.Text = "Configuration file:";
            // 
            // txtConfigFile
            // 
            this.txtConfigFile.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.txtConfigFile.Location = new System.Drawing.Point(140, 12);
            this.txtConfigFile.Name = "txtConfigFile";
            this.txtConfigFile.Size = new System.Drawing.Size(594, 25);
            this.txtConfigFile.TabIndex = 1;
            // 
            // btnStart
            // 
            this.btnStart.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.btnStart.Location = new System.Drawing.Point(17, 57);
            this.btnStart.Name = "btnStart";
            this.btnStart.Size = new System.Drawing.Size(156, 42);
            this.btnStart.TabIndex = 2;
            this.btnStart.Text = "Start";
            this.btnStart.UseVisualStyleBackColor = true;
            this.btnStart.Click += new System.EventHandler(this.btnStart_Click);
            // 
            // btnNavigateSlicedDG
            // 
            this.btnNavigateSlicedDG.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.btnNavigateSlicedDG.Location = new System.Drawing.Point(179, 57);
            this.btnNavigateSlicedDG.Name = "btnNavigateSlicedDG";
            this.btnNavigateSlicedDG.Size = new System.Drawing.Size(156, 42);
            this.btnNavigateSlicedDG.TabIndex = 3;
            this.btnNavigateSlicedDG.Text = "Navigate Sliced DG";
            this.btnNavigateSlicedDG.UseVisualStyleBackColor = true;
            this.btnNavigateSlicedDG.Click += new System.EventHandler(this.btnNavigateSlicedDG_Click);
            // 
            // btnClean
            // 
            this.btnClean.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.btnClean.Location = new System.Drawing.Point(341, 57);
            this.btnClean.Name = "btnClean";
            this.btnClean.Size = new System.Drawing.Size(156, 42);
            this.btnClean.TabIndex = 4;
            this.btnClean.Text = "Clean";
            this.btnClean.UseVisualStyleBackColor = true;
            this.btnClean.Click += new System.EventHandler(this.btnClean_Click);
            // 
            // lblStatus
            // 
            this.lblStatus.AutoSize = true;
            this.lblStatus.Font = new System.Drawing.Font("Segoe UI", 9.75F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Italic))), System.Drawing.GraphicsUnit.Point);
            this.lblStatus.Location = new System.Drawing.Point(574, 70);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(113, 17);
            this.lblStatus.TabIndex = 5;
            this.lblStatus.Text = "Waiting for a job";
            // 
            // Main
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(750, 111);
            this.Controls.Add(this.lblStatus);
            this.Controls.Add(this.btnClean);
            this.Controls.Add(this.btnNavigateSlicedDG);
            this.Controls.Add(this.btnStart);
            this.Controls.Add(this.txtConfigFile);
            this.Controls.Add(this.lblConfigFile);
            this.Name = "Main";
            this.Text = "DynAbs";
            this.Load += new System.EventHandler(this.Main_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblConfigFile;
        private System.Windows.Forms.TextBox txtConfigFile;
        private System.Windows.Forms.Button btnStart;
        private System.Windows.Forms.Button btnNavigateSlicedDG;
        private System.Windows.Forms.Button btnClean;
        private System.Windows.Forms.Label lblStatus;
    }
}

