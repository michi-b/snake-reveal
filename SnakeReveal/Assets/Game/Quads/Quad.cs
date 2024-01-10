using Extensions;
using Game.Grid;
using UnityEngine;
using Utility;

namespace Game.Quads
{
    public class Quad : MonoBehaviour
    {
        [SerializeField] private SimulationGrid _grid;
        [SerializeField] private BoxCollider2D _collider;
        [SerializeField] private Vector2Int _bottomLeftCorner;
        [SerializeField] private Vector2Int _size = new(1, 1);

        public string BottomLeftCornerPropertyName => nameof(_bottomLeftCorner);
        public string SizePropertyName => nameof(_size);
        public SimulationGrid Grid => _grid;


        public Vector2Int TopRight
        {
            set
            {
                Vector2Int size = value - _bottomLeftCorner;
                if (size != _size)
                {
                    _size = size;
                    Apply();
                }
            }
            get => _bottomLeftCorner + _size;
        }

        public Vector2Int BottomLeft
        {
            set
            {
                if (value != _bottomLeftCorner)
                {
                    Vector2Int delta = value - _bottomLeftCorner;
                    _size -= delta;
                    _bottomLeftCorner = value;
                    Apply();
                }
            }
            get => _bottomLeftCorner;
        }

        protected virtual void Reset()
        {
            _grid = SimulationGrid.EditModeFind()!;
            _collider = gameObject.GetComponent<BoxCollider2D>();
            _bottomLeftCorner = _grid.Round(transform.position);
            _size = Vector2Int.one;
            Apply();
        }

        protected void OnDrawGizmos()
        {
            GizmosUtility.DrawRect(_grid.GetScenePosition(_bottomLeftCorner), 
                _grid.GetScenePosition(_bottomLeftCorner + _size), 
                transform.position.z,
                Color.cyan);
        }

        public void Apply()
        {
            transform.position = _bottomLeftCorner.GetScenePosition(_grid).ToVector3(transform.position.z);
            Vector2 sceneSize = _size * Grid.SceneCellSize;
            _collider.offset = sceneSize * 0.5f;
            _collider.size = sceneSize;
        }

        public void Move(Vector2Int delta)
        {
            _bottomLeftCorner += delta;
            Apply();
        }
    }
}