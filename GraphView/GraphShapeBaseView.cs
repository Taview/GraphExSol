using GraphEx;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace GraphView
{
    public abstract class GraphShapeBaseView<TNodeKey> : IGraphBase 
            where TNodeKey : IEquatable<TNodeKey>
    {
        public List<NodeShapeView> NodeViews;
        public List<EdgeShapeView> EdgeViews;

        public int PrefferedWidth;
        public int PrefferedHeight;

        Random r = new Random();

        public void Init(Graph<TNodeKey> graph)
        {
            // Add some nodes to the graph
            this.NodeViews = new List<NodeShapeView>();
            this.EdgeViews = new List<EdgeShapeView>();

            // Add the node labels and edges to the form
            AddNodes(graph);
            AddEdges(graph);
        }

        public virtual Size GetPreferredSize()
        {
            return new Size(PrefferedWidth, PrefferedHeight);
        }

        public abstract void AddNodes(Graph<TNodeKey> graph);

        public abstract void AddEdges(Graph<TNodeKey> graph);


        public void Render(object sender, PaintEventArgs e)
        {
            if (EdgeViews != null)
            {
                foreach (var edge in EdgeViews)
                {
                    edge.Render(e);
                }
            }

            if (EdgeViews != null)
            {
                foreach (var nodeLabel in NodeViews)
                {
                    nodeLabel.Render(e);
                }
            }
        }
    }
}
