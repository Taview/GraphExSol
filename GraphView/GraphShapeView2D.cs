using GraphEx;
using System.Drawing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TrayNotify;

namespace GraphView
{
    public class GraphShapeView2D<TNodePayload, TEdgePayload> : GraphShapeBaseView<Point, TNodePayload, TEdgePayload>
            where TNodePayload : new()
            where TEdgePayload : new()
    {

        public int GraphWidth;
        public int GraphHeight;
        public Point offset;
        public int xScale;
        public int yScale;

        public int NodeTextPadding { get; set; }

        public Font NodeTextFont { get; set; }

        public Brush NodeBackground { get; set; }

        public Brush NodeForeground { get; set; }

        public bool NodeMakeSquaredForm { get; set; }

        public bool InverseXAxisGraph;

        public bool InverseYAxisGraph;

        public Pen EdgePen { get; set; }


        public GraphShapeView2D()
        {
            NodeTextFont = new Font("Tahoma", 10);
            NodeBackground = new SolidBrush(Color.Red);
            NodeTextPadding = 2;
            NodeMakeSquaredForm = false;
            NodeForeground = Brushes.White;

            EdgePen = new Pen(Brushes.Gold, 2);
        }

        public GraphShapeView2D(Point offset, int xScale, int yScale)
            : this()
        {
            this.offset = offset;
            this.xScale = xScale;
            this.yScale = yScale;
        }

        public override void AddEdges(Graph<Point, TNodePayload, TEdgePayload> graph)
        {

            foreach (var node in graph.Nodes)
            {
                foreach (var edge in node.Edges)
                {
                    // Get the start and end points for the edge

                    Point start = NodeViews[graph.NodeIndexes[edge.Value.From.Id]].Coord;
                    Point end = NodeViews[graph.NodeIndexes[edge.Value.To.Id]].Coord;

                    // Create a line for the edge

                    EdgeShapeView edgeLine = new EdgeShapeView(start, end) { Pen = EdgePen };
                    EdgeViews.Add(edgeLine);
                }
            }
        }

        private Point GetArrayCoords(Point coordPlain)
        {
            Point retVal = new Point();

            if (!InverseXAxisGraph)
                retVal.X = coordPlain.X;
            else
                retVal.X = (GraphWidth - 1) - coordPlain.X;

            if (!InverseYAxisGraph)
                retVal.Y = coordPlain.Y;
            else
                retVal.Y = (GraphHeight - 1) - coordPlain.Y;

            return retVal;
        }

        public override void AddNodes(Graph<Point, TNodePayload, TEdgePayload> graph)
        {
            GraphWidth = graph.Nodes.Max(node => node.Id.X);
            GraphHeight = graph.Nodes.Max(node => node.Id.Y);

            foreach (var node in graph.Nodes)
            {
                var nodeShapeView = new NodeShapeView();

                var graphCoord = GetArrayCoords(node.Id);
                nodeShapeView.Coord = new Point(graphCoord.X * xScale + offset.X, graphCoord.Y * yScale + offset.Y);

                nodeShapeView.TextFont = NodeTextFont;
                nodeShapeView.Background = NodeBackground;
                nodeShapeView.TextPadding = NodeTextPadding;
                nodeShapeView.MakeSquaredForm = NodeMakeSquaredForm;
                nodeShapeView.Foreground = NodeForeground;

                StringBuilder sb = new StringBuilder();
                sb.AppendLine($"Y={node.Id.Y}");
                sb.AppendLine($"X={node.Id.X}");

                int widthExtend = nodeShapeView.Coord.X + (xScale / 2);
                int heightExtend = nodeShapeView.Coord.Y + (yScale / 2);

                if (PrefferedWidth < widthExtend)
                {
                    PrefferedWidth = widthExtend;
                }

                if (PrefferedHeight < heightExtend)
                {
                    PrefferedHeight = heightExtend;
                }

                nodeShapeView.NodeKey = sb;
                nodeShapeView.TextPadding = 2;
                NodeViews.Add(nodeShapeView);
            }
        }
    }
}
