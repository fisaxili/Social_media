using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Social_media;

namespace Social_media
{
    public partial class Form1 : Form
    {
        // ── Граф и позиции узлов ─────────────────────────────────────
        private SocialGraph? _graph;
        private PointF[]? _nodePos;          // позиции узлов на canvas

        // ── Состояние BFS / пути ─────────────────────────────────────
        private Dictionary<int, List<int>>? _bfsLevels;   // результат BFS по уровням
        private List<int>? _shortestPath;                  // кратчайший путь
        private int _bfsSource = -1;
        private int _pathSource = -1, _pathTarget = -1;

        // ── Hover ────────────────────────────────────────────────────
        private int _hoveredNode = -1;

        // ── Статистика ───────────────────────────────────────────────
        private int[]? _degreeDistribution;
        private int _diameter = -1;

        // ── Цвета ────────────────────────────────────────────────────
        private static readonly Color[] LevelColors =
        {
            Color.FromArgb(255, 220, 50),   // 0 — источник
            Color.FromArgb(50, 200, 255),   // 1
            Color.FromArgb(50, 255, 150),   // 2
            Color.FromArgb(255, 120, 50),   // 3
            Color.FromArgb(200, 80, 255),   // 4
            Color.FromArgb(255, 80, 120),   // 5+
        };

        public Form1()
        {
            InitializeComponent();
            // Подключаем события рисования и мыши здесь, а не в Designer
            this.graphPanel.Paint += GraphPanel_Paint;
            this.graphPanel.MouseMove += GraphPanel_MouseMove;
            this.graphPanel.MouseClick += GraphPanel_MouseClick;
            this.bfsHighlightPanel.Paint += BfsHighlightPanel_Paint;
            this.pathVizPanel.Paint += PathVizPanel_Paint;
            this.degreeChartPanel.Paint += DegreeChartPanel_Paint;
            this.chkShowLabels.CheckedChanged += (s, e) => this.graphPanel.Invalidate();
            this.Load += Form1_Load;
        }

        private void Form1_Load(object? sender, EventArgs e)
        {
            // граф генерируется только по кнопке
        }

        // ════════════════════════════════════════════════════════════
        // Генерация графа
        // ════════════════════════════════════════════════════════════
        private void GenerateGraph()
        {
            int users = (int)nudUsers.Value;
            int edges = (int)nudEdges.Value;

            _graph = SocialGraph.GenerateRandom(users, edges);
            _bfsLevels = null;
            _shortestPath = null;
            _bfsSource = -1;
            _pathSource = -1;
            _pathTarget = -1;
            _degreeDistribution = null;
            _diameter = -1;

            // Обновить максимумы в NUD на других вкладках
            nudBfsSource.Maximum = users;
            nudPathFrom.Maximum = users;
            nudPathTo.Maximum = users;

            LayoutNodes();
            UpdateGraphInfo();

            graphPanel.Invalidate();
            bfsHighlightPanel.Invalidate();
            pathVizPanel.Invalidate();
            degreeChartPanel.Invalidate();
            txtStats.Clear();
            txtBfsResult.Clear();
            txtPathResult.Clear();
        }

        private void BtnGenerate_Click(object? sender, EventArgs e) => GenerateGraph();

        private void BtnShowAdjacency_Click(object? sender, EventArgs e)
        {
            if (_graph == null) { MessageBox.Show("Сначала сгенерируйте граф."); return; }

            var sb = new StringBuilder();
            int show = Math.Min(30, _graph.UserCount);
            for (int i = 0; i < show; i++)
            {
                var friends = _graph.GetFriends(i);
                var names = new string[friends.Count];
                for (int j = 0; j < friends.Count; j++)
                    names[j] = $"{friends[j] + 1}";
                sb.AppendLine($"{i + 1}: [{string.Join(", ", names)}]");
            }
            txtAdjacency.Text = sb.ToString();
        }
    }
}



