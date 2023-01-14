using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Xml.Schema;
using static System.Collections.Specialized.BitVector32;

namespace GraphEx
{
    public class Node2D : Graph<Point> 
    {
        public override string ToString()
        {
            return $"Node2D([{Id.X},{Id.Y}])";
        }
    }

    public class Edge2D : Edge<Graph<Point>> 
    {
        public override string ToString()
        {
            return $"Edge2D([{From.Id.X},{From.Id.Y}] - [{To.Id.X},{To.Id.Y}])";
        }

        public static double CalcDist(Edge2D edge, Func<double, double, double, double, double> distFunc)
        {
            var start = (Graph<Point>)edge.From;
            var stop = (Graph<Point>)edge.To;
            return distFunc(start.Id.X, start.Id.Y, stop.Id.X, stop.Id.Y);
        }
    }

    public class Graph<TNodeKey>
    where TNodeKey : IEquatable<TNodeKey>
    {
        public TNodeKey Id { get; set; }

        public string Name { get; set; }

        public Dictionary<TNodeKey, Edge<Graph<TNodeKey>>> Edges { get; set; }

        public Graph()
        {
            Edges = new Dictionary<TNodeKey, Edge<Graph<TNodeKey>>>();
        }
    }

    public class Edge<TNode>
    {
        public TNode From { get; set; }
        public TNode To { get; set; }
        public double Distance { get; set; }
        public Edge() { }
    }

    public class Graph<TNodeKey, TNode, TEdge>
    where TNodeKey : IEquatable<TNodeKey>
    where TNode : Graph<TNodeKey>, new()
    where TEdge : Edge<Graph<TNodeKey>>, new()
    {
        public Dictionary<TNodeKey, int> NodeIndexes;

        public List<TNode> Nodes;

        private bool _SelfNodeEdgeByDefault;

        public Graph(bool initializeSelfNode = false)
        {
            _SelfNodeEdgeByDefault = initializeSelfNode;
            Nodes = new List<TNode>();
            NodeIndexes = new Dictionary<TNodeKey, int>();
        }

        public int GetNodeCount()
        {
            return Nodes.Count;
        }

        public int GetEdgeCount()
        {
            return Nodes.Sum(node => node.Edges.Count);
        }

        public TNode AddNode(TNodeKey id)
        {
            if (!NodeIndexes.ContainsKey(id))
            {
                var newNode = new TNode();
                newNode.Id = id;

                //Not Thread safe
                Nodes.Add(newNode);
                NodeIndexes[id] = Nodes.Count-1;
                //Not Thread safe

                if (_SelfNodeEdgeByDefault)
                {
                    //Add self edge
                    AddEdge(newNode.Id, newNode.Id);
                }
            }

            return Nodes[NodeIndexes[id]];
        }

        public void RemoveNode(TNodeKey id)
        {
            if (!NodeIndexes.ContainsKey(id))
            {
                throw new ArgumentOutOfRangeException($"Node {id} is not found and hence can't be removed");
            }

            //Not thread safe
            var nodeIndexToRemove = GetNodeIndex(id);
            var nodeToRemove = Nodes[nodeIndexToRemove];

            Nodes.Remove(nodeToRemove);
            NodeIndexes.Remove(id);

            //Reducing all indexes after nodeIndexToRemove in NodeIndexes to update all indexes on a correct nodes in the list
            foreach (var node in NodeIndexes)
            {
                var key = node.Key;
                if (node.Value > nodeIndexToRemove)
                {
                    NodeIndexes[key] = node.Value - 1;
                }
            }

            //Not thread safe
        }

        public void BuildGraph()
        {
        }

        public TEdge AddEdge(TNodeKey from, TNodeKey to)
        {
            TNode fromNode = null;
            TNode toNode = null;
            if (NodeIndexes.ContainsKey(from)) { fromNode = Nodes[NodeIndexes[from]]; }
            if (NodeIndexes.ContainsKey(to)) { toNode = Nodes[NodeIndexes[to]]; }

            if (fromNode == null || toNode == null)
            {
                throw new ArgumentException($"Can't find nodes to construct route {from} {to}");
            }

            var newEdge = new TEdge();
            newEdge.From = fromNode;
            newEdge.To = toNode;
            //Add a link to a node
            fromNode.Edges[toNode.Id] = newEdge;

            return newEdge;
        }

        public void RemoveEdge(TNodeKey from, TNodeKey to)
        {
            if (!NodeIndexes.ContainsKey(from))
            {
                throw new ArgumentOutOfRangeException($"Node {from} is not found and hence edge ({from},{to}) can't be removed");
            }

            var fromIndex = NodeIndexes[from];
            var node = Nodes[fromIndex];

            if (!node.Edges.ContainsKey(to))
            {
                throw new ArgumentOutOfRangeException($"Edge ({from},{to}) is not found on node {from} and hence can't be removed");
            }

            var toIndex = NodeIndexes[to];
            node.Edges.Remove(to);
        }

        public int GetNodeIndex(TNodeKey id)
        {
            //return NodeList.FindIndex(elem => elem.Id.Equals(id));

            if (NodeIndexes.ContainsKey(id))
                return NodeIndexes[id];
            return -1;
        }

        public TNode GetNode(TNodeKey nodeKey)
        {
            if (!NodeIndexes.ContainsKey(nodeKey))
            {
                return null;
            }

            return Nodes[NodeIndexes[nodeKey]];
        }
    }

