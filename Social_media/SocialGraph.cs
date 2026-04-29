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
    }
}
