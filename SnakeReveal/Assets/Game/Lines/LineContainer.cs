using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

namespace Game.Lines
{
    /// <summary>
    ///     collection of line points and abstract base class of line loop and line chain
    /// </summary>
    public abstract class LineContainer : MonoBehaviour
    {
        [SerializeField] private SimulationGrid _grid;

        /// <summary>
        ///     line cache that provides the container with instances at runtime
        /// </summary>
        [SerializeField] private LineRendererCache _cache;

        [SerializeField] [HideInInspector] private List<LineNode> _nodes = new();

        public LineRendererCache Cache => _cache;

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

            return index == Count - 1
                ? _nodes[index].ToString()
                : $"{_nodes[index]} -> {this[index + 1]}";
        }

        public void Append()
        {
            _nodes.Add(new LineNode());
        }

        public void RemoveLast()
        {
            _nodes.RemoveAt(_nodes.Count - 1);
        }
    }
}