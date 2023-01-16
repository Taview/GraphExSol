using CsvHelper;
using CsvHelper.Configuration;
using GraphEx;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using WorldCitiesNet;
using WorldCitiesNet.Models;

namespace Graphex.Test
{
    public class GraphTests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void TestIfValueAssignedInCtor()
        {
            var gr = new Graph<string, Station, Route>();
            var stationA = gr.AddNode("A");
            var strationB = gr.AddNode("B");
            var strationC = gr.AddNode("C");

            gr.AddEdge("A", "C");
            gr.AddEdge("B", "C");
            gr.AddEdge("C", "C");
        }

        [Test]
        public void AddNodeShouldAddOrOverwriteNewNodes()
        {
            var gr = new Graph<string, Station, Route>();
            var stationA = gr.AddNode("A");
            var strationB = gr.AddNode("B");
            var strationC = gr.AddNode("C");

            //This node will be override node with the same key
            var strationA2 = gr.AddNode("A");

            Assert.AreEqual(3, gr.Nodes.Count);
        }

        [Test]
        public void AddEdgesShouldAddOrOverwriteEdgeNodes()
        {
            var gr = new Graph<string, Station, Route>();
            var stationA = gr.AddNode("A");
            stationA.Content = new Station();
            stationA.Content.Lat = 789;
            var stationB = gr.AddNode("B");
            stationB.Content = new Station();
            stationB.Content.Lon = 821;
            var stationC = gr.AddNode("C");

            gr.AddEdge("A", "C");
            gr.AddEdge("B", "C");
            gr.AddEdge("C", "B");
            gr.AddEdge("C", "C");
            gr.AddEdge("C", "B");



            Assert.AreEqual(1, gr.GetNode("A").Edges.Count);
            Assert.AreEqual(1, gr.GetNode("B").Edges.Count);
            Assert.AreEqual(2, gr.GetNode("C").Edges.Count);
        }


        /// <summary>
        /// Test dijkstras alorightm test
        /// Actual, Expected and Graph configuration can be foudn here
        /// https://www.freecodecamp.org/news/dijkstras-shortest-path-algorithm-visual-introduction/#:~:text=Dijkstra's%20Algorithm%20finds%20the%20shortest,node%20and%20all%20other%20nodes.
        /// 
        /// </summary>
        [Test]
        public void TestDejikstra()
        {
            var gr = new Graph<int, int, int>(initializeSelfNode: true);
            gr.AddNode(0);
            gr.AddNode(1);
            gr.AddNode(2);
            gr.AddNode(3);
            gr.AddNode(4);
            gr.AddNode(5);
            gr.AddNode(6);

            AddDejikstraEdge(gr, 0, 1, 2);
            AddDejikstraEdge(gr, 1, 3, 5);
            AddDejikstraEdge(gr, 3, 5, 15);
            AddDejikstraEdge(gr, 5, 6, 6);
            AddDejikstraEdge(gr, 6, 4, 2);
            AddDejikstraEdge(gr, 4, 3, 10);
            AddDejikstraEdge(gr, 3, 2, 8);
            AddDejikstraEdge(gr, 2, 0, 6);

            AddDejikstraEdge(gr, 5, 4, 6);

            int nodeStart = 0;
            int nodeEnd = 6;
            Console.WriteLine($"Looking route from {nodeStart} to {nodeEnd}");

            gr.BuildGraph();

            int startNodeIndex = gr.GetNodeIndex(nodeStart);
            int endNodeIndex = gr.GetNodeIndex(nodeEnd);

            int[] shortestPathIndexes;
            var shortestDistanceIndexes = Algorithms.FindShortestPathDejikstraFromNode(startNodeIndex, gr, edge => edge.Distance, out shortestPathIndexes);
            var resPath = Algorithms.GetShortestPath(shortestPathIndexes, startNodeIndex, endNodeIndex);

            int index = 0;
            foreach (var shortestDistanceIndex in shortestDistanceIndexes)
            {
                Console.WriteLine($"[{index}] Length from node = {shortestDistanceIndex}");
                index++;
            }

            if(resPath != null)
            {
                foreach (var resIndex in resPath)
                {
                    Console.WriteLine($"Path {resIndex}");
                }

            }
            else
            {
                Console.WriteLine($"Path is impossible");
            }

            Assert.AreEqual(3, gr.GetNode(0).Edges.Count); //1 - self + 2
            Assert.AreEqual(3, gr.GetNode(1).Edges.Count); //1 - self + 2
            Assert.AreEqual(3, gr.GetNode(2).Edges.Count); //1 - self + 2
            Assert.AreEqual(5, gr.GetNode(3).Edges.Count); //1 - self + 4
            Assert.AreEqual(4, gr.GetNode(4).Edges.Count); //1 - self + 3
            Assert.AreEqual(4, gr.GetNode(5).Edges.Count); //1 - self + 3
            Assert.AreEqual(3, gr.GetNode(6).Edges.Count); //1 - self + 2

            Assert.AreEqual(0, resPath[0]); //1 - self + 2
            Assert.AreEqual(1, resPath[1]); //1 - self + 2
            Assert.AreEqual(3, resPath[2]); //1 - self + 2
            Assert.AreEqual(4, resPath[3]); //1 - self + 2
            Assert.AreEqual(6, resPath[4]); //1 - self + 2

        }

