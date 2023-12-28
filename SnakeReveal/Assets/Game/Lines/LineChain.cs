using System;
using System.Collections.Generic;
using Extensions;
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

        [FormerlySerializedAs("_renderers")] [SerializeField]
        private LineChainRenderer[] _lineRenderers = Array.Empty<LineChainRenderer>();

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

        public Vector3 GetWorldPosition(Vector2Int position)
        {
            return Grid.GetScenePosition(position).ToVector3(transform.position.z);
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
                return;
            }

            if (Loop)
            {
                _lines.Add(new Line(position, _lines[0].Start));
                _lines[^1] = _lines[^1].WithEnd(position);
                return;
            }
            else
            {
                _lines[^1] = _lines[^1].AsOpenChainEnd(false);   
            }

            _lines.Add(new Line(_lines[^1].End, position, true));
        }

        public void RemoveLast()
        {
            Vector2Int lastStart = _lines[^1].Start;
            _lines.RemoveAt(_lines.Count - 1);
            if (_lines.Count > 0)
            {
                _lines[^1] = _lines[^1].WithEnd(Loop ? _lines[0].Start : lastStart);
            }
        }

        public void EditModeUpdateLineRenderers()
        {
            _renderPointsBuffer.Clear();

            if (Count > 0)
            {
                foreach (Line corner in _lines)
                {
                    _renderPointsBuffer.Add(Grid.GetScenePosition(corner.Start));
                }

                //special handling of open chain end position
                if (!_loop)
                {
                    _renderPointsBuffer.Add(Grid.GetScenePosition(_lines[^1].End));
                }
            }

            foreach (LineChainRenderer lineChainRenderer in _lineRenderers)
            {
                lineChainRenderer.EditModeRebuild(_renderPointsBuffer, Loop);
            }
        }

        public void EditModeApplyLines()
        {
            if (Count == 0)
            {
                return;
            }

            for (int i = 0; i < _lines.Count; i++)
            {
                _lines[i] = _lines[i].Clamp(Grid);
            }
            
            for (int i = 0; i < Count - 1; i++)
            {
                _lines[i] = _lines[i].WithEnd(_lines[i + 1].Start).AsOpenChainEnd(false);
            }

            // special handling of last line based on loop
            if (Loop)
            {
                _lines[^1] = _lines[^1].WithEnd(_lines[0].Start).AsOpenChainEnd(false);
            }
            else
            {
                _lines[^1] = _lines[^1].AsOpenChainEnd(true);
            }

        }
    }
}