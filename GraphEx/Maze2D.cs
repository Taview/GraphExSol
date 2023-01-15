using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;

namespace GraphEx
{
    public class MazeNode2D : Node<Point>
    {
        public char NodeType { get; set; }

        public override string ToString()
        {
            return $"MazeNode2D([{Id.X},{Id.Y}])";
        }
    }

    public class MazeEdge2D : Edge2D
    {
        public override string ToString()
        {
            return $"MazeEdge2D([{From.Id.X},{From.Id.Y}] - [{To.Id.X},{To.Id.Y}])";
        }

        public static double CalcDist(Edge2D edge, Func<double, double, double, double, double> distFunc)
        {
            var start = (Node<Point>)edge.From;
            var stop = (Node<Point>)edge.To;
            return distFunc(start.Id.X, stop.Id.X, start.Id.Y, stop.Id.Y);
        }
    }

    public enum UpdateType
    {
        Create,
        Delete
    }

    public class MazeNodeUpdateEventArgs : EventArgs
    {
        public UpdateType UpdateType;
        public Point Coord;
        public char NodeType;
    }

    public class Maze2D
    {
        public int MazeHeight { get; private set; }
        public int MazeWidth { get; private set; }

        public int MazeFirstColIndex { get; private set; }
        public int MazeFirstRowIndex { get; private set; }

        public bool InverseXAxisGraph;
        public bool InverseYAxisGraph;
        public bool IncludeDiagColRow;
        public Graph<Point, MazeNode2D, Edge2D> InternalGraph;
        IList<string> _mazeRows2D;

        public event EventHandler<MazeNodeUpdateEventArgs> NodeAdded;
        public event EventHandler<MazeNodeUpdateEventArgs> NodeUpdated;

        protected virtual void OnNodeAdded(MazeNodeUpdateEventArgs mazeParams)
        {
            EventHandler<MazeNodeUpdateEventArgs> handler = NodeAdded;
            if (handler != null)
            {
                handler(this, mazeParams);
            }
        }


        protected virtual void OnNodeUpdated(MazeNodeUpdateEventArgs mazeParams)
        {
            EventHandler<MazeNodeUpdateEventArgs> handler = NodeUpdated;
            if (handler != null)
            {
                handler(this, mazeParams);
            }
        }


        public Maze2D(bool includeHeaderRowCol = true)
        {
            IncludeDiagColRow = includeHeaderRowCol;
        }

        public static void RemoveAllAdjacentEdges(Maze2D maze, Point coord)
        {
            //Remove edges from all neigbours and add connections again
            int minXIndex = 0;
            int maxXIndex = 0;

            minXIndex = Math.Max(0, coord.X - 1);
            maxXIndex = Math.Min(maze.MazeWidth - 1, coord.X + 1);

            int minYIndex = 0;
            int maxYIndex = 0;

            minYIndex = Math.Max(0, coord.Y - 1);
            maxYIndex = Math.Min(maze.MazeHeight - 1, coord.Y + 1);

            for (int yIndex = minYIndex; yIndex <= maxYIndex; yIndex++)
                for (int xIndex = minXIndex; xIndex <= maxXIndex; xIndex++)
                {
                    var from = new Point(xIndex, yIndex);
                    var to = coord;

                    if (maze.InternalGraph.IsEdgeExist(from, to))
                    {
                        maze.InternalGraph.RemoveEdge(from, to);
                    }

                    if (maze.InternalGraph.IsEdgeExist(to, from))
                    {
                        maze.InternalGraph.RemoveEdge(to, from);
                    }
                }
        }

        public void InitializeFromAscii(IList<string> mazeRows2D)
        {
            _mazeRows2D = mazeRows2D.ToList();

            MazeHeight = mazeRows2D.Count();
            MazeWidth = mazeRows2D[0].Length;

            if (IncludeDiagColRow)
            {
                MazeFirstColIndex = 1; 
                MazeFirstRowIndex = 1;
            }

            MazeWidth = MazeWidth - MazeFirstColIndex;
            MazeHeight = MazeHeight - MazeFirstRowIndex;

            InternalGraph = new Graph<Point, MazeNode2D, Edge2D>();

            for (int yRow = 0; yRow < MazeHeight; yRow++)
                for (int xCol = 0; xCol < MazeWidth; xCol++)
                {
                    var arrayCoord = GetArrayCoords(new Point(xCol, yRow));
                    var nodeType = _mazeRows2D[arrayCoord.Y][arrayCoord.X];
                    var newNode = InternalGraph.AddNode(new Point(xCol, yRow));
                    newNode.Id = new Point(xCol, yRow);
                    newNode.NodeType = nodeType;
                }

            for (int yRow = 0; yRow < MazeHeight; yRow++)
                for (int xCol = 0; xCol < MazeWidth; xCol++)
                {
                    var coord = new Point(xCol, yRow);
                    OnNodeAdded(new MazeNodeUpdateEventArgs()
                    {
                         UpdateType = UpdateType.Create,
                         Coord = coord,
                         NodeType = InternalGraph.GetNode(coord).NodeType
                    });
                }
        }

