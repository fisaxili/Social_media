using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Social_media;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace Social_media
{
    public partial class Form1 : Form
    {
        // ── Граф и позиции узлов 
        private SocialGraph? _graph;
        private PointF[]? _nodePos;          // позиции узлов на canvas

        // ── Состояние BFS / пути 
        private Dictionary<int, List<int>>? _bfsLevels;   // результат BFS по уровням
        private List<int>? _shortestPath;                  // кратчайший путь
        private int _bfsSource = -1;
        private int _pathSource = -1, _pathTarget = -1;

        // Hover 
        private int _hoveredNode = -1;

        //Статистика 
        private int[]? _degreeDistribution;
        private int _diameter = -1;

        // Цвета 
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

        // Генерация графа
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

        // Размещение узлов — случайно по всей площади панели
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

        // Отрисовка основного графа
        private void GraphPanel_Paint(object? sender, PaintEventArgs e)
        {
            if (_graph == null || _nodePos == null) return;
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            DrawEdges(g, _nodePos, _graph, null, null);
            DrawNodes(g, _nodePos, _graph, null, null, null);
        }

        // Отрисовка BFS-подсветки
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

        // Отрисовка пути
        private void PathVizPanel_Paint(object? sender, PaintEventArgs e)
        {
            if (_graph == null || _nodePos == null) return;
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            var pos = ScalePositions(_nodePos, graphPanel.Size, pathVizPanel.Size);

            DrawEdges(g, pos, _graph, null, _shortestPath);
            DrawNodes(g, pos, _graph, null, null, _shortestPath);
        }

        // Общие методы рисования

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

        // Масштабирование позиций под другой размер панели
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

        // Hover на графе
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
        // Статистика
        private void BtnCalcStats_Click(object? sender, EventArgs e)
        {
            if (_graph == null) { MessageBox.Show("Сначала сгенерируйте граф."); return; }

            _degreeDistribution = _graph.DegreeDistribution();
            int pop = _graph.MostPopularUser();
            int components = _graph.CountComponents();
            bool connected = components == 1;
            double avg = _graph.AverageDegree();
            int edges = _graph.EdgeCount();

            // Медиана степени
            var degrees = new int[_graph.UserCount];
            for (int i = 0; i < _graph.UserCount; i++) degrees[i] = _graph.Degree(i);
            SortArray(degrees);
            int median = degrees[degrees.Length / 2];

            // Максимальная и минимальная степень
            int minDeg = degrees[0], maxDeg = degrees[^1];

            var sb = new StringBuilder();
            sb.AppendLine("═══ ОБЩАЯ СТАТИСТИКА ═══");
            sb.AppendLine($"Пользователей:      {_graph.UserCount}");
            sb.AppendLine($"Связей (рёбер):     {edges}");
            sb.AppendLine($"Компонент связности:{components}");
            sb.AppendLine($"Граф связный:       {(connected ? "Да ✓" : "Нет ✗")}");
            sb.AppendLine();
            sb.AppendLine("═══ СТЕПЕНИ ВЕРШИН ═══");
            sb.AppendLine($"Среднее кол-во друзей: {avg:F3}");
            sb.AppendLine($"Медиана:               {median}");
            sb.AppendLine($"Минимум:               {minDeg}");
            sb.AppendLine($"Максимум:              {maxDeg}");
            sb.AppendLine();
            sb.AppendLine("═══ САМЫЙ ПОПУЛЯРНЫЙ ═══");
            sb.AppendLine($"{_graph.UserNames[pop]}  (ID {pop + 1})");
            sb.AppendLine($"Друзей: {_graph.Degree(pop)}");
            sb.AppendLine();
            sb.AppendLine("═══ ДИАМЕТР ═══");
            if (_diameter >= 0)
                sb.AppendLine($"Диаметр графа: {_diameter}");
            else
                sb.AppendLine("Нажмите «Вычислить диаметр»");

            txtStats.Text = sb.ToString();
            degreeChartPanel.Invalidate();
        }

        private async void BtnCalcDiameter_Click(object? sender, EventArgs e)
        {
            if (_graph == null) { MessageBox.Show("Сначала сгенерируйте граф."); return; }

            btnCalcDiameter.Enabled = false;
            btnCalcStats.Enabled = false;
            progressBar.Visible = true;
            progressBar.Maximum = _graph.UserCount;
            progressBar.Value = 0;
            lblProgress.Text = "Вычисление диаметра...";

            var progress = new Progress<int>(v =>
            {
                progressBar.Value = Math.Min(v, progressBar.Maximum);
                lblProgress.Text = $"BFS из вершины {v + 1} / {_graph.UserCount}";
            });

            int diam = await Task.Run(() => _graph.Diameter(progress));
            _diameter = diam;

            progressBar.Visible = false;
            lblProgress.Text = $"Диаметр вычислен: {_diameter}";
            btnCalcDiameter.Enabled = true;
            btnCalcStats.Enabled = true;

            // Обновить текст статистики
            BtnCalcStats_Click(null, EventArgs.Empty);
        }

        // График распределения степеней
        private void DegreeChartPanel_Paint(object? sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            if (_degreeDistribution == null || _graph == null)
            {
                using var hint = new SolidBrush(Color.FromArgb(150, 100, 100, 100));
                using var f = new Font("Segoe UI", 11f);
                g.DrawString("Нажмите «Вычислить статистику»", f, hint, 20, 20);
                return;
            }

            int W = degreeChartPanel.Width;
            int H = degreeChartPanel.Height;
            int pad = 50;

            // Группируем степени в бины для читаемости
            int maxDeg = _degreeDistribution.Length - 1;
            int binCount = Math.Min(40, maxDeg + 1);
            var bins = new int[binCount];
            var binLabels = new string[binCount];

            for (int d = 0; d <= maxDeg; d++)
            {
                int bin = (int)((long)d * binCount / (maxDeg + 1));
                if (bin >= binCount) bin = binCount - 1;
                bins[bin] += _degreeDistribution[d];
            }
            for (int b = 0; b < binCount; b++)
            {
                int lo = b * (maxDeg + 1) / binCount;
                int hi = (b + 1) * (maxDeg + 1) / binCount - 1;
                binLabels[b] = lo == hi ? $"{lo}" : $"{lo}-{hi}";
            }

            int maxVal = 1;
            for (int i = 0; i < binCount; i++) if (bins[i] > maxVal) maxVal = bins[i];

            float barW = (float)(W - pad * 2) / binCount;
            float chartH = H - pad * 2;

            // Оси
            using var axisPen = new Pen(Color.FromArgb(150, 150, 150), 1f);
            g.DrawLine(axisPen, pad, pad, pad, H - pad);
            g.DrawLine(axisPen, pad, H - pad, W - pad, H - pad);

            using var barBrush = new LinearGradientBrush(
                new Point(0, pad), new Point(0, H - pad),
                Color.FromArgb(100, 180, 255), Color.FromArgb(50, 80, 200));
            using var labelFont = new Font("Segoe UI", 7f);
            using var labelBrush = new SolidBrush(Color.FromArgb(80, 80, 80));
            using var valFont = new Font("Segoe UI", 7.5f, FontStyle.Bold);
            using var valBrush = new SolidBrush(Color.FromArgb(40, 40, 40));

            for (int i = 0; i < binCount; i++)
            {
                float bh = bins[i] == 0 ? 0 : (float)bins[i] / maxVal * chartH;
                float bx = pad + i * barW + 1;
                float by = H - pad - bh;

                g.FillRectangle(barBrush, bx, by, barW - 2, bh);

                // Подпись оси X (каждый 4-й бин)
                if (i % 4 == 0)
                    g.DrawString(binLabels[i], labelFont, labelBrush, bx, H - pad + 3);

                // Значение над столбцом
                if (bins[i] > 0 && bh > 14)
                    g.DrawString(bins[i].ToString(), valFont, valBrush, bx, by - 14);
            }

            // Подписи осей
            using var axisFont = new Font("Segoe UI", 9f, FontStyle.Bold);
            using var axisBrush = new SolidBrush(Color.FromArgb(40, 40, 40));
            g.DrawString("Степень (кол-во друзей)", axisFont, axisBrush, W / 2 - 80, H - 18);

            for (int tick = 0; tick <= 4; tick++)
            {
                int val = maxVal * tick / 4;
                float ty = H - pad - (float)val / maxVal * chartH;
                g.DrawLine(axisPen, pad - 4, ty, pad, ty);
                g.DrawString(val.ToString(), labelFont, labelBrush, 2, ty - 6);
            }
        }

        // BFS по уровням
        private void BtnBfs_Click(object? sender, EventArgs e)
        {
            if (_graph == null) { MessageBox.Show("Сначала сгенерируйте граф."); return; }

            int source = (int)nudBfsSource.Value - 1;
            int depth = (int)nudBfsDepth.Value;

            _bfsSource = source;
            _bfsLevels = _graph.BfsLevels(source, depth);

            int total = 0;
            foreach (var kv in _bfsLevels) total += kv.Value.Count;

            var sb = new StringBuilder();
            sb.AppendLine($"BFS от вершины «User_{source + 1}» ({total + 1} вершин)");
            sb.AppendLine(new string('─', 45));
            sb.AppendLine($"  Уровень 0: User_{source + 1}");

            for (int d = 1; d <= depth; d++)
            {
                if (_bfsLevels.TryGetValue(d, out var list) && list.Count > 0)
                {
                    sb.AppendLine($"  Уровень {d} ({list.Count} польз.):");
                    int show = Math.Min(list.Count, 10);
                    for (int i = 0; i < show; i++)
                        sb.AppendLine($"    User_{list[i] + 1}");
                    if (list.Count > show)
                        sb.AppendLine($"    ... ещё {list.Count - show}");
                    sb.AppendLine();
                }
            }

            txtBfsResult.Text = sb.ToString();
            bfsHighlightPanel.Invalidate();
        }
        // Кратчайший путь
        private void BtnFindPath_Click(object? sender, EventArgs e)
        {
            if (_graph == null) { MessageBox.Show("Сначала сгенерируйте граф."); return; }

            int from = (int)nudPathFrom.Value - 1;
            int to = (int)nudPathTo.Value - 1;

            _pathSource = from;
            _pathTarget = to;
            _shortestPath = _graph.ShortestPath(from, to);

            var sb = new StringBuilder();
            sb.AppendLine($"От: User_{from + 1}");
            sb.AppendLine($"До: User_{to + 1}");
            sb.AppendLine();

            if (_shortestPath.Count == 0)
            {
                sb.AppendLine("Путь не найден.");
                sb.AppendLine("Пользователи в разных компонентах.");
            }
            else
            {
                sb.AppendLine($"Длина: {_shortestPath.Count - 1} шагов");
                sb.AppendLine();
                for (int i = 0; i < _shortestPath.Count; i++)
                {
                    int node = _shortestPath[i];
                    sb.AppendLine($"  {i}. User_{node + 1}");
                }
            }

            txtPathResult.Text = sb.ToString();
            pathVizPanel.Invalidate();
        }

        // Вспомогательная сортировка (сортировка вставками для малых массивов,
        // быстрая сортировка для больших — без использования Array.Sort)
        private static void SortArray(int[] arr)
        {
            QuickSort(arr, 0, arr.Length - 1);
        }

        private static void QuickSort(int[] arr, int lo, int hi)
        {
            if (lo >= hi) return;
            int pivot = arr[(lo + hi) / 2];
            int i = lo, j = hi;
            while (i <= j)
            {
                while (arr[i] < pivot) i++;
                while (arr[j] > pivot) j--;
                if (i <= j) { (arr[i], arr[j]) = (arr[j], arr[i]); i++; j--; }
            }
            QuickSort(arr, lo, j);
            QuickSort(arr, i, hi);
        }
    }
}
