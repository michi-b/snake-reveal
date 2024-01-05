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
        {
            Start = start;
            End = end;
            _direction = Start.GetDirection(End);
        }

        public GridDirection Direction => _direction;

        public void ReevaluateDirection()
        {
            _direction = Start.GetDirection(End);
        }
    }
}