using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GraphEx
{
    public static class Heuristics
    {
        // implementation for integer based Manhattan Distance
        public static int ManhattanDistance(int x1, int x2, int y1, int y2)
        {
            return Math.Abs(x1 - x2) + Math.Abs(y1 - y2);
        }

        // implementation for floating-point  Manhattan Distance
        public static double ManhattanDistance(double x1, double x2, double y1, double y2)
        {
            return Math.Abs(x1 - x2) + Math.Abs(y1 - y2);
        }

        // implementation for integer based Euclidean Distance
        public static int EuclideanDistance(int x1, int x2, int y1, int y2)
        {
            int square = (x1 - x2) * (x1 - x2) + (y1 - y2) * (y1 - y2);
            return square;
        }

        // implementation for floating-point EuclideanDistance
        public static double EuclideanDistance(double x1, double x2, double y1, double y2)
        {
            double square = (x1 - x2) * (x1 - x2) + (y1 - y2) * (y1 - y2);
            return square;
        }

        // implementation for integer based Chebyshev Distance
        public static int ChebyshevDistance(int dx, int dy)
        {
            // not quite sure if the math is correct here
            return 1 * (dx + dy) + (1 - 2 * 1) * (dx - dy);
        }

        // implementation for floating-point Chebyshev Distance
        public static double ChebyshevDistance(double dx, double dy)
        {
            // not quite sure if the math is correct here
            return 1 * (dx + dy) + (1 - 2 * 1) * (dx - dy);
        }
    }
}