        /// <summary>
        /// Test dijkstras alorightm test
        /// Actual, Expected and Graph configuration can be foudn here
        /// https://www.freecodecamp.org/news/dijkstras-shortest-path-algorithm-visual-introduction/#:~:text=Dijkstra's%20Algorithm%20finds%20the%20shortest,node%20and%20all%20other%20nodes.
        /// 
        /// </summary>
        [Test]
        public void TestAStarFast()
        {
            var gr = new Graph<int, int, int>(initializeSelfNode: true);
            gr.AddNode(0);
            gr.AddNode(1);
            gr.AddNode(2);
            gr.AddNode(3);
            gr.AddNode(4);
            gr.AddNode(5);
            gr.AddNode(6);

            AddDejikstraEdge(gr, 0, 1, 2);
            AddDejikstraEdge(gr, 1, 3, 5);
            AddDejikstraEdge(gr, 3, 5, 15);
            AddDejikstraEdge(gr, 5, 6, 6);
            AddDejikstraEdge(gr, 6, 4, 2);
            AddDejikstraEdge(gr, 4, 3, 10);
            AddDejikstraEdge(gr, 3, 2, 8);
            AddDejikstraEdge(gr, 2, 0, 6);

            AddDejikstraEdge(gr, 5, 4, 6);

            int nodeStart = 0;
            int nodeEnd = 6;
            Console.WriteLine($"Looking route from {nodeStart} to {nodeEnd}");

            gr.BuildGraph();

            int startNodeIndex = gr.GetNodeIndex(nodeStart);
            int endNodeIndex = gr.GetNodeIndex(nodeEnd);

            int[] shortestPathIndexes;
            var shortestDistanceIndexes = Algorithms.FindShortestPathAStarFromNode(
                startNodeIndex, 
                endNodeIndex, 
                gr, 
                edge => edge.Distance, 
                edge => edge.Distance, 
                out shortestPathIndexes);
            var resPath = Algorithms.GetShortestPath(shortestPathIndexes, startNodeIndex, endNodeIndex);

            int index = 0;
            foreach (var shortestDistanceIndex in shortestDistanceIndexes)
            {
                Console.WriteLine($"[{index}] Length from node = {shortestDistanceIndex}");
                index++;
            }

            if (resPath != null)
            {
                foreach (var resIndex in resPath)
                {
                    Console.WriteLine($"Path {resIndex}");
                }

            }
            else
            {
                Console.WriteLine($"Path is impossible");
            }

            Assert.AreEqual(3, gr.GetNode(0).Edges.Count); //1 - self + 2
            Assert.AreEqual(3, gr.GetNode(1).Edges.Count); //1 - self + 2
            Assert.AreEqual(3, gr.GetNode(2).Edges.Count); //1 - self + 2
            Assert.AreEqual(5, gr.GetNode(3).Edges.Count); //1 - self + 4
            Assert.AreEqual(4, gr.GetNode(4).Edges.Count); //1 - self + 3
            Assert.AreEqual(4, gr.GetNode(5).Edges.Count); //1 - self + 3
            Assert.AreEqual(3, gr.GetNode(6).Edges.Count); //1 - self + 2

            Assert.AreEqual(0, resPath[0]); //1 - self + 2
            Assert.AreEqual(1, resPath[1]); //1 - self + 2
            Assert.AreEqual(3, resPath[2]); //1 - self + 2
            Assert.AreEqual(4, resPath[3]); //1 - self + 2
            Assert.AreEqual(6, resPath[4]); //1 - self + 2

        }

