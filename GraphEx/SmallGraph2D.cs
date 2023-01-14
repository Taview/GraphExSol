using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace GraphEx
{
    //This graph is using space complexity of O(V^2) while time complexity is O(1) for most operations
    public class SmallGraph2D
    {
        public readonly int Width;
        public readonly int Height;
        public readonly int NodesTotal;
        private int EdgesTotal;

        private bool[,] _adjancencyMatrix;

        public SmallGraph2D(int width, int height)
        {
            Width = width;
            Height = height;
            NodesTotal = Width * Height;

            _adjancencyMatrix = new bool[NodesTotal, NodesTotal];
        }

        public int GetIndex(int x, int y)
        {
            return y*Width+x;
        }

        public void AddEdge(int startNode, int endNode)
        {
            _adjancencyMatrix[startNode, endNode] = true;
            EdgesTotal++;
        }
        public void RemoveEdge(int startNode, int endNode)
        {
            _adjancencyMatrix[startNode, endNode] = false;
            EdgesTotal--;
        }

        public object GetNodeCount()
        {
            return NodesTotal;
        }

        public object GetEdgeCount()
        {
            return EdgesTotal;
        }

        //public IEnumerable<T> Flatten<T>(T[,] map)
        //{
        //    for (int row = 0; row < map.GetLength(0); row++)
        //    {
        //        for (int col = 0; col < map.GetLength(1); col++)
        //        {
        //            yield return map[row, col];
        //        }
        //    }
        //}
    }
}
