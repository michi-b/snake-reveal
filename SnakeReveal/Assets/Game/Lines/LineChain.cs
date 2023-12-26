using System.Collections.Generic;
using System.Diagnostics;
using Extensions;
using Game.Enums;
using UnityEngine;

namespace Game.Lines
{
    public class LineChain : MonoBehaviour
    {
        [SerializeField] private SimulationGrid _grid;

        [SerializeField] private bool _loop;

        [SerializeField] [HideInInspector] private List<LineNode> _nodes = new();

        public SimulationGrid Grid => _grid;

        public int Count => _nodes.Count;

        [DebuggerDisplay("Debug: {Items[{index}]}")]
        public LineNode this[int index]
        {
            get => _nodes[index];
            set => _nodes[index] = value;
        }

        public string GetLineDescription(int index)
        {
            if (index < 0 || index >= Count)
            {
                return "Index out of range";
            }

            return $"{index}: " + (index == Count - 1
                ? _nodes[index].ToString()
                : $"{_nodes[index]} -> {this[index + 1]}");
        }

        public void Append()
        {
            _nodes.Add(new LineNode());
        }

        public void RemoveLast()
        {
            _nodes.RemoveAt(_nodes.Count - 1);
        }

        public void ReevaluateDirection(int i)
        {
            LineNode current = _nodes[i];
            current.Direction = TryGetNodeAfter(i, out LineNode next)
                ? current.Position.GetDirection(next.Position)
                : GridDirection.None;
            _nodes[i] = current;
        }

        private bool TryGetNodeAfter(int index, out LineNode node)
        {
            int count = Count;
            int nextIndex = _loop ? (index + 1) % count : index + 1;

            if (nextIndex < 0 || nextIndex > count - 1)
            {
                node = new LineNode();
                return false;
            }

            node = _nodes[nextIndex];
            return true;
        }
    }
}