namespace ZYCLEANERV1
{
    partial class MainForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.lvResults = new System.Windows.Forms.ListView();
            this.colCategory = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colPath = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colSize = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colStatus = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.progressBar = new System.Windows.Forms.ProgressBar();
            this.txtLog = new System.Windows.Forms.TextBox();
            this.lblStatus = new System.Windows.Forms.Label();
            this.lblFooter = new System.Windows.Forms.Label();
            this.btnClean = new NMSModToggler.Resources.AdvancedButton();
            this.btnScan = new NMSModToggler.Resources.AdvancedButton();
            this.label1 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // lvResults
            // 
            this.lvResults.BackColor = System.Drawing.SystemColors.Menu;
            this.lvResults.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.colCategory,
            this.colPath,
            this.colSize,
            this.colStatus});
            this.lvResults.HideSelection = false;
            this.lvResults.Location = new System.Drawing.Point(207, 119);
            this.lvResults.Name = "lvResults";
            this.lvResults.Size = new System.Drawing.Size(671, 127);
            this.lvResults.TabIndex = 2;
            this.lvResults.UseCompatibleStateImageBehavior = false;
            this.lvResults.View = System.Windows.Forms.View.Details;
            // 
            // colCategory
            // 
            this.colCategory.Text = "Category";
            // 
            // colPath
            // 
            this.colPath.Text = "Path";
            // 
            // colSize
            // 
            this.colSize.Text = "Size";
            // 
            // colStatus
            // 
            this.colStatus.Text = "Status";
            // 
            // progressBar
            // 
            this.progressBar.Location = new System.Drawing.Point(207, 282);
            this.progressBar.Name = "progressBar";
            this.progressBar.Size = new System.Drawing.Size(671, 31);
            this.progressBar.TabIndex = 3;
            // 
            // txtLog
            // 
            this.txtLog.Location = new System.Drawing.Point(214, 428);
            this.txtLog.Multiline = true;
            this.txtLog.Name = "txtLog";
            this.txtLog.ReadOnly = true;
            this.txtLog.Size = new System.Drawing.Size(398, 85);
            this.txtLog.TabIndex = 4;
            // 
            // lblStatus
            // 
            this.lblStatus.AutoSize = true;
            this.lblStatus.BackColor = System.Drawing.Color.Transparent;
            this.lblStatus.Font = new System.Drawing.Font("Liberation Sans", 18F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblStatus.ForeColor = System.Drawing.Color.White;
            this.lblStatus.Location = new System.Drawing.Point(209, 252);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(92, 27);
            this.lblStatus.TabIndex = 5;
            this.lblStatus.Text = "Status:";
            // 
            // lblFooter
            // 
            this.lblFooter.AutoSize = true;
            this.lblFooter.BackColor = System.Drawing.Color.Transparent;
            this.lblFooter.ForeColor = System.Drawing.Color.White;
            this.lblFooter.Location = new System.Drawing.Point(5, 525);
            this.lblFooter.Name = "lblFooter";
            this.lblFooter.Size = new System.Drawing.Size(146, 13);
            this.lblFooter.TabIndex = 6;
            this.lblFooter.Text = "© 2025 ZYCLEANER Project";
            // 
            // btnClean
            // 
            this.btnClean.BorderColor = System.Drawing.Color.White;
            this.btnClean.BorderRadius = 20;
            this.btnClean.BorderSize = 2;
            this.btnClean.ButtonColor1 = System.Drawing.Color.MidnightBlue;
            this.btnClean.ButtonColor2 = System.Drawing.Color.Navy;
            this.btnClean.FlatAppearance.BorderSize = 0;
            this.btnClean.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnClean.Font = new System.Drawing.Font("Liberation Sans", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnClean.ForeColor = System.Drawing.Color.White;
            this.btnClean.GradientDirection = System.Drawing.Drawing2D.LinearGradientMode.Vertical;
            this.btnClean.HoverColor1 = System.Drawing.Color.Blue;
            this.btnClean.HoverColor2 = System.Drawing.Color.MediumPurple;
            this.btnClean.Icon = null;
            this.btnClean.IconSize = 20;
            this.btnClean.IconSpacing = 5;
            this.btnClean.Location = new System.Drawing.Point(728, 330);
            this.btnClean.Name = "btnClean";
            this.btnClean.PressedColor1 = System.Drawing.Color.RoyalBlue;
            this.btnClean.PressedColor2 = System.Drawing.Color.SlateBlue;
            this.btnClean.Size = new System.Drawing.Size(150, 53);
            this.btnClean.TabIndex = 1;
            this.btnClean.Text = "Clean Selected";
            this.btnClean.TextColor = System.Drawing.Color.White;
            this.btnClean.UseVisualStyleBackColor = true;
            // 
            // btnScan
            // 
            this.btnScan.BorderColor = System.Drawing.Color.White;
            this.btnScan.BorderRadius = 20;
            this.btnScan.BorderSize = 2;
            this.btnScan.ButtonColor1 = System.Drawing.Color.MidnightBlue;
            this.btnScan.ButtonColor2 = System.Drawing.Color.Navy;
            this.btnScan.FlatAppearance.BorderSize = 0;
            this.btnScan.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnScan.Font = new System.Drawing.Font("Liberation Sans", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnScan.ForeColor = System.Drawing.Color.White;
            this.btnScan.GradientDirection = System.Drawing.Drawing2D.LinearGradientMode.Vertical;
            this.btnScan.HoverColor1 = System.Drawing.Color.Blue;
            this.btnScan.HoverColor2 = System.Drawing.Color.MediumPurple;
            this.btnScan.Icon = null;
            this.btnScan.IconSize = 20;
            this.btnScan.IconSpacing = 5;
            this.btnScan.Location = new System.Drawing.Point(207, 330);
            this.btnScan.Name = "btnScan";
            this.btnScan.PressedColor1 = System.Drawing.Color.RoyalBlue;
            this.btnScan.PressedColor2 = System.Drawing.Color.SlateBlue;
            this.btnScan.Size = new System.Drawing.Size(150, 53);
            this.btnScan.TabIndex = 0;
            this.btnScan.Text = "Scan Caches";
            this.btnScan.TextColor = System.Drawing.Color.White;
            this.btnScan.UseVisualStyleBackColor = true;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.BackColor = System.Drawing.Color.Transparent;
            this.label1.Font = new System.Drawing.Font("Liberation Sans", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.ForeColor = System.Drawing.Color.White;
            this.label1.Location = new System.Drawing.Point(211, 408);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(39, 17);
            this.label1.TabIndex = 7;
            this.label1.Text = "Log:";
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(40)))), ((int)(((byte)(40)))), ((int)(((byte)(40)))));
            this.BackgroundImage = global::ZYCLEANERV1.Properties.Resources.thumbnail;
            this.ClientSize = new System.Drawing.Size(1081, 542);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.lblFooter);
            this.Controls.Add(this.lblStatus);
            this.Controls.Add(this.txtLog);
            this.Controls.Add(this.progressBar);
            this.Controls.Add(this.lvResults);
            this.Controls.Add(this.btnClean);
            this.Controls.Add(this.btnScan);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "ZYCLEANER by CEZEY";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private NMSModToggler.Resources.AdvancedButton btnScan;
        private NMSModToggler.Resources.AdvancedButton btnClean;
        private System.Windows.Forms.ListView lvResults;
        private System.Windows.Forms.ColumnHeader colCategory;
        private System.Windows.Forms.ColumnHeader colPath;
        private System.Windows.Forms.ColumnHeader colSize;
        private System.Windows.Forms.ColumnHeader colStatus;
        private System.Windows.Forms.ProgressBar progressBar;
        private System.Windows.Forms.TextBox txtLog;
        private System.Windows.Forms.Label lblStatus;
        private System.Windows.Forms.Label lblFooter;
        private System.Windows.Forms.Label label1;
    }
}

