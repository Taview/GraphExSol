using System.Drawing.Drawing2D;
using System.Drawing;

public class EdgeShapeView
{
    private Point start;
    private Point end;
    public Pen Pen;

    public EdgeShapeView(Point start, Point end)
    {
        Pen = new Pen(Color.Black, 2);
        this.start = start;
        this.end = end;
    }

    public void Render(PaintEventArgs e)
    {
        e.Graphics.DrawLine(Pen, start, end);
    }
}