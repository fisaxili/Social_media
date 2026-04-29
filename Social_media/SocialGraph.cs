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
    }
}
