using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Xml.Linq;
using System.Xml.Schema;
using static System.Collections.Specialized.BitVector32;

namespace GraphEx
{
    public class NodeV<TKey, TPayload>
    where TKey : IEquatable<TKey>
    where TPayload : new()
    {
        public TKey Id { get; set; }

        public TPayload Content { get; set; }

        public Dictionary<TKey, EdgeV<NodeV<TKey, TPayload>>> Edges { get; set; }

        public NodeV()
        {
            Edges = new Dictionary<TKey, EdgeV<NodeV<TKey, TPayload>>>();
        }
    }

    public class EdgeV<TNode>
    {
        public TNode From { get; set; }
        public TNode To { get; set; }
        public double Distance { get; set; }
        public EdgeV() { }
    }

    public class GraphV<TKeyNode, TNodePayload, TEdgePayload>
    where TKeyNode : IEquatable<TKeyNode>
    where TNodePayload : new()
    where TEdgePayload : new()
    {
        public Dictionary<TKeyNode, int> NodeIndexes;

        public List<NodeV<TKeyNode, TNodePayload>> Nodes;

        private bool _SelfNodeEdgeByDefault;

        public GraphV(bool initializeSelfNode = false)
        {
            _SelfNodeEdgeByDefault = initializeSelfNode;
            Nodes = new List<NodeV<TKeyNode, TNodePayload>>();
            NodeIndexes = new Dictionary<TKeyNode, int>();
        }

        public int GetNodeCount()
        {
            return Nodes.Count;
        }

        public int GetEdgeCount()
        {
            return Nodes.Sum(node => node.Edges.Count);
        }

        public NodeV<TKeyNode, TNodePayload> AddNode(TKeyNode id)
        {
            if (!NodeIndexes.ContainsKey(id))
            {
                var newNode = new NodeV<TKeyNode, TNodePayload>();
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

        public void RemoveNode(TKeyNode id)
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

        public EdgeV<NodeV<TKeyNode, TNodePayload>> AddEdge(TKeyNode from, TKeyNode to)
        {
            NodeV<TKeyNode, TNodePayload> fromNode = null;
            NodeV<TKeyNode, TNodePayload> toNode = null;
            if (NodeIndexes.ContainsKey(from)) { fromNode = Nodes[NodeIndexes[from]]; }
            if (NodeIndexes.ContainsKey(to)) { toNode = Nodes[NodeIndexes[to]]; }

            if (fromNode == null || toNode == null)
            {
                throw new ArgumentException($"Can't find nodes to construct route {from} {to}");
            }

            var newEdge = new EdgeV<NodeV<TKeyNode, TNodePayload>>();
            newEdge.From = fromNode;
            newEdge.To = toNode;
            //Add a link to a node
            fromNode.Edges[toNode.Id] = newEdge;

            return newEdge;
        }

        public bool IsEdgeExist(TKeyNode from, TKeyNode to)
        {
            NodeV<TKeyNode, TNodePayload> fromNode = null;
            if (NodeIndexes.ContainsKey(from)) { fromNode = Nodes[NodeIndexes[from]]; }

            if (fromNode == null)
            {
                return false;
            }

            return fromNode.Edges.ContainsKey(to);
        }

        public void RemoveEdge(TKeyNode from, TKeyNode to)
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

        public int GetNodeIndex(TKeyNode id)
        {
            //return NodeList.FindIndex(elem => elem.Id.Equals(id));

            if (NodeIndexes.ContainsKey(id))
                return NodeIndexes[id];
            return -1;
        }

        public NodeV<TKeyNode, TNodePayload> GetNode(TKeyNode nodeKey)
        {
            if (!NodeIndexes.ContainsKey(nodeKey))
            {
                return null;
            }

            return Nodes[NodeIndexes[nodeKey]];
        }
    }
}
