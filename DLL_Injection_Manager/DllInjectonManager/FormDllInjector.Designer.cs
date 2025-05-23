namespace DLL_Injection_Manager.DllInjectonManager
{
    partial class FormDllInjector
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormDllInjector));
            listBoxProcesses = new ListBox();
            btnRefresh = new Button();
            btnBrowse = new Button();
            btnInject = new Button();
            lblStatus = new Label();
            txtSearch = new TextBox();
            label3 = new Label();
            listBoxRecentDlls = new ListBox();
            SuspendLayout();
            // 
            // listBoxProcesses
            // 
            listBoxProcesses.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            listBoxProcesses.BackColor = SystemColors.ButtonShadow;
            listBoxProcesses.FormattingEnabled = true;
            listBoxProcesses.ItemHeight = 15;
            listBoxProcesses.Location = new Point(12, 41);
            listBoxProcesses.Name = "listBoxProcesses";
            listBoxProcesses.Size = new Size(456, 109);
            listBoxProcesses.TabIndex = 0;
            // 
            // btnRefresh
            // 
            btnRefresh.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnRefresh.Location = new Point(396, 12);
            btnRefresh.Name = "btnRefresh";
            btnRefresh.Size = new Size(75, 23);
            btnRefresh.TabIndex = 2;
            btnRefresh.Text = "Refresh";
            btnRefresh.UseVisualStyleBackColor = true;
            btnRefresh.Click += btnRefresh_Click;
            // 
            // btnBrowse
            // 
            btnBrowse.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnBrowse.Location = new Point(12, 271);
            btnBrowse.Name = "btnBrowse";
            btnBrowse.Size = new Size(125, 23);
            btnBrowse.TabIndex = 5;
            btnBrowse.Text = "Browse for DLL's...";
            btnBrowse.UseVisualStyleBackColor = true;
            btnBrowse.Click += btnBrowse_Click;
            // 
            // btnInject
            // 
            btnInject.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnInject.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            btnInject.Location = new Point(393, 296);
            btnInject.Name = "btnInject";
            btnInject.Size = new Size(75, 32);
            btnInject.TabIndex = 6;
            btnInject.Text = "Inject DLL";
            btnInject.UseVisualStyleBackColor = true;
            btnInject.Click += btnInject_Click;
            // 
            // lblStatus
            // 
            lblStatus.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            lblStatus.AutoSize = true;
            lblStatus.Location = new Point(15, 305);
            lblStatus.MaximumSize = new Size(375, 0);
            lblStatus.Name = "lblStatus";
            lblStatus.Size = new Size(94, 15);
            lblStatus.TabIndex = 7;
            lblStatus.Text = "Ready to inject...";
            // 
            // txtSearch
            // 
            txtSearch.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtSearch.BackColor = SystemColors.ButtonShadow;
            txtSearch.Location = new Point(76, 12);
            txtSearch.Name = "txtSearch";
            txtSearch.PlaceholderText = "Search by process name...";
            txtSearch.Size = new Size(314, 23);
            txtSearch.TabIndex = 8;
            txtSearch.TextChanged += txtSearch_TextChanged;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(15, 15);
            label3.Name = "label3";
            label3.Size = new Size(45, 15);
            label3.TabIndex = 9;
            label3.Text = "Search:";
            // 
            // listBoxRecentDlls
            // 
            listBoxRecentDlls.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            listBoxRecentDlls.BackColor = SystemColors.ButtonShadow;
            listBoxRecentDlls.FormattingEnabled = true;
            listBoxRecentDlls.ItemHeight = 15;
            listBoxRecentDlls.Location = new Point(12, 156);
            listBoxRecentDlls.Name = "listBoxRecentDlls";
            listBoxRecentDlls.Size = new Size(456, 109);
            listBoxRecentDlls.TabIndex = 10;
            // 
            // FormDllInjector
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = SystemColors.ScrollBar;
            ClientSize = new Size(480, 357);
            Controls.Add(listBoxRecentDlls);
            Controls.Add(label3);
            Controls.Add(txtSearch);
            Controls.Add(lblStatus);
            Controls.Add(btnInject);
            Controls.Add(btnBrowse);
            Controls.Add(btnRefresh);
            Controls.Add(listBoxProcesses);
            Icon = (Icon)resources.GetObject("$this.Icon");
            MinimumSize = new Size(400, 396);
            Name = "FormDllInjector";
            StartPosition = FormStartPosition.CenterParent;
            Text = "DLL Injection Manager";
            Load += FormDllInjector_Load;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private ListBox listBoxProcesses;
        private Button btnRefresh;
        private Button btnBrowse;
        private Button btnInject;
        private Label lblStatus;
        private TextBox txtSearch;
        private Label label3;
        private ListBox listBoxRecentDlls;
    }
}
