namespace Social_media
{
    partial class Form1
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            this.mainTabControl = new System.Windows.Forms.TabControl();
            this.tabVisualization = new System.Windows.Forms.TabPage();
            this.tabStats = new System.Windows.Forms.TabPage();
            this.tabBFS = new System.Windows.Forms.TabPage();
            this.tabPath = new System.Windows.Forms.TabPage();
            // Viz
            this.vizSidePanel = new System.Windows.Forms.Panel();
            this.graphPanel = new System.Windows.Forms.Panel();
            this.lblUsers = new System.Windows.Forms.Label();
            this.nudUsers = new System.Windows.Forms.NumericUpDown();
            this.lblEdges = new System.Windows.Forms.Label();
            this.nudEdges = new System.Windows.Forms.NumericUpDown();
            this.btnGenerate = new System.Windows.Forms.Button();
            this.chkShowLabels = new System.Windows.Forms.CheckBox();
            this.lblLegend = new System.Windows.Forms.Label();
            this.lblGraphInfo = new System.Windows.Forms.Label();
            // Stats
            this.statsSidePanel = new System.Windows.Forms.Panel();
            this.statsPanel = new System.Windows.Forms.Panel();
            this.lblStatsTitle = new System.Windows.Forms.Label();
            this.btnCalcStats = new System.Windows.Forms.Button();
            this.btnCalcDiameter = new System.Windows.Forms.Button();
            this.progressBar = new System.Windows.Forms.ProgressBar();
            this.lblProgress = new System.Windows.Forms.Label();
            this.txtStats = new System.Windows.Forms.RichTextBox();
            this.degreeChartPanel = new System.Windows.Forms.Panel();
            this.lblChartTitle = new System.Windows.Forms.Label();
            // BFS
            this.bfsSidePanel = new System.Windows.Forms.Panel();
            this.bfsHighlightPanel = new System.Windows.Forms.Panel();
            this.lblBfsVizTitle = new System.Windows.Forms.Label();
            this.lblBfsSource = new System.Windows.Forms.Label();
            this.nudBfsSource = new System.Windows.Forms.NumericUpDown();
            this.lblBfsDepth = new System.Windows.Forms.Label();
            this.nudBfsDepth = new System.Windows.Forms.NumericUpDown();
            this.btnBfs = new System.Windows.Forms.Button();
            this.txtBfsResult = new System.Windows.Forms.RichTextBox();
            // Path
            this.pathSidePanel = new System.Windows.Forms.Panel();
            this.pathVizPanel = new System.Windows.Forms.Panel();
            this.lblPathVizTitle = new System.Windows.Forms.Label();
            this.lblPathFrom = new System.Windows.Forms.Label();
            this.nudPathFrom = new System.Windows.Forms.NumericUpDown();
            this.lblPathTo = new System.Windows.Forms.Label();
            this.nudPathTo = new System.Windows.Forms.NumericUpDown();
            this.btnFindPath = new System.Windows.Forms.Button();
            this.txtPathResult = new System.Windows.Forms.RichTextBox();

            ((System.ComponentModel.ISupportInitialize)this.nudUsers).BeginInit();
            ((System.ComponentModel.ISupportInitialize)this.nudEdges).BeginInit();
            ((System.ComponentModel.ISupportInitialize)this.nudBfsSource).BeginInit();
            ((System.ComponentModel.ISupportInitialize)this.nudBfsDepth).BeginInit();
            ((System.ComponentModel.ISupportInitialize)this.nudPathFrom).BeginInit();
            ((System.ComponentModel.ISupportInitialize)this.nudPathTo).BeginInit();
            this.SuspendLayout();

            // ── mainTabControl ───────────────────────────────────────
            this.mainTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.mainTabControl.Location = new System.Drawing.Point(0, 0);
            this.mainTabControl.Size = new System.Drawing.Size(1200, 800);
            this.mainTabControl.TabIndex = 0;
            this.mainTabControl.Controls.Add(this.tabVisualization);
            this.mainTabControl.Controls.Add(this.tabStats);
            this.mainTabControl.Controls.Add(this.tabBFS);
            this.mainTabControl.Controls.Add(this.tabPath);

            // ── TAB 1: Граф ──────────────────────────────────────────
            this.tabVisualization.Text = "Граф";
            this.tabVisualization.Controls.Add(this.graphPanel);
            this.tabVisualization.Controls.Add(this.vizSidePanel);

            this.vizSidePanel.Dock = System.Windows.Forms.DockStyle.Left;
            this.vizSidePanel.Width = 200;
            this.vizSidePanel.Controls.Add(this.lblLegend);
            this.vizSidePanel.Controls.Add(this.lblGraphInfo);
            this.vizSidePanel.Controls.Add(this.chkShowLabels);
            this.vizSidePanel.Controls.Add(this.btnGenerate);
            this.vizSidePanel.Controls.Add(this.nudEdges);
            this.vizSidePanel.Controls.Add(this.lblEdges);
            this.vizSidePanel.Controls.Add(this.nudUsers);
            this.vizSidePanel.Controls.Add(this.lblUsers);

