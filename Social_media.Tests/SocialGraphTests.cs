using Social_media;

namespace Social_media.Tests
{
    [TestFixture]
    public class SocialGraphConstructorTests
    {
        [Test]
        public void Constructor_SetsUserCount()
        {
            var graph = new SocialGraph(5);
            Assert.That(graph.UserCount, Is.EqualTo(5));
        }

        [Test]
        public void Constructor_InitializesUserNames()
        {
            var graph = new SocialGraph(3);
            Assert.That(graph.UserNames[0], Is.EqualTo("User_1"));
            Assert.That(graph.UserNames[1], Is.EqualTo("User_2"));
            Assert.That(graph.UserNames[2], Is.EqualTo("User_3"));
        }

        [Test]
        public void Constructor_NoEdgesInitially()
        {
            var graph = new SocialGraph(4);
            Assert.That(graph.EdgeCount(), Is.EqualTo(0));
        }
    }

    [TestFixture]
    public class AddEdgeTests
    {
        private SocialGraph _graph = null!;

        [SetUp]
        public void SetUp() => _graph = new SocialGraph(5);

        [Test]
        public void AddEdge_CreatesBidirectionalFriendship()
        {
            _graph.AddEdge(0, 1);
            Assert.That(_graph.GetFriends(0), Contains.Item(1));
            Assert.That(_graph.GetFriends(1), Contains.Item(0));
        }

        [Test]
        public void AddEdge_SelfLoop_IsIgnored()
        {
            _graph.AddEdge(2, 2);
            Assert.That(_graph.Degree(2), Is.EqualTo(0));
        }

        [Test]
        public void AddEdge_Duplicate_IsIgnored()
        {
            _graph.AddEdge(0, 1);
            _graph.AddEdge(0, 1);
            Assert.That(_graph.Degree(0), Is.EqualTo(1));
            Assert.That(_graph.EdgeCount(), Is.EqualTo(1));
        }

        [Test]
        public void AddEdge_IncreasesEdgeCount()
        {
            _graph.AddEdge(0, 1);
            _graph.AddEdge(1, 2);
            Assert.That(_graph.EdgeCount(), Is.EqualTo(2));
        }
    }

    [TestFixture]
    public class DegreeTests
    {
        [Test]
        public void Degree_IsolatedNode_ReturnsZero()
        {
            var graph = new SocialGraph(3);
            Assert.That(graph.Degree(0), Is.EqualTo(0));
        }

        [Test]
        public void Degree_ReturnsCorrectCount()
        {
            var graph = new SocialGraph(4);
            graph.AddEdge(0, 1);
            graph.AddEdge(0, 2);
            graph.AddEdge(0, 3);
            Assert.That(graph.Degree(0), Is.EqualTo(3));
        }
    }

    [TestFixture]
    public class MostPopularUserTests
    {
        [Test]
        public void MostPopularUser_ReturnsCentralNode()
        {
            // Звезда: 0 соединён со всеми
            var graph = new SocialGraph(5);
            graph.AddEdge(0, 1);
            graph.AddEdge(0, 2);
            graph.AddEdge(0, 3);
            graph.AddEdge(0, 4);
            Assert.That(graph.MostPopularUser(), Is.EqualTo(0));
        }

        [Test]
        public void MostPopularUser_SingleNode_ReturnsZero()
        {
            var graph = new SocialGraph(1);
            Assert.That(graph.MostPopularUser(), Is.EqualTo(0));
        }
    }

    [TestFixture]
    public class AverageDegreeTests
    {
        [Test]
        public void AverageDegree_NoEdges_ReturnsZero()
        {
            var graph = new SocialGraph(4);
            Assert.That(graph.AverageDegree(), Is.EqualTo(0.0));
        }

        [Test]
        public void AverageDegree_PathGraph_IsCorrect()
        {
            // 0-1-2-3: каждый узел имеет степень 1 или 2, сумма = 6, среднее = 6/4 = 1.5
            var graph = new SocialGraph(4);
            graph.AddEdge(0, 1);
            graph.AddEdge(1, 2);
            graph.AddEdge(2, 3);
            Assert.That(graph.AverageDegree(), Is.EqualTo(1.5));
        }

        [Test]
        public void AverageDegree_CompleteGraph_IsCorrect()
        {
            // K3: каждый узел имеет степень 2, среднее = 2.0
            var graph = new SocialGraph(3);
            graph.AddEdge(0, 1);
            graph.AddEdge(1, 2);
            graph.AddEdge(0, 2);
            Assert.That(graph.AverageDegree(), Is.EqualTo(2.0));
        }
    }

