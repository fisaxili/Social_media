using System;
using System.Collections.Generic;

namespace Social_media
{
    /// <summary>
    /// Граф социальной сети на основе списка смежности.
    /// Все алгоритмы реализованы вручную без использования библиотечных аналогов.
    /// </summary>
    public class SocialGraph
    {
        // Список смежности: adjacency[i] — список друзей пользователя i
        private readonly List<int>[] adjacency;

        public int UserCount { get; }

        // Имена пользователей
        public string[] UserNames { get; }

        public SocialGraph(int userCount)
        {
            UserCount = userCount;
            adjacency = new List<int>[userCount];
            UserNames = new string[userCount];
            for (int i = 0; i < userCount; i++)
            {
                adjacency[i] = new List<int>();
                UserNames[i] = $"User_{i + 1}";
            }
        }

        /// <summary>Добавить ненаправленное ребро (дружбу) между u и v.</summary>
        public void AddEdge(int u, int v)
        {
            if (u == v) return;
            if (!adjacency[u].Contains(v))
            {
                adjacency[u].Add(v);
                adjacency[v].Add(u);
            }
        }

        /// <summary>Получить список друзей пользователя.</summary>
        public IReadOnlyList<int> GetFriends(int user) => adjacency[user];

        //Степень вершины (количество друзей)

        public int Degree(int user) => adjacency[user].Count;


        //Пользователь с максимальным числом друзей

        public int MostPopularUser()
        {
            int maxUser = 0;
            int maxDeg = adjacency[0].Count;
            for (int i = 1; i < UserCount; i++)
            {
                if (adjacency[i].Count > maxDeg)
                {
                    maxDeg = adjacency[i].Count;
                    maxUser = i;
                }
            }
            return maxUser;
        }

        // Среднее количество друзей
 
        public double AverageDegree()
        {
            long total = 0;
            for (int i = 0; i < UserCount; i++)
                total += adjacency[i].Count;
            return (double)total / UserCount;
        }

        // Распределение степеней
        /// <summary>Возвращает массив: degreeDistribution[d] = количество пользователей со степенью d.</summary>
        public int[] DegreeDistribution()
        {
            int maxDeg = 0;
            for (int i = 0; i < UserCount; i++)
                if (adjacency[i].Count > maxDeg) maxDeg = adjacency[i].Count;

            var dist = new int[maxDeg + 1];
            for (int i = 0; i < UserCount; i++)
                dist[adjacency[i].Count]++;
            return dist;
        }

        // BFS — все пользователи на расстоянии <= k
        /// <summary>
        /// Возвращает словарь: расстояние -> список пользователей на этом расстоянии от source.
        /// Обходит до глубины maxDepth включительно.
        /// </summary>
        public Dictionary<int, List<int>> BfsLevels(int source, int maxDepth)
        {
            var result = new Dictionary<int, List<int>>();
            var dist = new int[UserCount];
            for (int i = 0; i < UserCount; i++) dist[i] = -1;

            // Собственная очередь на массиве
            var queue = new int[UserCount];
            int head = 0, tail = 0;

            dist[source] = 0;
            queue[tail++] = source;

            while (head < tail)
            {
                int cur = queue[head++];
                int d = dist[cur];
                if (d >= maxDepth) continue;

                foreach (int nb in adjacency[cur])
                {
                    if (dist[nb] == -1)
                    {
                        dist[nb] = d + 1;
                        queue[tail++] = nb;

                        if (!result.ContainsKey(dist[nb]))
                            result[dist[nb]] = new List<int>();
                        result[dist[nb]].Add(nb);
                    }
                }
            }
            return result;
        }

        // Кратчайший путь между двумя пользователями (BFS)
        /// <summary>
        /// Возвращает список вершин кратчайшего пути от source до target,
        /// или пустой список если пути нет.
        /// </summary>
        public List<int> ShortestPath(int source, int target)
        {
            if (source == target) return new List<int> { source };

            var prev = new int[UserCount];
            var dist = new int[UserCount];
            for (int i = 0; i < UserCount; i++) { prev[i] = -1; dist[i] = -1; }

            var queue = new int[UserCount];
            int head = 0, tail = 0;

            dist[source] = 0;
            queue[tail++] = source;

            bool found = false;
            while (head < tail && !found)
            {
                int cur = queue[head++];
                foreach (int nb in adjacency[cur])
                {
                    if (dist[nb] == -1)
                    {
                        dist[nb] = dist[cur] + 1;
                        prev[nb] = cur;
                        if (nb == target) { found = true; break; }
                        queue[tail++] = nb;
                    }
                }
            }

            if (!found) return new List<int>();

            // Восстановление пути
            var path = new List<int>();
            int node = target;
            while (node != -1)
            {
                path.Add(node);
                node = prev[node];
            }
            path.Reverse();
            return path;
        }

