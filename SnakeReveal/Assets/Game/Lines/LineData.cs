using System;
using System.Diagnostics.CodeAnalysis;
using Game.Enums;
using UnityEngine;
using Utility;

namespace Game.Lines
{
    [Serializable, SuppressMessage("ReSharper", "InconsistentNaming")]
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

        public LineData(Vector2Int start, Vector2Int end, GridDirection direction)
        {
            Start = start;
            End = end;
            _direction = direction;
        }

        public override string ToString()
        {
            return $"{Start} -> {End}({Direction})";
        }

        public GridDirection Direction
        {
            get => _direction;
            set => _direction = value;
        }

        public LineData Reverse()
        {
            return new LineData(End, Start, _direction.Reverse());
        }
    }
}