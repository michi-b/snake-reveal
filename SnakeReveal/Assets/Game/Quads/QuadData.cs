using System;
using System.Diagnostics;
using UnityEngine;

namespace Game.Quads
{
    [Serializable, DebuggerDisplay("{ToString(),nq}")]
    public struct QuadData
    {
        [SerializeField] private Vector2Int _bottomLeftCorner;
        [SerializeField] private Vector2Int _size;

        public QuadData(Vector2Int position)
        {
            _bottomLeftCorner = position;
            _size = Vector2Int.one;
        }

        public QuadData(int startX, int endX, int startY, int endY)
        {
            _bottomLeftCorner = new Vector2Int(startX, startY);
            _size = new Vector2Int(endX - startX, endY - startY);
        }

        public Vector2Int TopRight
        {
            set
            {
                Vector2Int size = value - _bottomLeftCorner;
                _size = size;
            }
            get => _bottomLeftCorner + _size;
        }

        public Vector2Int BottomLeft
        {
            set
            {
                Vector2Int delta = value - _bottomLeftCorner;
                _size -= delta;
                _bottomLeftCorner = value;
            }
            get => _bottomLeftCorner;
        }

        public Vector2Int Size
        {
            set => _size = value;
            get => _size;
        }

        public void Move(Vector2Int delta)
        {
            _bottomLeftCorner += delta;
        }

        public override string ToString()
        {
            return $"{BottomLeft} -> {TopRight}";
        }

        public int GetCellCount()
        {
            return _size.x * _size.y;
        }
    }
}