    [TestFixture]
    public class DegreeDistributionTests
    {
        [Test]
        public void DegreeDistribution_NoEdges_AllIsolated()
        {
            var graph = new SocialGraph(3);
            var dist = graph.DegreeDistribution();
            Assert.That(dist[0], Is.EqualTo(3));
        }

        [Test]
        public void DegreeDistribution_StarGraph_IsCorrect()
        {
            // Звезда: центр (0) имеет степень 3, остальные — степень 1
            var graph = new SocialGraph(4);
            graph.AddEdge(0, 1);
            graph.AddEdge(0, 2);
            graph.AddEdge(0, 3);
            var dist = graph.DegreeDistribution();
            Assert.That(dist[1], Is.EqualTo(3)); // три листа
            Assert.That(dist[3], Is.EqualTo(1)); // один центр
        }
    }

    [TestFixture]
    public class ShortestPathTests
    {
        private SocialGraph _graph = null!;

        [SetUp]
        public void SetUp()
        {
            // 0 - 1 - 2 - 3
            //         |
            //         4
            _graph = new SocialGraph(5);
            _graph.AddEdge(0, 1);
            _graph.AddEdge(1, 2);
            _graph.AddEdge(2, 3);
            _graph.AddEdge(2, 4);
        }

        [Test]
        public void ShortestPath_SameNode_ReturnsSingleElement()
        {
            var path = _graph.ShortestPath(2, 2);
            Assert.That(path, Is.EqualTo(new List<int> { 2 }));
        }

        [Test]
        public void ShortestPath_DirectNeighbors_ReturnsLengthTwo()
        {
            var path = _graph.ShortestPath(0, 1);
            Assert.That(path.Count, Is.EqualTo(2));
            Assert.That(path[0], Is.EqualTo(0));
            Assert.That(path[^1], Is.EqualTo(1));
        }

        [Test]
        public void ShortestPath_LongerPath_IsCorrect()
        {
            var path = _graph.ShortestPath(0, 3);
            Assert.That(path, Is.EqualTo(new List<int> { 0, 1, 2, 3 }));
        }

        [Test]
        public void ShortestPath_NoPath_ReturnsEmpty()
        {
            var disconnected = new SocialGraph(4);
            disconnected.AddEdge(0, 1);
            disconnected.AddEdge(2, 3);
            var path = disconnected.ShortestPath(0, 3);
            Assert.That(path, Is.Empty);
        }
    }

    [TestFixture]
    public class BfsLevelsTests
    {
        [Test]
        public void BfsLevels_ReturnsCorrectLevels()
        {
            // 0 - 1 - 2 - 3
            var graph = new SocialGraph(4);
            graph.AddEdge(0, 1);
            graph.AddEdge(1, 2);
            graph.AddEdge(2, 3);

            var levels = graph.BfsLevels(0, 3);
            Assert.That(levels[1], Contains.Item(1));
            Assert.That(levels[2], Contains.Item(2));
            Assert.That(levels[3], Contains.Item(3));
        }

        [Test]
        public void BfsLevels_MaxDepthLimitsResult()
        {
            var graph = new SocialGraph(4);
            graph.AddEdge(0, 1);
            graph.AddEdge(1, 2);
            graph.AddEdge(2, 3);

            var levels = graph.BfsLevels(0, 1);
            Assert.That(levels.ContainsKey(2), Is.False);
            Assert.That(levels.ContainsKey(3), Is.False);
        }

        [Test]
        public void BfsLevels_IsolatedSource_ReturnsEmpty()
        {
            var graph = new SocialGraph(3);
            var levels = graph.BfsLevels(0, 2);
            Assert.That(levels, Is.Empty);
        }
    }

    [TestFixture]
    public class ConnectivityTests
    {
        [Test]
        public void IsConnected_ConnectedGraph_ReturnsTrue()
        {
            var graph = new SocialGraph(3);
            graph.AddEdge(0, 1);
            graph.AddEdge(1, 2);
            Assert.That(graph.IsConnected(), Is.True);
        }

        [Test]
        public void IsConnected_DisconnectedGraph_ReturnsFalse()
        {
            var graph = new SocialGraph(4);
            graph.AddEdge(0, 1);
            graph.AddEdge(2, 3);
            Assert.That(graph.IsConnected(), Is.False);
        }

        [Test]
        public void IsConnected_SingleNode_ReturnsTrue()
        {
            var graph = new SocialGraph(1);
            Assert.That(graph.IsConnected(), Is.True);
        }