        // Проверка связности графа

        public bool IsConnected() => CountComponents() == 1;


        // Количество компонент связности

        public int CountComponents()
        {
            var visited = new bool[UserCount];
            int count = 0;
            var queue = new int[UserCount];

            for (int start = 0; start < UserCount; start++)
            {
                if (visited[start]) continue;
                count++;
                // BFS для обхода компоненты
                int head = 0, tail = 0;
                visited[start] = true;
                queue[tail++] = start;
                while (head < tail)
                {
                    int cur = queue[head++];
                    foreach (int nb in adjacency[cur])
                    {
                        if (!visited[nb])
                        {
                            visited[nb] = true;
                            queue[tail++] = nb;
                        }
                    }
                }
            }
            return count;
        }

        /// <summary>Возвращает массив: componentId[i] — номер компоненты пользователя i.</summary>
        public int[] GetComponentIds()
        {
            var comp = new int[UserCount];
            for (int i = 0; i < UserCount; i++) comp[i] = -1;

            int compId = 0;
            var queue = new int[UserCount];

            for (int start = 0; start < UserCount; start++)
            {
                if (comp[start] != -1) continue;
                int head = 0, tail = 0;
                comp[start] = compId;
                queue[tail++] = start;
                while (head < tail)
                {
                    int cur = queue[head++];
                    foreach (int nb in adjacency[cur])
                    {
                        if (comp[nb] == -1)
                        {
                            comp[nb] = compId;
                            queue[tail++] = nb;
                        }
                    }
                }
                compId++;
            }
            return comp;
        }

        // Диаметр графа (максимальное расстояние между любыми двумя вершинами)
        // Для больших графов используем эвристику: BFS из нескольких случайных вершин
        /// <summary>
        /// Вычисляет диаметр графа.
        /// Для связного графа — точный результат через BFS из каждой вершины (может быть медленно для 500 узлов).
        /// Для несвязного — диаметр наибольшей компоненты.
        /// </summary>
        public int Diameter(IProgress<int>? progress = null)
        {
            int diameter = 0;
            var dist = new int[UserCount];
            var queue = new int[UserCount];

            for (int source = 0; source < UserCount; source++)
            {
                progress?.Report(source);

                // BFS из source
                for (int i = 0; i < UserCount; i++) dist[i] = -1;
                int head = 0, tail = 0;
                dist[source] = 0;
                queue[tail++] = source;

                while (head < tail)
                {
                    int cur = queue[head++];
                    foreach (int nb in adjacency[cur])
                    {
                        if (dist[nb] == -1)
                        {
                            dist[nb] = dist[cur] + 1;
                            if (dist[nb] > diameter) diameter = dist[nb];
                            queue[tail++] = nb;
                        }
                    }
                }
            }
            return diameter;
        }

        //  BFS-расстояния от одной вершины (для визуализации и статистики)
        public int[] BfsDistances(int source)
        {
            var dist = new int[UserCount];
            for (int i = 0; i < UserCount; i++) dist[i] = -1;
            var queue = new int[UserCount];
            int head = 0, tail = 0;
            dist[source] = 0;
            queue[tail++] = source;
            while (head < tail)
            {
                int cur = queue[head++];
                foreach (int nb in adjacency[cur])
                {
                    if (dist[nb] == -1)
                    {
                        dist[nb] = dist[cur] + 1;
                        queue[tail++] = nb;
                    }
                }
            }
            return dist;
        }

        // Генерация случайного графа (500 узлов, ~2000 рёбер)
        public static SocialGraph GenerateRandom(int users = 500, int edges = 2000, int seed = 42)
        {
            var graph = new SocialGraph(users);
            var rng = new Random(seed);

            // Сначала создаём связный остов через случайное дерево
            var perm = new int[users];
            for (int i = 0; i < users; i++) perm[i] = i;
            // Перемешать (Fisher-Yates)
            for (int i = users - 1; i > 0; i--)
            {
                int j = rng.Next(i + 1);
                (perm[i], perm[j]) = (perm[j], perm[i]);
            }
            for (int i = 1; i < users; i++)
                graph.AddEdge(perm[i], perm[rng.Next(i)]);

            // Добавляем случайные рёбра до нужного количества
            int attempts = 0;
            while (CountEdges(graph) < edges && attempts < edges * 20)
            {
                int u = rng.Next(users);
                int v = rng.Next(users);
                graph.AddEdge(u, v);
                attempts++;
            }

            return graph;
        }

        private static int CountEdges(SocialGraph g)
        {
            int total = 0;
            for (int i = 0; i < g.UserCount; i++)
                total += g.adjacency[i].Count;
            return total / 2;
        }

        public int EdgeCount() => CountEdges(this);
    }
}
