using System;
using Game.Enums;
using UnityEngine;
using UnityEngine.Serialization;
using Utility;

namespace Game.Lines
{
    /// <summary>
    ///     minimal immutable struct to cache information of line container lines
    /// </summary>
    [Serializable]
    public struct Line
    {
        [SerializeField] private Vector2Int _start;
        [SerializeField] private Vector2Int _end;
        [SerializeField] private GridDirection _direction;
        [SerializeField] private bool _isOpenChainEnd;

        public const string StartPropertyName = nameof(_start);
        
        public const string EndPropertyName = nameof(_end);
        
        public const string DirectionPropertyName = nameof(_direction);
        
        public const string IsChainEndPropertyName = nameof(_isOpenChainEnd);

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
            return new Line(_start, _end, isOpenChainEnd);
        }
    }
}