        public void EnrichWithDebugRowCol()
        {
            IList<string> _debugMazeRows2D;

            _debugMazeRows2D = new List<string>();

            var headerDiagRowList = Enumerable.Range(0, MazeWidth).Select(index => (index % 10).ToString()).ToList();

            if (InverseXAxisGraph)
                headerDiagRowList.Reverse();

            var headerRowStr = $"x{string.Join("", headerDiagRowList)}";

            _debugMazeRows2D.Add(headerRowStr);
            for (int coordY = 0; coordY < MazeHeight; coordY++)
            {
                int yIndex = coordY;
                if (InverseYAxisGraph)
                {
                    yIndex = (MazeHeight -1) - coordY;
                }

                var indexRow = (yIndex % 10).ToString();
                _debugMazeRows2D.Add($"{indexRow}{_mazeRows2D[coordY]}");
            }

            _mazeRows2D = _debugMazeRows2D;

            IncludeDiagColRow = true;
            MazeFirstColIndex = 1;
            MazeFirstRowIndex = 1;
        }

        public void UpdateMaze(Point coord, char mazeType)
        {
            int xCol = coord.X; 
            int yRow = coord.Y;
            var arrayCoord = GetArrayCoords(new Point(xCol, yRow));

            //update maze string at arrayCoord.X, arrayCoord.Y
            string rowstr = _mazeRows2D[arrayCoord.Y];

            var sb = new StringBuilder(rowstr);
            sb.Remove(arrayCoord.X, 1);
            sb.Insert(arrayCoord.X, mazeType);

            _mazeRows2D[arrayCoord.Y] = sb.ToString();

            var newNode = InternalGraph.AddNode(new Point(xCol, yRow));
            newNode.Id = new Point(xCol, yRow);
            newNode.NodeType = mazeType;

            var coord2 = new Point(xCol, yRow);
            OnNodeUpdated(new MazeNodeUpdateEventArgs()
            {
                UpdateType = UpdateType.Create,
                Coord = coord2,
                NodeType = InternalGraph.GetNode(coord).NodeType
            });
        }

        private Point GetArrayCoords(Point coordPlain)
        {
            Point retVal = new Point();

            if (!InverseXAxisGraph)
                retVal.X = coordPlain.X + MazeFirstColIndex;
            else
                retVal.X = ( MazeWidth -1) - coordPlain.X + MazeFirstColIndex;

            if (!InverseYAxisGraph)
                retVal.Y = coordPlain.Y + MazeFirstRowIndex;
            else
                retVal.Y = ( MazeHeight-1 ) - coordPlain.Y + MazeFirstRowIndex;

            return retVal;
        }

        public string PrintMaze(bool showDiagInfo = true)
        {
            int startIndexRow = showDiagInfo ? 0 : MazeFirstRowIndex;
            int startIndexCol = showDiagInfo ? 0 : MazeFirstColIndex;

            var sb = new StringBuilder();
            for(int yRow = startIndexRow; yRow < MazeHeight + MazeFirstRowIndex; yRow++)
                sb.AppendLine(_mazeRows2D[yRow].Substring(startIndexCol));

            return sb.ToString();
        }

        public string PrintPathOverlay(string baseStr, char overlayChar, IEnumerable<MazeNode2D> points, bool extraSpaces = true, bool showDiagInfo = true)
        {
            int startIndexRow = showDiagInfo ? 0 : MazeFirstRowIndex;
            int startIndexCol = showDiagInfo ? 0 : MazeFirstColIndex;

            var overlayHash = new HashSet<Point>();
            foreach (var point in points)
            {
                overlayHash.Add(point.Id);
            }

            char[,] mazeViewChars;

            string[] baseMazeArray = baseStr.Split(new Char[] { ',', '\n' })
                                    .Where(sline => !string.IsNullOrWhiteSpace(sline))
                                    .ToArray();

            int width = baseMazeArray[0].Length;
            int height = baseMazeArray.Length;

            mazeViewChars = new char[width, height];

            StringBuilder sb = new StringBuilder();
            for (int y = 0; y < height; y++)
            {
                var rowString = baseMazeArray[y];
                for (int x = 0; x < width; x++)
                {
                    var stringCoords = new Point(x, y);
                    var arrayCoord = GetArrayCoords(stringCoords);
                    arrayCoord.X -= 2;

                    if (overlayHash.Contains(arrayCoord))
                        sb.Append(overlayChar);
                    else
                        sb.Append(rowString[x]);
                }
                if(extraSpaces)
                sb.AppendLine();
            }

            return sb.ToString();
        }
    }
}
