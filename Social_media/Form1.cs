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
            // ════════════════════════════════════════════════════════════
        // Размещение узлов — случайно по всей площади панели
        // ════════════════════════════════════════════════════════════
        private void LayoutNodes()
        {
            if (_graph == null) return;
            int n = _graph.UserCount;
            _nodePos = new PointF[n];

            int W = Math.Max(graphPanel.Width, 100);
            int H = Math.Max(graphPanel.Height, 100);
            int margin = 20;
            var rng = new Random(42);

            for (int i = 0; i < n; i++)
            {
                float x = margin + (float)rng.NextDouble() * (W - margin * 2);
                float y = margin + (float)rng.NextDouble() * (H - margin * 2);
                _nodePos[i] = new PointF(x, y);
            }
        }

        private void UpdateGraphInfo()
        {
            if (_graph == null) return;
            int pop = _graph.MostPopularUser();
            lblGraphInfo.Text =
                $"Пользователей: {_graph.UserCount}\n" +
                $"Связей: {_graph.EdgeCount()}\n" +
                $"Компонент: {_graph.CountComponents()}\n" +
                $"Связный: {(_graph.IsConnected() ? "Да" : "Нет")}\n" +
                $"Ср. друзей: {_graph.AverageDegree():F2}\n" +
                $"Самый популярный:\n  {_graph.UserNames[pop]}\n  ({_graph.Degree(pop)} друзей)";
        }

        // ════════════════════════════════════════════════════════════
        // Отрисовка основного графа
        // ════════════════════════════════════════════════════════════
        private void GraphPanel_Paint(object? sender, PaintEventArgs e)
        {
            if (_graph == null || _nodePos == null) return;
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            DrawEdges(g, _nodePos, _graph, null, null);
            DrawNodes(g, _nodePos, _graph, null, null, null);
        }

        // ════════════════════════════════════════════════════════════
        // Отрисовка BFS-подсветки
        // ════════════════════════════════════════════════════════════
        private void BfsHighlightPanel_Paint(object? sender, PaintEventArgs e)
        {
            if (_graph == null || _nodePos == null) return;
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            // Пересчитать позиции под размер этой панели
            var pos = ScalePositions(_nodePos, graphPanel.Size, bfsHighlightPanel.Size);

            DrawEdges(g, pos, _graph, null, null);
            DrawNodes(g, pos, _graph, _bfsLevels, _bfsSource == -1 ? (int?)null : _bfsSource, null);
        }

        // ════════════════════════════════════════════════════════════
        // Отрисовка пути
        // ════════════════════════════════════════════════════════════
        private void PathVizPanel_Paint(object? sender, PaintEventArgs e)
        {
            if (_graph == null || _nodePos == null) return;
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            var pos = ScalePositions(_nodePos, graphPanel.Size, pathVizPanel.Size);

            DrawEdges(g, pos, _graph, null, _shortestPath);
            DrawNodes(g, pos, _graph, null, null, _shortestPath);
        }

        // ════════════════════════════════════════════════════════════
        // Общие методы рисования
        // ════════════════════════════════════════════════════════════
        private void DrawEdges(Graphics g, PointF[] pos, SocialGraph graph,
            Dictionary<int, List<int>>? bfsLevels, List<int>? path)
        {
            var pathEdges = new HashSet<(int, int)>();
            if (path != null)
                for (int i = 0; i < path.Count - 1; i++)
                {
                    pathEdges.Add((path[i], path[i + 1]));
                    pathEdges.Add((path[i + 1], path[i]));
                }

            using var penNormal = new Pen(Color.FromArgb(120, 150, 150, 160), 1f);
            using var penPath = new Pen(Color.FromArgb(255, 220, 80, 0), 3f);

            for (int u = 0; u < graph.UserCount; u++)
            {
                foreach (int v in graph.GetFriends(u))
                {
                    if (v <= u) continue;
                    bool isPath = pathEdges.Contains((u, v));
                    g.DrawLine(isPath ? penPath : penNormal, pos[u], pos[v]);
                }
            }
        }

        private void DrawNodes(Graphics g, PointF[] pos, SocialGraph graph,
            Dictionary<int, List<int>>? bfsLevels, int? bfsSource, List<int>? path)
        {
            var pathSet = new HashSet<int>();
            if (path != null) foreach (int n in path) pathSet.Add(n);

            var bfsNodeLevel = new Dictionary<int, int>();
            if (bfsSource.HasValue) bfsNodeLevel[bfsSource.Value] = 0;
            if (bfsLevels != null)
                foreach (var kv in bfsLevels)
                    foreach (int n in kv.Value)
                        bfsNodeLevel[n] = kv.Key;

            bool showLabels = chkShowLabels.Checked;

            // Найдём макс. степень для нормировки размера
            int maxDeg = 1;
            for (int i = 0; i < graph.UserCount; i++)
                if (graph.Degree(i) > maxDeg) maxDeg = graph.Degree(i);

            for (int i = 0; i < graph.UserCount; i++)
            {
                int deg = graph.Degree(i);

                // Размер: от 3 до 10 пикселей в зависимости от степени
                float r = 3f + (float)deg / maxDeg * 7f;

                Color nodeColor;

                if (pathSet.Contains(i))
                {
                    nodeColor = (i == path![0] || i == path[^1])
                        ? Color.FromArgb(255, 60, 60)
                        : Color.FromArgb(255, 180, 0);
                    r = Math.Max(r, 7f);
                }
                else if (bfsNodeLevel.TryGetValue(i, out int lvl))
                {
                    int ci = Math.Min(lvl, LevelColors.Length - 1);
                    nodeColor = LevelColors[ci];
                }
                else
                {
                    // Цвет по степени: мало — синий, много — красный
                    float t = (float)deg / maxDeg;
                    int red = (int)(50 + t * 200);
                    int green = (int)(100 - t * 60);
                    int blue = (int)(200 - t * 160);
                    nodeColor = Color.FromArgb(200, red, green, blue);
                }

                float x = pos[i].X - r;
                float y = pos[i].Y - r;
                float d = r * 2;

                using var brush = new SolidBrush(nodeColor);
                g.FillEllipse(brush, x, y, d, d);

                // Тонкая обводка
                using var pen = new Pen(Color.FromArgb(80, 0, 0, 0), 0.5f);
                g.DrawEllipse(pen, x, y, d, d);

                if (showLabels || i == _hoveredNode)
                {
                    string label = $"{i + 1}";
                    using var font = new Font("Segoe UI", 7f);
                    using var lblBrush = new SolidBrush(Color.Black);
                    g.DrawString(label, font, lblBrush, pos[i].X + r + 1, pos[i].Y - 6);
                }
            }

            if (bfsLevels != null && bfsSource.HasValue)
                DrawBfsLegend(g, bfsLevels);
        }

        private static void DrawBfsLegend(Graphics g, Dictionary<int, List<int>> levels)
        {
            int x = 10, y = 10;
            using var font = new Font("Segoe UI", 9f, FontStyle.Bold);
            using var bg = new SolidBrush(Color.FromArgb(160, 20, 20, 30));
            g.FillRectangle(bg, x - 4, y - 4, 180, (levels.Count + 2) * 22 + 8);

            using var white = new SolidBrush(Color.White);
            g.DrawString("Легенда BFS:", font, white, x, y);
            y += 22;

            // Источник
            using var srcBrush = new SolidBrush(LevelColors[0]);
            g.FillEllipse(srcBrush, x, y + 3, 12, 12);
            g.DrawString("Источник (d=0)", font, white, x + 16, y);
            y += 22;

            foreach (var kv in levels)
            {
                int ci = Math.Min(kv.Key, LevelColors.Length - 1);
                using var lb = new SolidBrush(LevelColors[ci]);
                g.FillEllipse(lb, x, y + 3, 12, 12);
                g.DrawString($"d={kv.Key}  ({kv.Value.Count} польз.)", font, white, x + 16, y);
                y += 22;
            }
        }

        // ════════════════════════════════════════════════════════════
        // Масштабирование позиций под другой размер панели
        // ════════════════════════════════════════════════════════════
        private static PointF[] ScalePositions(PointF[] src, Size srcSize, Size dstSize)
        {
            if (srcSize.Width == 0 || srcSize.Height == 0) return src;
            float sx = (float)dstSize.Width / srcSize.Width;
            float sy = (float)dstSize.Height / srcSize.Height;
            var result = new PointF[src.Length];
            for (int i = 0; i < src.Length; i++)
                result[i] = new PointF(src[i].X * sx, src[i].Y * sy);
            return result;
        }

        // ════════════════════════════════════════════════════════════
        // Hover на графе
        // ════════════════════════════════════════════════════════════
        private void GraphPanel_MouseMove(object? sender, MouseEventArgs e)
        {
            // hover отключён — при 500 узлах перерисовка на каждое движение мыши тормозит
        }

        private void GraphPanel_MouseClick(object? sender, MouseEventArgs e)
        {
            if (_graph == null || _nodePos == null) return;
            int node = FindNearestNode(_nodePos, e.Location, 15f);
            if (node < 0) return;

            // Показать информацию об узле в тултипе через lblGraphInfo
            lblGraphInfo.Text =
                $"Пользователь: {_graph.UserNames[node]}\n" +
                $"ID: {node + 1}\n" +
                $"Друзей: {_graph.Degree(node)}\n\n" +
                $"Нажмите «Сгенерировать»\nчтобы сбросить выбор.";
        }

        private static int FindNearestNode(PointF[] pos, Point mouse, float threshold)
        {
            float best = threshold * threshold;
            int found = -1;
            for (int i = 0; i < pos.Length; i++)
            {
                float dx = pos[i].X - mouse.X;
                float dy = pos[i].Y - mouse.Y;
                float d2 = dx * dx + dy * dy;
                if (d2 < best) { best = d2; found = i; }
            }
            return found;
        }
    }
}



