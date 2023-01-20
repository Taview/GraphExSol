namespace GraphView
{
    using System.Drawing;
    using System.Windows.Forms;

    public partial class GraphViewForm : Form
    {
        public IGraphBase GraphView;

        ScrollableControl myScrollableControl;

        private Timer animationTimer;

        public GraphViewForm()
        {
            InitializeComponent();
        }

        public GraphViewForm(IGraphBase graphView)
            :this()
        {
            SuspendLayout();
            myScrollableControl = new ScrollableControl();
            myScrollableControl.AutoScroll = true;
            this.Controls.Add(myScrollableControl);

            myScrollableControl.Dock= DockStyle.Fill;
            myScrollableControl.Size = new Size(1264, 985);

            myScrollableControl.Paint += new PaintEventHandler(GraphView_Paint);
            ResumeLayout();

            this.GraphView = graphView;
        }

        private void GraphView_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.TranslateTransform(myScrollableControl.AutoScrollPosition.X, myScrollableControl.AutoScrollPosition.Y);

            if (GraphView != null)
            {
                GraphView.Render(this, e);
                var PrefSize = GraphView.GetPreferredSize();

                if (myScrollableControl != null)
                {
                    myScrollableControl.AutoScrollMinSize = 
                        new Size(PrefSize.Width, PrefSize.Height);
                }
            }
        }
    }
}