    public static class Algorithms
    {
        public static double[] FindShortestPathDejikstraFromNode<TNodeKey, TNode, TEdge>(
            int startNodeIndex, 
            Graph<TNodeKey, TNode, TEdge> graph, 
            Func<TEdge, double> distFunc, 
            out int[] path)
            where TNodeKey : IEquatable<TNodeKey>
            where TNode : Graph<TNodeKey>, new()
            where TEdge : Edge<Graph<TNodeKey>>, new()
        {

            var _graph = graph;

            var _nodeIndexes = _graph.NodeIndexes;
            int _nodeCount = graph.Nodes.Count;
            var _nodeList = _graph.Nodes;

            path = new int[_nodeCount];

            int searchIndex = startNodeIndex;

            PriorityQueue<int, double> priorityQueue = new PriorityQueue<int, double>();
            double[] distances = new double[_nodeCount];
            bool[] visited = new bool[_nodeCount];

            for (int index = 0; index < _nodeCount; index++)
            {
                visited[index] = false;
                path[index] = -1;
                distances[index] = double.PositiveInfinity;
            }
            distances[searchIndex] = 0;
            path[searchIndex] = -1;
            int current = searchIndex;

            int loops = 0;
            while (true)
            {
                visited[current] = true;
                foreach (var edge in _nodeList[current].Edges.Values)
                {
                    var nextNodeIndex = _nodeIndexes[edge.To.Id];
                    if (visited[nextNodeIndex])
                        continue;

                    var newDist = distances[current] + distFunc((TEdge)edge);

                    if (newDist < distances[nextNodeIndex])
                    {
                        distances[nextNodeIndex] = newDist;
                        path[nextNodeIndex] = current;
                        priorityQueue.Enqueue(nextNodeIndex, distances[nextNodeIndex]);
                    }
                }

                while (visited[current])
                {
                    if (priorityQueue.Count == 0)
                    {
                        //Console.WriteLine($"loops {loops}");
                        return distances;
                    }
                        

                    var resIndex = priorityQueue.Dequeue();
                    current = resIndex;
                }

                loops += 1;
            }
        }


