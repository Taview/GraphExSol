using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GraphEx
{
    public static class Algorithms
    {
        public static double[] FindShortestPathDejikstraFromNode<TKeyNode>(
            int startNodeIndex,
            Graph<TKeyNode> graph,
            Func<Edge<TKeyNode>, double> distFunc,
            out int[] path)
            where TKeyNode : IEquatable<TKeyNode>
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

                    var newDist = distances[current] + distFunc(edge);

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


        public static double[] FindShortestPathAStarFromNode<TKeyNode>(
            int startNodeIndex,
            int endNodeIndex,
            Graph<TKeyNode> graph,
            Func<Edge<TKeyNode>, double> distFunc,
            Func<Edge<TKeyNode>, double> finalDistTargetFunc,
            out int[] path)
            where TKeyNode : IEquatable<TKeyNode>
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
            int currentFromNodeIndex = searchIndex;

            bool exitCond = false;
            int loops = 0;
            while (!exitCond)
            {
                visited[currentFromNodeIndex] = true;
                foreach (var edge in _nodeList[currentFromNodeIndex].Edges.Values)
                {
                    var toNodeIndex = _nodeIndexes[edge.To.Id];
                    if (visited[toNodeIndex])
                        continue;

                    var newDist = distances[currentFromNodeIndex] + distFunc(edge);

                    if (newDist < distances[toNodeIndex])
                    {
                        distances[toNodeIndex] = newDist;
                        path[toNodeIndex] = currentFromNodeIndex;

                        //Create new EdgeToTarget to estimate its weight/dist foe heuristics
                        var targetEdge = new Edge<TKeyNode> () { From = _nodeList[toNodeIndex], To = _nodeList[endNodeIndex]};
                        double heuDist = 0;
                        heuDist = distances[toNodeIndex] + finalDistTargetFunc(targetEdge);
                        priorityQueue.Enqueue(toNodeIndex, heuDist);
                    }
                }

                while (visited[currentFromNodeIndex])
                {
                    if (priorityQueue.Count == 0)
                    {
                        //Console.WriteLine($"loops {loops}");
                        //Console.WriteLine($"Priority queue count {priorityQueue.Count}, exitcond {exitCond}");
                        return distances;
                    }

                    var resIndex = priorityQueue.Dequeue();
                    currentFromNodeIndex = resIndex;

                    exitCond = currentFromNodeIndex == endNodeIndex;
                    if (exitCond)
                        break;
                }

                loops += 1;
            }

            //Console.WriteLine($"loops {loops}");
            //Console.WriteLine($"Priority queue count {priorityQueue.Count}, exitcond {exitCond}");
            return distances;
        }

        public static double[] FindShortestPathAStarFromNodeWithTurnCost<TKeyNode>(
            int startNodeIndex,
            int endNodeIndex,
            Graph<TKeyNode> graph,
            Func<Graph<TKeyNode>, int, int, int> getDirFunct,
            Func<int, int, int> dirPenaltyFunct,
            Func<Edge<TKeyNode>, double> distFunc,
            Func<Edge<TKeyNode>, double> finalDistTargetFunc,
            out int[] directions,
            out int[] path)
            where TKeyNode : IEquatable<TKeyNode>
        {
            var _graph = graph;

            var _nodeIndexes = _graph.NodeIndexes;
            int _nodeCount = graph.Nodes.Count;
            var _nodeList = _graph.Nodes;

            path = new int[_nodeCount];
            directions = new int[_nodeCount];

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
            int currentFromNodeIndex = searchIndex;

            bool exitCond = false;
            int loops = 0;
            while (!exitCond)
            {
                visited[currentFromNodeIndex] = true;
                foreach (var edge in _nodeList[currentFromNodeIndex].Edges.Values)
                {
                    var toNodeIndex = _nodeIndexes[edge.To.Id];
                    if (visited[toNodeIndex])
                        continue;

                    //Lets find previous edge and check its direction
                    int currentDirection = getDirFunct(graph, currentFromNodeIndex, toNodeIndex);

                    int turnCost = 0;
                    //Get previous edge if exists
                    if (path[currentFromNodeIndex] != -1)
                    {
                        //Get dir of previous segment
                        int prevIndex = path[currentFromNodeIndex];
                        int prevDirection = directions[currentFromNodeIndex];

                        //We assume that agent is moving in the same direction if no change as is
                        if (currentDirection == 0)
                        {
                            currentDirection = prevDirection;
                        }

                        turnCost = dirPenaltyFunct(currentDirection, prevDirection);
                    }

                    var newDist = distances[currentFromNodeIndex] + distFunc(edge) + turnCost;

                    if (newDist < distances[toNodeIndex])
                    {
                        directions[toNodeIndex] = currentDirection;
                        distances[toNodeIndex] = newDist;
                        path[toNodeIndex] = currentFromNodeIndex;

                        //Create new EdgeToTarget to estimate its weight/dist foe heuristics
                        var targetEdge = new Edge<TKeyNode>() { From = _nodeList[toNodeIndex], To = _nodeList[endNodeIndex] };
                        double heuDist = 0;
                        heuDist = distances[toNodeIndex] + finalDistTargetFunc(targetEdge);
                        priorityQueue.Enqueue(toNodeIndex, heuDist);
                    }
                }

                while (visited[currentFromNodeIndex])
                {
                    if (priorityQueue.Count == 0)
                    {
                        //Console.WriteLine($"loops {loops}");
                        //Console.WriteLine($"Priority queue count {priorityQueue.Count}, exitcond {exitCond}");
                        return distances;
                    }

                    var resIndex = priorityQueue.Dequeue();
                    currentFromNodeIndex = resIndex;

                    exitCond = currentFromNodeIndex == endNodeIndex;
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

        public static List<Tuple<int, int>> GetPathWithDirections(int[] shortestIndexes, int[] directions, int startNode, int endNode)
        {
            var direIndexesToFollow = new List<Tuple<int, int>>();
            int currentPathIndex = endNode;
            direIndexesToFollow.Add(new Tuple<int, int>(currentPathIndex, directions[currentPathIndex]));

            while (currentPathIndex != startNode)
            {
                currentPathIndex = shortestIndexes[currentPathIndex];
                if (currentPathIndex == -1) return null;

                var res = new Tuple<int, int>(currentPathIndex, directions[currentPathIndex]);

                direIndexesToFollow.Add(res);
            }

            //We are starting from end point till starting point so order is reversed
            direIndexesToFollow.Reverse();

            return direIndexesToFollow;
        }

        public static double GetShortestDistance(double[] distIndexes, int endNodeIndex)
        {
            return distIndexes[endNodeIndex];
        }
    }
}
