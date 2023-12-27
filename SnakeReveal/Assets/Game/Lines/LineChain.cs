using System;
using System.Collections.Generic;
using Extensions;
using Game.Enums;
using Unity.Mathematics;
using UnityEngine;

namespace Game.Lines
{
    public class LineChain : MonoBehaviour
    {
        [SerializeField] private SimulationGrid _grid;

        [SerializeField] private LineChainRenderer[] _renderers = Array.Empty<LineChainRenderer>();

        [SerializeField] private bool _loop;

        [SerializeField] [HideInInspector] private List<Corner> _corners = new();

        public SimulationGrid Grid => _grid;

        public int Count => _corners.Count;

        public bool Loop => _loop;

        public Corner this[int index]
        {
            get => _corners[index];
            set => _corners[index] = value;
        }

        public void Append()
        {
            _corners.Add(new Corner());
        }

        public void RemoveLast()
        {
            _corners.RemoveAt(_corners.Count - 1);
        }

        public void ReevaluateDirection(int i)
        {
            Corner current = _corners[i];
            current._direction = TryGetCornerAfter(i, out Corner next)
                ? current._position.GetDirection(next._position)
                : GridDirection.None;
            _corners[i] = current;
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

            corner = _corners[nextIndex];
            return true;
        }

        /// <summary>
        ///     minimal struct to cache information in line points of a line container
        /// </summary>
        [Serializable]
        public struct Corner
        {
            public int2 _position;
            public GridDirection _direction;

            public Corner(int2 position, GridDirection direction)
            {
                _position = position;
                _direction = direction;
            }
        }

        public void UpdateRenderersInEditMode()
        {
            UpdateRendererPoints();
            foreach (LineChainRenderer lineChainRenderer in _renderers)
            {
                lineChainRenderer.SetInEditMode(_renderPointsBuffer);
            }
        }

        private void UpdateRendererPoints()
        {
            _renderPointsBuffer.Clear();
            foreach (Corner corner in _corners)
            {
                AddRendererPoint(corner);
            }
            
            //add first point again to connect end to start if the chain is set to loop
            if(_loop && _corners.Count > 0)
            {
                AddRendererPoint(_corners[0]);
            }
        }

        private void AddRendererPoint(Corner corner)
        {
            Vector3 position = Grid.GetScenePosition(corner._position).ToVector3(0f);
            _renderPointsBuffer.Add(position);
        }

        private List<Vector3> _renderPointsBuffer = new List<Vector3>(LineChainRenderer.InitialLineCapacity);
    }
}