        public static double[] FindShortestPathAStarFromNode<TNodeKey, TNode, TEdge>(
            int startNodeIndex, 
            int endNodeIndex, 
            Graph<TNodeKey, TNode, TEdge> graph, 
            Func<TEdge, double> distFunc, 
            Func<TEdge, double> finalDistTargetFunc, 
            out int[] path)
            where TNodeKey : IEquatable<TNodeKey>
            where TNode : Graph<TNodeKey>, new()
            where TEdge : Edge<Graph<TNodeKey>>, new()
        {
            var _graph = graph;

            var _nodeIndexes = _graph.NodeIndexes;
            int _nodeCount = graph.Nodes.Count;
            var _nodeList = _graph.Nodes;

            path = new int[_nodeCount];

            int searchIndex = startNodeIndex;

            PriorityQueue<int, double> priorityQueue = new PriorityQueue<int, double>();
            double[] distances = new double[_nodeCount];
            bool[] visited = new bool[_nodeCount];

            for (int index = 0; index < _nodeCount; index++)
            {
                visited[index] = false;
                path[index] = -1;
                distances[index] = double.PositiveInfinity;
            }
            distances[searchIndex] = 0;
            path[searchIndex] = -1;
            int current = searchIndex;

            bool exitCond = false;
            int loops = 0;
            while (!exitCond)
            {
                visited[current] = true;
                foreach (var edge in _nodeList[current].Edges.Values)
                {
                    var nextNodeIndex = _nodeIndexes[edge.To.Id];
                    if (visited[nextNodeIndex])
                        continue;

                    var newDist = distances[current] + distFunc((TEdge)edge);

                    if (newDist < distances[nextNodeIndex])
                    {
                        distances[nextNodeIndex] = newDist;
                        path[nextNodeIndex] = current;

                        //Create new EdgeToTarget to estimate its weight/dist foe heuristics
                        TEdge targetEdge = new TEdge() { From = _nodeList[nextNodeIndex], To = _nodeList[endNodeIndex] };
                        double heuDist = 0;
                        heuDist = distances[nextNodeIndex] + finalDistTargetFunc((TEdge)targetEdge);
                        priorityQueue.Enqueue(nextNodeIndex, heuDist);
                    }
                }

                while (visited[current])
                {
                    if (priorityQueue.Count == 0)
                    {
                        //Console.WriteLine($"loops {loops}");
                        //Console.WriteLine($"Priority queue count {priorityQueue.Count}, exitcond {exitCond}");
                        return distances;
                    }

                    var resIndex = priorityQueue.Dequeue();
                    current = resIndex;

                    exitCond = current == endNodeIndex;
                    if (exitCond)
                        break;
                }

                loops += 1;
            }

            //Console.WriteLine($"loops {loops}");
            //Console.WriteLine($"Priority queue count {priorityQueue.Count}, exitcond {exitCond}");
            return distances;
        }


        public static List<int> GetShortestPath(int[] shortestIndexes, int startNode, int endNode)
        {
            List<int> indexesToFollow = new List<int>();
            int currentPathIndex = endNode;
            indexesToFollow.Add(currentPathIndex);

            while (currentPathIndex != startNode)
            {
                currentPathIndex = shortestIndexes[currentPathIndex];
                if (currentPathIndex == -1) return null;
                indexesToFollow.Add(currentPathIndex);
            }

            //We are starting from end point till starting point so order is reversed
            indexesToFollow.Reverse();

            return indexesToFollow;
        }
    }

    public static class Heuristics
    {
        // implementation for integer based Manhattan Distance
        public static int ManhattanDistance(int x1, int x2, int y1, int y2)
        {
            return Math.Abs(x1 - x2) + Math.Abs(y1 - y2);
        }

        // implementation for floating-point  Manhattan Distance
        public static double ManhattanDistance(double x1, double x2, double y1, double y2)
        {
            return Math.Abs(x1 - x2) + Math.Abs(y1 - y2);
        }

        // implementation for integer based Euclidean Distance
        public static int EuclideanDistance(int x1, int x2, int y1, int y2)
        {
            int square = (x1 - x2) * (x1 - x2) + (y1 - y2) * (y1 - y2);
            return square;
        }

        // implementation for floating-point EuclideanDistance
        public static double EuclideanDistance(double x1, double x2, double y1, double y2)
        {
            double square = (x1 - x2) * (x1 - x2) + (y1 - y2) * (y1 - y2);
            return square;
        }

        // implementation for integer based Chebyshev Distance
        public static int ChebyshevDistance(int dx, int dy)
        {
            // not quite sure if the math is correct here
            return 1 * (dx + dy) + (1 - 2 * 1) * (dx - dy);
        }

        // implementation for floating-point Chebyshev Distance
        public static double ChebyshevDistance(double dx, double dy)
        {
            // not quite sure if the math is correct here
            return 1 * (dx + dy) + (1 - 2 * 1) * (dx - dy);
        }
    }
}
