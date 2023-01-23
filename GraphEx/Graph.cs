using System;
using System.Collections.Generic;
using System.Linq;

namespace GraphEx
{
    public class Node<TNodeKey>
    where TNodeKey : IEquatable<TNodeKey>
    {
        public TNodeKey Id { get; set; }

        public Object Payload { get; set; }

        public Dictionary<TNodeKey, Edge<TNodeKey>> Edges { get; set; }

        public Node()
        {
            Edges = new Dictionary<TNodeKey, Edge<TNodeKey>>();
        }

        public override string ToString()
        {
            return $"Node[{Id}] - ({Payload})";
        }
    }

    public class Edge<TNodeKey>
        where TNodeKey : IEquatable<TNodeKey>
    {
        public Node<TNodeKey> From { get; set; }
        public Node<TNodeKey> To { get; set; }
        //public double Distance { get; set; }

        public Object Payload { get; set; }

        public Edge() { }

        public override string ToString()
        {
            return $"Edge [{From} -> {To}]";
        }
    }

    public class Graph<TNodeKey>
    where TNodeKey : IEquatable<TNodeKey>
    {
        public Dictionary<TNodeKey, int> NodeIndexes;

        public List<Node<TNodeKey>> Nodes;

        private bool _SelfNodeEdgeByDefault;

        public Graph(bool initializeSelfNode = false)
        {
            _SelfNodeEdgeByDefault = initializeSelfNode;
            Nodes = new List<Node<TNodeKey>>();
            NodeIndexes = new Dictionary<TNodeKey, int>();
        }

        public int GetNodeCount()
        {
            return Nodes.Count;
        }

        public int GetEdgeCount()
        {
            return Nodes.Sum(node => node.Edges.Count);
        }

        public Node<TNodeKey> AddNode(TNodeKey id)
        {
            if (!NodeIndexes.ContainsKey(id))
            {
                var newNode = new Node<TNodeKey>();
                newNode.Id = id;

                //Not Thread safe
                Nodes.Add(newNode);
                NodeIndexes[id] = Nodes.Count-1;
                //Not Thread safe

                if (_SelfNodeEdgeByDefault)
                {
                    //Add self edge
                    AddEdge(newNode.Id, newNode.Id);
                }
            }

            return Nodes[NodeIndexes[id]];
        }

        public void RemoveNode(TNodeKey id)
        {
            if (!NodeIndexes.ContainsKey(id))
            {
                throw new ArgumentOutOfRangeException($"Node {id} is not found and hence can't be removed");
            }

            //Not thread safe
            var nodeIndexToRemove = GetNodeIndex(id);
            var nodeToRemove = Nodes[nodeIndexToRemove];

            Nodes.Remove(nodeToRemove);
            NodeIndexes.Remove(id);

            //Reducing all indexes after nodeIndexToRemove in NodeIndexes to update all indexes on a correct nodes in the list
            foreach (var node in NodeIndexes)
            {
                var key = node.Key;
                if (node.Value > nodeIndexToRemove)
                {
                    NodeIndexes[key] = node.Value - 1;
                }
            }

            //Not thread safe
        }

        public void BuildGraph()
        {
        }

        public Edge<TNodeKey> AddEdge(TNodeKey from, TNodeKey to)
        {
            Node<TNodeKey> fromNode = null;
            Node<TNodeKey> toNode = null;
            if (NodeIndexes.ContainsKey(from)) { fromNode = Nodes[NodeIndexes[from]]; }
            if (NodeIndexes.ContainsKey(to)) { toNode = Nodes[NodeIndexes[to]]; }

            if (fromNode == null || toNode == null)
            {
                throw new ArgumentException($"Can't find nodes to construct route {from} {to}");
            }

            var newEdge = new Edge<TNodeKey>();
            newEdge.From = fromNode;
            newEdge.To = toNode;
            //Add a link to a node
            fromNode.Edges[toNode.Id] = newEdge;

            return newEdge;
        }

        public bool IsEdgeExist(TNodeKey from, TNodeKey to)
        {
            Node<TNodeKey> fromNode = null;
            if (NodeIndexes.ContainsKey(from)) { fromNode = Nodes[NodeIndexes[from]]; }

            if (fromNode == null)
            {
                return false;
            }

            return fromNode.Edges.ContainsKey(to);
        }

        public void RemoveEdge(TNodeKey from, TNodeKey to)
        {
            if (!NodeIndexes.ContainsKey(from))
            {
                throw new ArgumentOutOfRangeException($"Node {from} is not found and hence edge ({from},{to}) can't be removed");
            }

            var fromIndex = NodeIndexes[from];
            var node = Nodes[fromIndex];

            if (!node.Edges.ContainsKey(to))
            {
                throw new ArgumentOutOfRangeException($"Edge ({from},{to}) is not found on node {from} and hence can't be removed");
            }

            var toIndex = NodeIndexes[to];
            node.Edges.Remove(to);
        }

        public int GetNodeIndex(TNodeKey id)
        {
            //return NodeList.FindIndex(elem => elem.Id.Equals(id));

            if (NodeIndexes.ContainsKey(id))
                return NodeIndexes[id];
            return -1;
        }

        public Node<TNodeKey> GetNode(TNodeKey nodeKey)
        {
            if (!NodeIndexes.ContainsKey(nodeKey))
            {
                return null;
            }

            return Nodes[NodeIndexes[nodeKey]];
        }
    }
}
