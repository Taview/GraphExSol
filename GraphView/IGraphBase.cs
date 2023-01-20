namespace GraphView
{
    public interface IGraphBase
    {
        public Size GetPreferredSize();

        void Render(object sender, PaintEventArgs e);
    }
}