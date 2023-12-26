using System;
using System.Collections.Generic;
using System.Diagnostics;
using Extensions;
using Game.Enums;
using Unity.Mathematics;
using UnityEngine;

namespace Game.Lines
{
    public class LineChain : MonoBehaviour
    {
        [SerializeField] private SimulationGrid _grid;

        [SerializeField] private bool _loop;

        [SerializeField] [HideInInspector] private List<Corner> _nodes = new();

        public SimulationGrid Grid => _grid;

        public int Count => _nodes.Count;

        public bool Loop => _loop;

        [DebuggerDisplay("Debug: {Items[{index}]}")]
        public Corner this[int index]
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
                ? _nodes[index].Position.ToString()
                : $"{_nodes[index].Position} -> {this[index + 1].Position}");
        }

        public void Append()
        {
            _nodes.Add(new Corner());
        }

        public void RemoveLast()
        {
            _nodes.RemoveAt(_nodes.Count - 1);
        }

        public void ReevaluateDirection(int i)
        {
            Corner current = _nodes[i];
            current.Direction = TryGetCornerAfter(i, out Corner next)
                ? current.Position.GetDirection(next.Position)
                : GridDirection.None;
            _nodes[i] = current;
        }

        private bool TryGetCornerAfter(int index, out Corner corner)
        {
            int count = Count;
            int nextIndex = _loop ? (index + 1) % count : index + 1;

            if (nextIndex < 0 || nextIndex > count - 1)
            {
                corner = new Corner();
                return false;
            }

            corner = _nodes[nextIndex];
            return true;
        }
        
        /// <summary>
        /// minimal struct to cache information in line points of a line container
        /// </summary>
        [Serializable]
        public struct Corner
        {
            public int2 Position;
            public GridDirection Direction;
        }
    }
}