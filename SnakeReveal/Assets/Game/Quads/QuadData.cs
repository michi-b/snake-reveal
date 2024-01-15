using System;
using UnityEngine;

namespace Game.Quads
{
    [Serializable]
    public struct QuadData
    {
        [SerializeField] private Vector2Int _bottomLeftCorner;
        [SerializeField] private Vector2Int _size;

        public QuadData(Vector2Int position)
        {
            _bottomLeftCorner = position;
            _size = Vector2Int.one;
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
    }
}