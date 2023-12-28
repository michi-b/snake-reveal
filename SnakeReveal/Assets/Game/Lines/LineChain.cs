using System;
using System.Collections.Generic;
using Game.Enums;
using UnityEngine;
using UnityEngine.Serialization;

namespace Game.Lines
{
    public class LineChain : MonoBehaviour
    {
        public const string LinesPropertyName = nameof(_lines);
        public const string LoopPropertyName = nameof(_loop);
        
        [SerializeField] private SimulationGrid _grid;

        [SerializeField] private LineChainRenderer[] _renderers = Array.Empty<LineChainRenderer>();

        [FormerlySerializedAs("_corners")] [SerializeField] [HideInInspector]
        private List<Line> _lines = new();

        [SerializeField] [HideInInspector] private Turn _turn = Turn.None;

        [SerializeField] [HideInInspector] private bool _loop;

        private readonly List<Vector2> _renderPointsBuffer = new(LineChainRenderer.InitialLineCapacity);

        public SimulationGrid Grid => _grid;

        public int Count => _lines.Count;

        public bool Loop => _loop;

        public Turn Turn => _turn;

        public Line this[int index]
        {
            get => _lines[index];
            set => _lines[index] = value;
        }

        private bool TryGetNextIndex(int index, out int nextIndex)
        {
            if (Loop)
            {
                nextIndex = (index + 1) % Count;
                return true;
            }

            nextIndex = index + 1;
            return nextIndex <= Count - 1;
        }

        public void EvaluateTurn()
        {
            GridDirection lastDirection = _loop ? _lines[^1].Direction : GridDirection.None;

            // sum of clockwise turns - sum of counter clockwise turns
            int clockwiseWeight = 0;

            // ReSharper disable once ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator
            // not using linq to avoid allocation
            foreach (Line corner in _lines)
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

        public void Append(Vector2Int position)
        {
            if (_lines.Count == 0)
            {
                _lines.Add(new Line(position, position, !_loop));
            }

            if (Loop)
            {
                _lines.Add(new Line(position, _lines[0].Start));
                _lines[^2] = _lines[^2].WithEnd(position);
            }

            _lines[^1] = _lines[^1].AsOpenChainEnd(false);
            _lines.Add(new Line(_lines[^1].End, position, true));
        }

        public void RemoveLast()
        {
            _lines.RemoveAt(_lines.Count - 1);
            if (_lines.Count > 0)
            {
                _lines[^1] = _lines[^1].WithEnd((Loop ? _lines[0] : _lines[^1]).Start);
            }
        }

        public void UpdateRenderersInEditMode()
        {
            _renderPointsBuffer.Clear();

            if (Count > 0)
            {
                foreach (Line corner in _lines)
                {
                    _renderPointsBuffer.Add(Grid.GetScenePosition(corner.Start));
                }

                //special handling of loop / open chain end position
                Vector2Int finalPosition = Loop ? _lines[0].Start : _lines[^1].End;
                _renderPointsBuffer.Add(Grid.GetScenePosition(finalPosition));
            }

            foreach (LineChainRenderer lineChainRenderer in _renderers)
            {
                lineChainRenderer.SetInEditMode(_renderPointsBuffer);
            }
        }

        public void ReevaluateLinesFromStartPositions()
        {
            if (Count == 0)
            {
                return;
            }

            for (int i = 0; i < Count - 1; i++)
            {
                _lines[i] = _lines[i].WithEnd(_lines[i + 1].Start);
            }

            // special handling of last line based on loop
            if (Loop)
            {
                _lines[^1] = _lines[^1].WithEnd(_lines[0].Start);
            }
            else
            {
                _lines[^1] = _lines[^1].AsOpenChainEnd(true);
            }
        }
    }
}