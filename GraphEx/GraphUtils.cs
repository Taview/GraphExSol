using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GraphEx
{
    public class GraphUtils
    {
        public static double GetDistance(Edge<Point> edge,
            Func<int, int, int, int, int> distFunc)
        {
            var from = edge.From;
            var to = edge.To;

            return distFunc(from.Id.X, to.Id.X, from.Id.Y, to.Id.Y);
        }
    }
}
