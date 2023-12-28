using System;
using Game.Enums;
using UnityEngine;
using Utility;

namespace Game.Lines
{
    /// <summary>
    ///     minimal immutable struct to cache information of line container lines
    /// </summary>
    [Serializable]
    public struct Line : IEquatable<Line>
    {
        public const string StartPropertyName = nameof(_start);

        public const string EndPropertyName = nameof(_end);

        public const string DirectionPropertyName = nameof(_direction);

        public const string IsChainEndPropertyName = nameof(_isOpenChainEnd);
        [SerializeField] private Vector2Int _start;
        [SerializeField] private Vector2Int _end;
        [SerializeField] private GridDirection _direction;
        [SerializeField] private bool _isOpenChainEnd;

        public Line(Vector2Int start, Vector2Int end, bool isOpenChainEnd = false)
        {
            _start = start;
            _end = end;
            _direction = _start.GetDirection(_end);
            _isOpenChainEnd = isOpenChainEnd;
        }

        public GridDirection Direction => _direction;

        public Vector2Int Start => _start;

        public Vector2Int End => _end;

        public bool IsOpenChainEnd => _isOpenChainEnd;

        public bool Equals(Line other)
        {
            return _start.Equals(other._start)
                   && _end.Equals(other._end)
                   && _direction == other._direction
                   && _isOpenChainEnd == other._isOpenChainEnd;
        }

        public Line WithEnd(Vector2Int end)
        {
            return new Line(_start, end, _isOpenChainEnd);
        }

        public Line WithStart(Vector2Int start)
        {
            return new Line(start, _end, _isOpenChainEnd);
        }

        public Line Move(Vector2Int move)
        {
            return new Line(_start + move, _end + move, _isOpenChainEnd);
        }

        public Line AsOpenChainEnd(bool isOpenChainEnd)
        {
            return new Line(_start, _end, isOpenChainEnd);
        }

        public static bool operator ==(Line left, Line right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Line left, Line right)
        {
            return !(left == right);
        }

        public override bool Equals(object obj)
        {
            return obj is Line other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(_start, _end, (int)_direction, _isOpenChainEnd);
        }

        public Line Clamp(SimulationGrid grid)
        {
            return new Line(grid.Clamp(_start), grid.Clamp(_end), _isOpenChainEnd);
        }
    }
}