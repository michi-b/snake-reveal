using System;
using System.Collections.Generic;
using System.Linq;
using Extensions;
using Game.Enums;
using Game.Lines.Multi;
using Unity.Mathematics;
using Unity.VisualScripting.FullSerializer.Internal.Converters;
using UnityEngine;
using UnityEngine.Serialization;

namespace Game.Lines
{
    public class LineChain : MonoBehaviour
    {
        [SerializeField] private SimulationGrid _grid;

        [SerializeField] private bool _loop;

        [SerializeField] private LineChainRenderer[] _renderers = Array.Empty<LineChainRenderer>();

        [SerializeField] private List<int2> _corners = new();

        [SerializeField] [HideInInspector] private Turn _turn = Turn.None;

        private readonly List<Vector2> _renderPointsBuffer = new(LineChainRenderer.InitialLineCapacity);
        
        public const string CornersPropertyName = nameof(_corners);

        public SimulationGrid Grid => _grid;

        public int Count => _corners.Count;

        public bool Loop => _loop;

        public Turn Turn => _turn;

        public int2 this[int index]
        {
            get => _corners[index];
            set => _corners[index] = value;
        }

        private bool TryGetCornerAfter(int index, out int2 corner)
        {
            int count = Count;
            int nextIndex = _loop ? (index + 1) % count : index + 1;

            if (nextIndex < 0 || nextIndex > count - 1)
            {
                corner = default;
                return false;
            }

            corner = _corners[nextIndex];
            return true;
        }

        private GridDirection GetDirectionFrom(int index)
        {
            if(index < 0 || index > Count - 1)
            {
                return GridDirection.None;
            }

            int nextIndex = index + 1;
            
            if (nextIndex > Count - 1)
            {
                return _loop
                    ? _corners[index].GetDirection(nextIndex)
                    : GridDirection.None;
            }

            return _corners[index].GetDirection(nextIndex);
        }
        
        public void EvaluateTurn()
        {
            GridDirection lastDirection = GetDirectionFrom(Count - 1);

            // sum of clockwise turns - sum of counter clockwise turns
            int clockwiseWeight = 0;

            // ReSharper disable once ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator
            // not using linq to avoid allocation
            for (int i = 0; i < _corners.Count - 1; i++)
            {
                int2 corner = _corners[i];
                GridDirection direction = GetDirectionFrom(i);
                if (direction == GridDirection.None)
                {
                    continue;
                }

                if (lastDirection != GridDirection.None)
                {
                    Turn turn = lastDirection.GetTurn(direction);
                    clockwiseWeight += turn.GetClockwiseWeight();
                }

                lastDirection = direction;
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
            _corners.Add(position);
        }

        public void RemoveLast()
        {
            _corners.RemoveAt(_corners.Count - 1);
        }

        public void UpdateRenderersInEditMode()
        {
            foreach (LineChainRenderer lineChainRenderer in _renderers)
            {
                _renderPointsBuffer.Clear();
                foreach (int2 corner in _corners)
                {
                    _renderPointsBuffer.Add(Grid.GetScenePosition(corner));
                }
                lineChainRenderer.SetInEditMode(_renderPointsBuffer, _loop);
            }
        }
    }
}