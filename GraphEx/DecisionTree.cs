using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GraphEx
{
    public class DecisionTree
    {
    }

    public class GameStateNode : Node<string>
    {
        public int Cost;
        public Point Loc;
        public Direction Dir;
        public Point ExitLocation;
        public bool IsSuccess;
        public bool isOver;
        public HashSet<Point> Blockers;
    }

    public class GameStateEdge : Edge<Node<string>> { }

    public enum Direction
    {
        NONE,
        LEFT,
        RIGHT
    }
}
