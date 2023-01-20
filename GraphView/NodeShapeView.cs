using GraphEx;
using System;
using System.Xml.Linq;

public class NodeShapeView
{
    public Object NodeKey { get; set; }

    public Point Coord { get; set; }

    public int TextPadding { get; set; }

    public Font TextFont { get; set; }

    public Brush Background { get; set; }

    public Brush Foreground { get; set; }

    public bool MakeSquaredForm { get; set; }

    public NodeShapeView()
    {
        //Populate defaults
        TextFont = new Font("Tahoma", 10);
        Background = new SolidBrush(Color.Red);
        TextPadding = 2;
        MakeSquaredForm = false;
        Foreground = Brushes.White;
    }

    public void Render(PaintEventArgs e)
    {
        var nodeID = NodeKey?.ToString();
        var strSizeF = e.Graphics.MeasureString(nodeID, TextFont);
        var backgroundBoundingBox = GetCenterRect(Coord, strSizeF, MakeSquaredForm, TextPadding);
        var textBoundingBox = GetCenterRect(Coord, strSizeF, false, 0);

        var centeredCoord = GetCenterCoord(backgroundBoundingBox);

        // Fill ellipse on screen.
        e.Graphics.FillEllipse(Background, backgroundBoundingBox);
        e.Graphics.DrawString(nodeID, TextFont, Foreground, new PointF(textBoundingBox.X, textBoundingBox.Y));
    }

    private Rectangle GetCenterRect(Point start, SizeF areaSize, bool makeSquared = false, int paddings = 0)
    {
        var areaSizeToCalc = areaSize;

        if (makeSquared)
        {
            areaSizeToCalc = new SizeF(Math.Max(areaSize.Width, areaSize.Height), Math.Max(areaSize.Width, areaSize.Height));
        }

        Point centeredCoord = new Point(start.X - ((int)areaSizeToCalc.Width / 2), start.Y - ((int)areaSizeToCalc.Height / 2));

        return new Rectangle(centeredCoord.X - paddings, centeredCoord.Y - paddings, (int)areaSizeToCalc.Width + (paddings * 2), (int)areaSizeToCalc.Height + (paddings * 2));
    }

    private Point GetCenterCoord(Rectangle area)
    {
        return new Point(area.X + (area.Width/2), area.Y + (area.Height / 2));
    }
}