            this.lblUsers.AutoSize = true;
            this.lblUsers.Location = new System.Drawing.Point(10, 10);
            this.lblUsers.Text = "Пользователей:";

            this.nudUsers.Location = new System.Drawing.Point(10, 30);
            this.nudUsers.Width = 170;
            this.nudUsers.Minimum = 10;
            this.nudUsers.Maximum = 500;
            this.nudUsers.Value = 500;

            this.lblEdges.AutoSize = true;
            this.lblEdges.Location = new System.Drawing.Point(10, 60);
            this.lblEdges.Text = "Связей:";

            this.nudEdges.Location = new System.Drawing.Point(10, 80);
            this.nudEdges.Width = 170;
            this.nudEdges.Minimum = 10;
            this.nudEdges.Maximum = 2000;
            this.nudEdges.Value = 2000;

            this.btnGenerate.Location = new System.Drawing.Point(10, 115);
            this.btnGenerate.Size = new System.Drawing.Size(170, 30);
            this.btnGenerate.Text = "Сгенерировать граф";
            this.btnGenerate.Click += new System.EventHandler(this.BtnGenerate_Click);

            this.chkShowLabels.AutoSize = true;
            this.chkShowLabels.Location = new System.Drawing.Point(10, 155);
            this.chkShowLabels.Text = "Показывать метки";

            this.lblGraphInfo.Location = new System.Drawing.Point(10, 185);
            this.lblGraphInfo.Size = new System.Drawing.Size(175, 200);
            this.lblGraphInfo.Text = "Граф не загружен";

            this.lblLegend.Location = new System.Drawing.Point(10, 395);
            this.lblLegend.Size = new System.Drawing.Size(175, 80);
            this.lblLegend.Text = "Цвет узла:\n● синий — мало друзей\n● красный — много друзей";
            this.lblLegend.Font = new System.Drawing.Font("Segoe UI", 8.5F);

            this.graphPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.graphPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;



            // ── Form ─────────────────────────────────────────────────
            this.ClientSize = new System.Drawing.Size(1200, 800);
            this.MinimumSize = new System.Drawing.Size(900, 600);
            this.Controls.Add(this.mainTabControl);
            this.Text = "Анализатор социальной сети";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;

            ((System.ComponentModel.ISupportInitialize)this.nudUsers).EndInit();
            ((System.ComponentModel.ISupportInitialize)this.nudEdges).EndInit();
            ((System.ComponentModel.ISupportInitialize)this.nudBfsSource).EndInit();
            ((System.ComponentModel.ISupportInitialize)this.nudBfsDepth).EndInit();
            ((System.ComponentModel.ISupportInitialize)this.nudPathFrom).EndInit();
            ((System.ComponentModel.ISupportInitialize)this.nudPathTo).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion

        private System.Windows.Forms.TabControl mainTabControl;
        private System.Windows.Forms.TabPage tabVisualization, tabStats, tabBFS, tabPath;
        // Viz
        private System.Windows.Forms.Panel vizSidePanel, graphPanel;
        private System.Windows.Forms.Label lblUsers, lblEdges, lblGraphInfo, lblLegend;
        private System.Windows.Forms.NumericUpDown nudUsers, nudEdges;
        private System.Windows.Forms.Button btnGenerate;
        private System.Windows.Forms.CheckBox chkShowLabels;
        // Stats
        private System.Windows.Forms.Panel statsSidePanel, statsPanel, degreeChartPanel;
        private System.Windows.Forms.Label lblStatsTitle, lblChartTitle, lblProgress;
        private System.Windows.Forms.Button btnCalcStats, btnCalcDiameter;
        private System.Windows.Forms.ProgressBar progressBar;
        private System.Windows.Forms.RichTextBox txtStats;
        // BFS
        private System.Windows.Forms.Panel bfsSidePanel, bfsHighlightPanel;
        private System.Windows.Forms.Label lblBfsSource, lblBfsDepth, lblBfsVizTitle;
        private System.Windows.Forms.NumericUpDown nudBfsSource, nudBfsDepth;
        private System.Windows.Forms.Button btnBfs;
        private System.Windows.Forms.RichTextBox txtBfsResult;
        // Path
        private System.Windows.Forms.Panel pathSidePanel, pathVizPanel;
        private System.Windows.Forms.Label lblPathFrom, lblPathTo, lblPathVizTitle;
        private System.Windows.Forms.NumericUpDown nudPathFrom, nudPathTo;
        private System.Windows.Forms.Button btnFindPath;
        private System.Windows.Forms.RichTextBox txtPathResult;
    }
}
