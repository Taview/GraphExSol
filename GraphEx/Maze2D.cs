using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GraphEx
{
    public class MazeNode2D : Node<Point>
    {
        public char NodeType { get; set; }

        public override string ToString()
        {
            return $"Maze2D([{Id.X},{Id.Y}])";
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

        public void InitializeFromAscii(IList<string> mazeRows2D)
        {
            _mazeRows2D = mazeRows2D.ToList();

            MazeHeight = mazeRows2D.Count();
            MazeWidth = mazeRows2D[0].Length;

            if (IncludeDiagColRow)
            {
                MazeFirstColIndex = 1; 
                MazeFirstRowIndex = 1;

                MazeWidth = MazeWidth - MazeFirstColIndex;
                MazeHeight = MazeHeight - MazeFirstRowIndex;
            }

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
                retVal.X = MazeWidth - coordPlain.X;

            if (!InverseYAxisGraph)
                retVal.Y = coordPlain.Y + MazeFirstColIndex;
            else
                retVal.Y = MazeHeight - coordPlain.Y;

            return retVal;
        }

        public string PrintMaze(bool showDiagInfo = true)
        {
            int startIndexRow = showDiagInfo ? 0 : MazeFirstRowIndex;
            int startIndexCol = showDiagInfo ? 0 : MazeFirstColIndex;

            var sb = new StringBuilder();
            for(int yRow = startIndexRow; yRow <= MazeHeight; yRow++)
                sb.AppendLine(_mazeRows2D[yRow].Substring(startIndexCol));

            return sb.ToString();
        }
    }
}
