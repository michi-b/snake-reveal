using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Game.Enums;
using UnityEngine;
using Utility;

namespace Game.Lines
{
    [Serializable, SuppressMessage("ReSharper", "InconsistentNaming"), DebuggerDisplay("{DebuggerDisplay,nq}")]
    // this is an unmanaged data struct separate from the line component ot allow for more efficient processing in some operations
    public struct LineData
    {
        public Vector2Int Start;
        public Vector2Int End;
        [SerializeField] private GridDirection _direction;

        public LineData(Vector2Int start, Vector2Int end)
            : this(start, end, start.GetDirection(end))
        {
        }

        private LineData(Vector2Int start, Vector2Int end, GridDirection direction)
        {
            Start = start;
            End = end;
            _direction = direction;
        }

        public string DebuggerDisplay => $"{Start} -> {End}({Direction})";

        public GridDirection Direction => _direction;

        public LineData WithStart(Vector2Int start)
        {
            return new LineData(start, End, start.GetDirection(End));
        }

        public LineData WithEnd(Vector2Int end)
        {
            return new LineData(Start, end, Start.GetDirection(end));
        }

        public void ReevaluateDirection()
        {
            _direction = Start.GetDirection(End);
        }

        public LineData Reverse()
        {
            return new LineData(End, Start, _direction.Reverse());
        }
    }
}