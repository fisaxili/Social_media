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
        // ─────────────────────────────────────────────
        // 1. Степень вершины (количество друзей)
        // ─────────────────────────────────────────────
        public int Degree(int user) => adjacency[user].Count;

        // ─────────────────────────────────────────────
        // 2. Пользователь с максимальным числом друзей
        // ─────────────────────────────────────────────
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
        // ─────────────────────────────────────────────
        // 3. Среднее количество друзей
        // ─────────────────────────────────────────────
        public double AverageDegree()
        {
            long total = 0;
            for (int i = 0; i < UserCount; i++)
                total += adjacency[i].Count;
            return (double)total / UserCount;
        }

        // ─────────────────────────────────────────────
        // 8. Распределение степеней
        // ─────────────────────────────────────────────
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
        // ─────────────────────────────────────────────
        // 4. BFS — все пользователи на расстоянии <= k
        // ─────────────────────────────────────────────
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
        // ─────────────────────────────────────────────
        // 5. Кратчайший путь между двумя пользователями (BFS)
        // ─────────────────────────────────────────────
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
        // ─────────────────────────────────────────────
        // 6. Проверка связности графа
        // ─────────────────────────────────────────────
        public bool IsConnected() => CountComponents() == 1;

        // ─────────────────────────────────────────────
        // 7. Количество компонент связности
        // ─────────────────────────────────────────────
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

    }
}
