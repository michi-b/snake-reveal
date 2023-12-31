using System;
using System.Runtime.CompilerServices;
using Game.Enums;
using Game.Grid;
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

        [SerializeField] private Vector2Int _start;
        [SerializeField] private Vector2Int _end;
        [SerializeField] private GridDirection _direction;

        public Line(Vector2Int start, Vector2Int end)
        {
            _start = start;
            _end = end;
            _direction = _start.GetDirection(_end);
        }

        public GridDirection Direction => _direction;

        public Vector2Int Start => _start;

        public Vector2Int End => _end;

        public AxisOrientation Orientation => _direction.GetOrientation();

        public bool Equals(Line other)
        {
            return _start.Equals(other._start)
                   && _end.Equals(other._end)
                   && _direction == other._direction;
        }

        public Line WithEnd(Vector2Int end)
        {
            return new Line(_start, end);
        }

        public Line WithStart(Vector2Int start)
        {
            return new Line(start, _end);
        }

        public Line Move(Vector2Int move)
        {
            return new Line(_start + move, _end + move);
        }

        public Line AsOpenChainEnd(bool isOpenChainEnd)
        {
            return new Line(_start, _end);
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
            return HashCode.Combine(_start, _end, (int)_direction);
        }

        public Line Clamp(SimulationGrid grid)
        {
            return new Line(grid.Clamp(_start), grid.Clamp(_end));
        }

        public bool Contains(Vector2Int position)
        {
            return _direction.GetOrientation() switch
            {
                AxisOrientation.Horizontal => ContainsHorizontalNoAssert(position),
                AxisOrientation.Vertical => ContainsVerticallyNoAssert(position),
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        public static bool ContainsHorizontal(Line line, Vector2Int position)
        {
#if DEBUG
            Debug.Assert(line.Orientation == AxisOrientation.Horizontal);
#endif
            return line.ContainsHorizontalNoAssert(position);
        }

        public static bool ContainsVertical(Line line, Vector2Int position)
        {
#if DEBUG
            Debug.Assert(line.Orientation == AxisOrientation.Vertical);
#endif
            return line.ContainsVerticallyNoAssert(position);
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool ContainsHorizontalNoAssert(Vector2Int position)
        {
            return _start.y == position.y
                   && _start.x < position.x != _end.x < position.x;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool ContainsVerticallyNoAssert(Vector2Int position)
        {
            return _start.x == position.x
                   && _start.y < position.y != _end.y < position.y;
        }

        public Line Invert()
        {
            return new Line(_end, _start);
        }
    }
}