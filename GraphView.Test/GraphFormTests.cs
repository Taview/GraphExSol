using GraphEx;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace GraphView.Test
{
    [TestFixture, SingleThreaded, Explicit("Tests should be run manually as they contain UI elements")]
    public class GraphFormTests
    {
        private GraphViewForm m_myForm;

        [Test]
        //[Ignore("This test is for Demo purposes only comment ignore attribute to proceed")]
        public void Demo2DGraphs()
        {
            var graphView2D = new GraphShapeView2D<int, int>(new Point(20, 35), 60, 70);
            graphView2D.NodeMakeSquaredForm = true;
            graphView2D.NodeTextPadding = 5;
            graphView2D.NodeTextFont = new Font("Tahoma",8);
            graphView2D.NodeBackground = Brushes.Wheat;
            graphView2D.NodeForeground = Brushes.DarkMagenta;

            graphView2D.EdgePen = new Pen(Color.LightGray, 1);

            float[] dashValues = { 4, 2, 4, 2 };
            graphView2D.EdgePen.DashPattern = dashValues;

            var myGraphViewForm = new GraphViewForm(graphView2D);
            
            myGraphViewForm.Text = "DFS demo";

            myGraphViewForm.BackColor = Color.Black;


            var graph = new Graph<Point, int, int>();

            int width = 69;
            int height = 13;

            for (int iY = 0; iY < height; iY++)
            {
                for (int iX = 0; iX < width; iX++)
                {
                    graph.AddNode(new Point(iX, iY));

                    if (iX > 0)
                    {
                        var from = new Point(iX - 1, iY);
                        var to = new Point(iX, iY);
                        
                        graph.AddEdge(from, to);
                    }

                    if (iY > 0)
                    {
                        var from = new Point(iX, iY - 1);
                        var to = new Point(iX, iY);

                        //if(iY % 3 !=0 )
                        graph.AddEdge(from, to);
                    }
                }

            }

           graphView2D.Init(graph);

           myGraphViewForm.ShowDialog(null);

        }

        [Test]
        //[Ignore("This test is for Demo purposes only comment ignore attribute to proceed")]
        public void ShowDejikstra()
        {
            Maze2D maze = new Maze2D();
            maze.IncludeDiagColRow = true;
            maze.InverseYAxisGraph = true;
            maze.NodeAdded += Maze_NodeAdded;

            List<string> mazeRows2D = new List<string>()
            {
                "x0123456789012345678",
                "9.........X.........",
                "8.........L.........",
                "7....L............L.",
                "6...L.....L.........",
                "5....L............L.",
                "4...L.....L.........",
                "3....L............L.",
                "2...L.....L.........",
                "1....L............L.",
                "0...L.....L.........",

            };

            maze.InitializeFromAscii(mazeRows2D);
            string res = maze.PrintMaze();
            Console.WriteLine(res);
            Console.WriteLine($"Node count {maze.InternalGraph.GetNodeCount()}");
            Console.WriteLine($"Edge count {maze.InternalGraph.GetEdgeCount()}");

            Point startPoint = new Point(5, 0);
            int startNodeIndex = maze.InternalGraph.GetNodeIndex(startPoint);

            Point endPoint = new Point(9, 9);
            int endNodeIndex = maze.InternalGraph.GetNodeIndex(endPoint);

            int[] shortestIndexes;
            int[] directions;

            //var distances = Algorithms.FindShortestPathDejikstraFromNode(startNodeIndex, maze.InternalGraph,
            //    route => Edge2D.CalcDist(route, Heuristics.EuclideanDistance),
            //    out shortestIndexes);

            var distances = Algorithms.FindShortestPathAStarFromNodeWithTurnCost(
                                startNodeIndex, endNodeIndex, maze.InternalGraph,
                                GetDirectionOfEdge,
                                GetTurnPenalty,
                                route => CalcDistPoint(route, Heuristics.ManhattanDistance),
                                route => 0,
                                out directions,
                                out shortestIndexes);

            var dirs = Algorithms.GetPathWithDirections(shortestIndexes, directions, startNodeIndex, endNodeIndex);
            var dirsResult = dirs.Select(dirNode => new { Node = maze.InternalGraph.Nodes[dirNode.Item1], Dir = dirNode.Item2 });

            var pathStations = Algorithms.GetShortestPath(shortestIndexes, startNodeIndex, endNodeIndex);
            var len = Algorithms.GetShortestDistance(distances, endNodeIndex);

            Console.WriteLine($"Shortest Length {len}");

            var pathToFollow = pathStations.Select(nodeIndex => maze.InternalGraph.Nodes[nodeIndex]);

            var resPath = string.Join(',', pathToFollow);
            Console.WriteLine(resPath);

            Console.WriteLine(maze.PrintPathOverlay(res, '+', pathToFollow, false));

            //Initilize View

            var graphView2D = new GraphShapeView2D<MazeNode2D, MazeEdge2D>(new Point(100, 200), 60, 70);
            graphView2D.InverseYAxisGraph = true;
            graphView2D.NodeMakeSquaredForm = true;
            graphView2D.NodeTextPadding = 5;
            graphView2D.NodeTextFont = new Font("Tahoma", 8);
            graphView2D.NodeBackground = Brushes.Wheat;
            graphView2D.NodeForeground = Brushes.DarkMagenta;

            graphView2D.EdgePen = new Pen(Color.LightGray, 1);

            float[] dashValues = { 4, 2, 4, 2 };
            graphView2D.EdgePen.DashPattern = dashValues;

            var myGraphViewForm = new GraphViewForm(graphView2D);

            myGraphViewForm.Text = "ShowDejikstra demo";

            myGraphViewForm.BackColor = Color.Black;

            graphView2D.Init(maze.InternalGraph);

            myGraphViewForm.ShowDialog(null);

        }

        private void Maze_NodeAdded(object sender, MazeNodeUpdateEventArgs e)
        {
            AddNodeConnections(e.Coord, e.NodeType, (Maze2D)sender);
        }

        private void AddNodeConnections(Point coord, char NodeType, Maze2D maze)
        {
            switch (NodeType)
            {
                case 'X':
                case 'L':
                    {
                        AddHorizontalConnections(coord, maze.InternalGraph, maze.MazeWidth);

                        //This nodes are elevator types nodes
                        if (coord.Y < maze.MazeHeight - 1)
                        {
                            var from = new Point(coord.X, coord.Y);
                            var to = new Point(coord.X, coord.Y + 1);
                            maze.InternalGraph.AddEdge(from, to);
                        }

                        break;
                    }
                case '.':
                    {
                        //Regular empty node
                        AddHorizontalConnections(coord, maze.InternalGraph, maze.MazeWidth);
                        break;
                    }
                default:
                    {
                        throw new ArgumentOutOfRangeException($"Unknown node {NodeType} in the maze");
                    }
            }
        }

        private void AddHorizontalConnections(Point coord, Graph<Point, MazeNode2D, MazeEdge2D> graph, int maxWidth)
        {
            if (coord.X > 0)
            {
                var prev = new Point(coord.X - 1, coord.Y);
                var current = new Point(coord.X, coord.Y);
                graph.AddEdge(prev, current);
                graph.AddEdge(current, prev);
            }

            if (coord.X + 1 < maxWidth)
            {
                var current = new Point(coord.X, coord.Y);
                var next = new Point(coord.X + 1, coord.Y);
                graph.AddEdge(current, next);
                graph.AddEdge(next, current);
            }
        }

        private int GetDirectionOfEdge(Graph<Point, MazeNode2D, MazeEdge2D> graph, int indexFrom, int indexTo)
        {
            var nodeFromCoord = graph.Nodes[indexFrom];
            var nodeToCoord = graph.Nodes[indexTo];

            return nodeFromCoord.Id.X.CompareTo(nodeToCoord.Id.X);
        }

        private int GetTurnPenalty(int dir1, int dir2)
        {
            if (dir1 != dir2)
            {
                return 5;
            }


            return 0;
        }

        public static double CalcDistPoint<TNodePayloadType, TEdgePayloadType>(
        Edge<Node<Point, TNodePayloadType, TEdgePayloadType>, TEdgePayloadType> edge,
        Func<int, int, int, int, int> distFunc)
        {
            var from = edge.From;
            var to = edge.To;
            return distFunc(from.Id.X, to.Id.X, from.Id.Y, to.Id.Y);
        }
    }
}