        private void AddDejikstraEdge(Graph<int, int, int> gr, int from, int to, double dist)
        {
            var edge = gr.AddEdge(from, to);
            edge.Distance = dist;
            
            //Simulate bi directional tree
            var edge2 = gr.AddEdge(to, from);
            edge2.Distance = dist;
        }

        [Test]
        public void CreateTestStations()
        {
            var gr = new Graph<string, Station, Route>();
            var stationABDU = gr.AddNode("ABDU");
            stationABDU.Content = new Station();
            stationABDU.Content.Desc = "Abel Durand";
            stationABDU.Content.Lat = 47.22019661;
            stationABDU.Content.Lon = -1.60337553;

            var stationABLA = gr.AddNode("ABLA");
            stationABLA.Content = new Station();
            stationABLA.Content.Desc = "Avenue Blanche";
            stationABLA.Content.Lat = 47.22973509;
            stationABLA.Content.Lon = -1.58937990;

            var stationACHA = gr.AddNode("ACHA");
            stationACHA.Content = new Station();
            stationACHA.Content.Desc = "Angle Chaillou";
            stationACHA.Content.Lat = 47.26979248;
            stationACHA.Content.Lon = -1.57206627;

            gr.AddEdge("ABDU", "ABLA");
            gr.AddEdge("ABLA", "ACHA");

            gr.BuildGraph();

            int startNodeIndex = gr.GetNodeIndex("ABDU");
            int endNodeIndex = gr.GetNodeIndex("ABLA");

            int[] shortestIndexPaths;

            var nodeList = gr.Nodes.ToList();
            var result = Algorithms.FindShortestPathDejikstraFromNode(startNodeIndex, gr, Helper.CalcDistStation, out shortestIndexPaths);
            var pathStations = Algorithms.GetShortestPath(shortestIndexPaths, startNodeIndex, endNodeIndex);

            foreach(var stationIndex in pathStations)
            {
                Console.WriteLine(nodeList[stationIndex].Content.Desc);
            }

            Assert.AreEqual(1, gr.GetNode("ABDU").Edges.Count);
            Assert.AreEqual(1, gr.GetNode("ABLA").Edges.Count);
            Assert.AreEqual(0, gr.GetNode("ACHA").Edges.Count);
        }

        [Test]
        public void ShouldCreateAndDeleteNodesCorrectly()
        {
            var gr = new Graph<string, Station, Route>();
            gr.AddNode("Node0");
            gr.AddNode("Node1");
            gr.AddNode("Node2");

            //Positive test
            gr.RemoveNode("Node1");
            //negative test - node doesn't exist
            Assert.Throws<ArgumentOutOfRangeException>(() => gr.RemoveNode("Node3"),"This node doesn't exists and hence exception should be raised here");
            Assert.AreEqual(1, gr.NodeIndexes["Node2"]);
            Assert.AreEqual(2, gr.Nodes.Count);
        }

        [Test]
        public void ShouldCreateAndDeleteEdgesCorrectly()
        {
            var gr = new Graph<string, Station, Route>(initializeSelfNode: true);
            gr.AddNode("Node1");
            gr.AddNode("Node2");
            gr.AddNode("Node3");

            gr.AddEdge("Node1", "Node2"); 
            gr.AddEdge("Node2", "Node1");

            gr.AddEdge("Node2", "Node3");
            gr.RemoveEdge("Node2", "Node3");

            Assert.Throws<ArgumentOutOfRangeException>(() => gr.RemoveEdge("Node4", "Node3"), "This node doesn't exists so edge can't be removed");
            Assert.Throws<ArgumentOutOfRangeException>(() => gr.RemoveEdge("Node1", "Node3"), "This edge doesn't exists so it can't be removed");

            Assert.AreEqual(2, gr.GetNode("Node1").Edges.Count);
            Assert.AreEqual(2, gr.GetNode("Node2").Edges.Count);
            Assert.AreEqual(1, gr.GetNode("Node3").Edges.Count);
        }

        [Test]
        public void GetPathShouldCorrectlyForOneHop()
        {
            int[] shortIndexes = new int[] { -1, 0, 1 };
            var pathIndexes = Algorithms.GetShortestPath(shortIndexes, 0, 1);

            Assert.NotNull(pathIndexes);
            Assert.AreEqual(2, pathIndexes.Count);
            Assert.AreEqual(0, pathIndexes[0]);
            Assert.AreEqual(1, pathIndexes[1]);
        }

