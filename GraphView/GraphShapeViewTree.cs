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

    /// <summary>
    /// This class represent a visual representation of a tree structure
    /// </summary>
    /// <typeparam name="TNodePayload">Payt</typeparam>
    /// <typeparam name="TEdgePayload"></typeparam>
    public class GraphShapeViewTree<TNodeKey, TNodePayload, TEdgePayload> : GraphShapeBaseView<TNodeKey, TNodePayload, TEdgePayload>
            where TNodeKey : IEquatable<TNodeKey>
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

        public Pen EdgePen { get; set; }


        public GraphShapeViewTree()
        {
            NodeTextFont = new Font("Tahoma", 10);
            NodeBackground = new SolidBrush(Color.Red);
            NodeTextPadding = 2;
            NodeMakeSquaredForm = false;
            NodeForeground = Brushes.White;

            EdgePen = new Pen(Brushes.Gold, 2);
        }

        public GraphShapeViewTree(Point offset, int xScale, int yScale)
            : this()
        {
            this.offset = offset;
            this.xScale = xScale;
            this.yScale = yScale;
        }

        public override void AddNodes(Graph<TNodeKey, TNodePayload, TEdgePayload> graph)
        {
            //Ok we need BFS to calc all
        }

        public override void AddEdges(Graph<TNodeKey, TNodePayload, TEdgePayload> graph)
        {
            //
        }
    }
}
