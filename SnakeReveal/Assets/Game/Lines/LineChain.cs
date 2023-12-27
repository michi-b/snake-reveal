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

        [SerializeField] private bool _loop;

        [SerializeField] private LineChainRenderer[] _renderers = Array.Empty<LineChainRenderer>();

        [SerializeField] [HideInInspector] private List<Corner> _corners = new();

        [SerializeField] [HideInInspector] private Turn _turn = Turn.None;

        private readonly List<Vector2> _renderPointsBuffer = new(LineChainRenderer.InitialLineCapacity);
        
        public const string CornersPropertyName = nameof(_corners);

        public SimulationGrid Grid => _grid;

        public int Count => _corners.Count;

        public bool Loop => _loop;

        public Turn Turn => _turn;

        public Corner this[int index]
        {
            get => _corners[index];
            set => _corners[index] = value;
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

        public void EvaluateDirection(int i)
        {
            Corner current = _corners[i];
            current.Direction = TryGetCornerAfter(i, out Corner next)
                ? current.Position.GetDirection(next.Position)
                : GridDirection.None;
            _corners[i] = current;
        }

        public void EvaluateTurn()
        {
            GridDirection lastDirection = _loop ? _corners[^1].Direction : GridDirection.None;

            // sum of clockwise turns - sum of counter clockwise turns
            int clockwiseWeight = 0;

            // ReSharper disable once ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator
            // not using linq to avoid allocation
            foreach (Corner corner in _corners)
            {
                if (corner.Direction == GridDirection.None)
                {
                    continue;
                }

                if (lastDirection != GridDirection.None)
                {
                    Turn turn = lastDirection.GetTurn(corner.Direction);
                    clockwiseWeight += turn.GetClockwiseWeight();
                }

                lastDirection = corner.Direction;
            }

            _turn = clockwiseWeight > 0
                ? Turn.Right
                : clockwiseWeight < 0
                    ? Turn.Left
                    : Turn.None;

#if UNITY_EDITOR
            Debug.Log($"{nameof(LineChain)}.{nameof(EvaluateTurn)} yielded a clockwise weight of " +
                      $"\"{clockwiseWeight}\", which is a \"{Turn}\" turn");
#endif
        }

        public void Append(int2 position)
        {
            _corners.Add(new Corner(position, GridDirection.None));

            //evaluate direction of new corner and the corner before it
            if (Count > 1)
            {
                EvaluateDirection(Count - 2);
            }

            EvaluateDirection(Count - 1);
        }

        public void RemoveLast()
        {
            _corners.RemoveAt(_corners.Count - 1);
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
            if (_loop && _corners.Count > 0)
            {
                AddRendererPoint(_corners[0]);
            }
        }

        private void AddRendererPoint(Corner corner)
        {
            Vector2 position = Grid.GetScenePosition(corner.Position);
            _renderPointsBuffer.Add(position);
        }

        /// <summary>
        ///     minimal struct to cache information in line points of a line container
        /// </summary>
        [Serializable]
        public struct Corner
        {
            [SerializeField] private int2 _position;
            [SerializeField] private GridDirection _direction;

            public Corner(int2 position, GridDirection direction)
            {
                _position = position;
                _direction = direction;
            }

            public GridDirection Direction
            {
                get => _direction;
                set => _direction = value;
            }

            public int2 Position
            {
                get => _position;
                set => _position = value;
            }
        }
    }
}