using GraphEx;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WorldCitiesNet.Models;

namespace Graphex.Test
{
    internal class MazeTests
    {
        [Test]
        public void ShouldConstructMazeFromAscii()
        {
            Maze2D maze = new Maze2D();
            maze.IncludeDiagColRow = true;
            maze.InverseYAxisGraph = true;
            maze.NodeAdded += Maze_NodeAdded;

            List<string> mazeRows2D = new List<string>()
            {
                "x0123456789012345678",
                "0.........X.........",
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
        }


        [Test]
        public void ShouldUpdateEdgesCorrectly()
        {
            Maze2D maze = new Maze2D();
            maze.IncludeDiagColRow = true;
            maze.InverseYAxisGraph = true;
            maze.NodeAdded += Maze_NodeAdded;
            maze.NodeUpdated += Maze_NodeUpdated;

            List<string> mazeRows2D = new List<string>()
            {
                "x01234",
                "2.....",
                "1.....",
                "0.....",

            };

            maze.InitializeFromAscii(mazeRows2D);
            Console.WriteLine(maze.PrintMaze());

            Assert.AreEqual(15, maze.InternalGraph.GetNodeCount());
            Assert.AreEqual(24, maze.InternalGraph.GetEdgeCount());

            maze.UpdateMaze(new Point(0, 0), 'L');
            maze.UpdateMaze(new Point(1, 0), 'L');

            Console.WriteLine(maze.PrintMaze());
            Assert.AreEqual(15, maze.InternalGraph.GetNodeCount());
            Assert.AreEqual(26, maze.InternalGraph.GetEdgeCount());
        }

        private void Maze_NodeUpdated(object sender, MazeNodeUpdateEventArgs e)
        {
            Maze2D maze = (Maze2D)sender;
            Maze2D.RemoveAllAdjacentEdges(maze, e.Coord);
            AddNodeConnections(e.Coord, e.NodeType, (Maze2D)sender);
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
                            var to = new Point(coord.X, coord.Y  + 1);
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

        private void AddHorizontalConnections(Point coord, Graph<Point, MazeNode2D, Edge2D> graph, int maxWidth)
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

        [Test]
        [TestCase(0, 0, 3, 1, false, false)] //TestName = "ShouldCreateGraphNoInvert"
        [TestCase(3, 0, 0, 1, true,  false)] //TestName = "ShouldCreateGraphWithXAxisInvert"
        [TestCase(0, 1, 3, 0, false, true)] //TestName = "ShouldCreateGraphWithYAxisInvert"
        [TestCase(3, 1, 0, 0, true,  true)] //TestName = "ShouldCreateGraphWithX&YAxisInvert"
        public void ShouldCorrectlyCreateGraphWithInverseAxes(int ax, int ay, int hx, int hy, bool invertXAxis, bool invertYAxis)
        {
            Maze2D maze = new Maze2D();
            maze.IncludeDiagColRow = true;
            maze.InverseXAxisGraph = invertXAxis;
            maze.InverseYAxisGraph = invertYAxis;

            List<string> mazeRows2D = new List<string>()
            {
                "x0123",
                "0abcd",
                "1efgh",

            };

            maze.InitializeFromAscii(mazeRows2D);
            var nodeA = maze.InternalGraph.GetNode(new Point(ax, ay));
            var nodeH = maze.InternalGraph.GetNode(new Point(hx, hy));

            Assert.AreEqual('a', nodeA.NodeType);
            Assert.AreEqual('h', nodeH.NodeType);
        }
    }
}