        [Test]
        public void CountComponents_TwoComponents_ReturnsTwo()
        {
            var graph = new SocialGraph(4);
            graph.AddEdge(0, 1);
            graph.AddEdge(2, 3);
            Assert.That(graph.CountComponents(), Is.EqualTo(2));
        }

        [Test]
        public void CountComponents_AllIsolated_ReturnsUserCount()
        {
            var graph = new SocialGraph(5);
            Assert.That(graph.CountComponents(), Is.EqualTo(5));
        }

        [Test]
        public void GetComponentIds_SameComponent_SameId()
        {
            var graph = new SocialGraph(4);
            graph.AddEdge(0, 1);
            graph.AddEdge(1, 2);
            graph.AddEdge(2, 3);
            var ids = graph.GetComponentIds();
            Assert.That(ids[0], Is.EqualTo(ids[1]));
            Assert.That(ids[1], Is.EqualTo(ids[2]));
            Assert.That(ids[2], Is.EqualTo(ids[3]));
        }

        [Test]
        public void GetComponentIds_DifferentComponents_DifferentIds()
        {
            var graph = new SocialGraph(4);
            graph.AddEdge(0, 1);
            graph.AddEdge(2, 3);
            var ids = graph.GetComponentIds();
            Assert.That(ids[0], Is.Not.EqualTo(ids[2]));
        }
    }

    [TestFixture]
    public class DiameterTests
    {
        [Test]
        public void Diameter_PathGraph_IsCorrect()
        {
            // 0 - 1 - 2 - 3 — диаметр = 3
            var graph = new SocialGraph(4);
            graph.AddEdge(0, 1);
            graph.AddEdge(1, 2);
            graph.AddEdge(2, 3);
            Assert.That(graph.Diameter(), Is.EqualTo(3));
        }

        [Test]
        public void Diameter_StarGraph_IsTwo()
        {
            // Звезда с центром 0 — диаметр = 2
            var graph = new SocialGraph(5);
            graph.AddEdge(0, 1);
            graph.AddEdge(0, 2);
            graph.AddEdge(0, 3);
            graph.AddEdge(0, 4);
            Assert.That(graph.Diameter(), Is.EqualTo(2));
        }

        [Test]
        public void Diameter_SingleNode_IsZero()
        {
            var graph = new SocialGraph(1);
            Assert.That(graph.Diameter(), Is.EqualTo(0));
        }
    }

    [TestFixture]
    public class BfsDistancesTests
    {
        [Test]
        public void BfsDistances_SourceIsZero()
        {
            var graph = new SocialGraph(3);
            graph.AddEdge(0, 1);
            graph.AddEdge(1, 2);
            var dist = graph.BfsDistances(0);
            Assert.That(dist[0], Is.EqualTo(0));
        }

        [Test]
        public void BfsDistances_ReachableNodes_CorrectDistances()
        {
            var graph = new SocialGraph(4);
            graph.AddEdge(0, 1);
            graph.AddEdge(1, 2);
            graph.AddEdge(2, 3);
            var dist = graph.BfsDistances(0);
            Assert.That(dist[1], Is.EqualTo(1));
            Assert.That(dist[2], Is.EqualTo(2));
            Assert.That(dist[3], Is.EqualTo(3));
        }

        [Test]
        public void BfsDistances_UnreachableNode_IsMinusOne()
        {
            var graph = new SocialGraph(3);
            graph.AddEdge(0, 1);
            // узел 2 изолирован
            var dist = graph.BfsDistances(0);
            Assert.That(dist[2], Is.EqualTo(-1));
        }
    }

    [TestFixture]
    public class GenerateRandomTests
    {
        [Test]
        public void GenerateRandom_HasCorrectUserCount()
        {
            var graph = SocialGraph.GenerateRandom(100, 200, seed: 1);
            Assert.That(graph.UserCount, Is.EqualTo(100));
        }

        [Test]
        public void GenerateRandom_IsConnected()
        {
            // Генератор строит связный остов, граф должен быть связным
            var graph = SocialGraph.GenerateRandom(50, 100, seed: 42);
            Assert.That(graph.IsConnected(), Is.True);
        }

        [Test]
        public void GenerateRandom_SameSeed_ProducesSameEdgeCount()
        {
            var g1 = SocialGraph.GenerateRandom(100, 300, seed: 7);
            var g2 = SocialGraph.GenerateRandom(100, 300, seed: 7);
            Assert.That(g1.EdgeCount(), Is.EqualTo(g2.EdgeCount()));
        }
    }
}