        [Test]
        [TestCase(1000, 1000, 0, 2, 999, 999)]
        public void Test2DGraphDejikstra(int width, int height, int x1, int y1, int x2, int y2)
        {
            var gr = new Graph<Point, Node<Point, int, int>, int>();

            Point start, end;
            BuildTest2Graph(width, height, x1, y1, x2, y2, gr, out start, out end);

            int startNodeIndex = gr.GetNodeIndex(start);
            int endNodeIndex = gr.GetNodeIndex(end);

            Console.WriteLine($"Dejikstra Starts here");
            var dejikstraWatchTime = new System.Diagnostics.Stopwatch();
            dejikstraWatchTime.Start();

            int[] shortestPathIndexes;
            
            var shortestDistanceIndexes = Algorithms.FindShortestPathDejikstraFromNode(
                startNodeIndex, 
                gr, 
                edge => Helper.CalcDistPoint(edge, Heuristics.ManhattanDistance), 
                out shortestPathIndexes);

            dejikstraWatchTime.Stop();
            Console.WriteLine($"Dejikstra Execution Time: {dejikstraWatchTime.Elapsed.TotalSeconds} secs");

            var resPath = Algorithms.GetShortestPath(shortestPathIndexes, startNodeIndex, endNodeIndex);

            var nodeList = gr.Nodes;

            //int index = 0;
            if (resPath != null)
            {
                foreach (var resIndex in resPath)
                {
                    //Console.WriteLine($"Path {nodeList[resIndex]}");
                }

            }
            else
            {
                Console.WriteLine($"Path is impossible");
            }
        }

        [Test]
        [TestCase(1000, 1000, 0, 2, 999, 999)]
        public void Test2DGraphAStar(int width, int height, int x1, int y1, int x2, int y2)
        {
            var gr = new Graph<Point, Node<Point, int, int>, int>();

            Point start, end;
            BuildTest2Graph(width, height, x1, y1, x2, y2, gr, out start, out end);

            Console.WriteLine($"Start point {start} and end {end}");
            int startNodeIndex = gr.GetNodeIndex(start);
            int endNodeIndex = gr.GetNodeIndex(end);

            Console.WriteLine($"A* Starts here");
            var astartWatchTimer = new System.Diagnostics.Stopwatch();
            astartWatchTimer.Start();

            int[] shortestPathIndexes;

            var shortestDistanceIndexes = Algorithms.FindShortestPathAStarFromNode(
                startNodeIndex,
                endNodeIndex,
                gr,
                edge => Helper.CalcDistPoint(edge, Heuristics.ManhattanDistance),
                edge => Helper.CalcDistPoint(edge, Heuristics.ManhattanDistance),
                out shortestPathIndexes);

            astartWatchTimer.Stop();
            Console.WriteLine($"A* Execution Time: {astartWatchTimer.Elapsed.TotalSeconds} secs");

            var resPath = Algorithms.GetShortestPath(shortestPathIndexes, startNodeIndex, endNodeIndex);

            var nodeList = gr.Nodes;

            //int index = 0;
            if (resPath != null)
            {
                foreach (var resIndex in resPath)
                {
                    //Console.WriteLine($"Path {nodeList[resIndex]}");
                }

            }
            else
            {
                Console.WriteLine($"Path is impossible");
            }
        }

        private static void BuildTest2Graph(int width, int height, int x1, int y1, int x2, int y2, Graph<Point, Node<Point, int, int>, int> gr, out Point start, out Point end)
        {
            start = new Point(x1, y1);
            end = new Point(x2, y2);
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    gr.AddNode(new Point(x, y));
                    if (x > 0)
                    {
                        //Connect all nodes on one row. Prev with current
                        var from = new Point(x - 1, y);
                        var to = new Point(x, y);
                        gr.AddEdge(from, to);
                        gr.AddEdge(to, from);
                    }

                    //Connect all nodes in one column. Prev with current
                    if (y > 0)
                    {
                        var from = new Point(x, y - 1);
                        var to = new Point(x, y);
                        gr.AddEdge(from, to);
                        gr.AddEdge(to, from);
                    }
                }
            }

            gr.BuildGraph();
        }
    }
}