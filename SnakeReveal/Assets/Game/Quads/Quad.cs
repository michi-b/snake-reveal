using Extensions;
using Game.Grid;
using UnityEngine;
using Utility;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Game.Quads
{
    public class Quad : MonoBehaviour
    {
        [SerializeField] private SimulationGrid _grid;
        [SerializeField] private BoxCollider2D _collider;
        [SerializeField] private QuadData _data;

        public SimulationGrid Grid => _grid;

        public Vector2Int TopRight
        {
            set
            {
                if(value != TopRight)
                {
                    _data.TopRight = value;
                    Apply();
                }
            }
            get => _data.TopRight;
        }

        public Vector2Int BottomLeft
        {
            set
            {
                if(value != BottomLeft)
                {
                    _data.BottomLeft = value;
                    Apply();
                }
            }
            get => _data.BottomLeft;
        }

        public Vector2Int Size
        {
            set
            {
                if(value != Size)
                {
                    _data.Size = value;
                    Apply();
                }
            }
            get => _data.Size;
        }

        protected virtual void Reset()
        {
            _grid = SimulationGrid.EditModeFind()!;
            _collider = gameObject.GetComponent<BoxCollider2D>();
            _data = new QuadData(_grid.Round(transform.position));
            Apply();
        }

        public void Initialize(SimulationGrid grid, QuadData quadData)
        {
            _grid = grid;
            _data = quadData;
            Apply();
        }

        public void Apply()
        {
            transform.position = BottomLeft.GetScenePosition(_grid).ToVector3(transform.position.z);
            Vector2 sceneSize = Size * Grid.SceneCellSize;
            _collider.offset = sceneSize * 0.5f;
            _collider.size = sceneSize;
        }

        public void Move(Vector2Int delta)
        {
            _data.Move(delta);
            Apply();
        }

#if UNITY_EDITOR
        protected void OnDrawGizmosSelected()
        {
            if (!Selection.Contains(gameObject) || _grid == null)
            {
                return;
            }
            
            GizmosUtility.DrawRect(_grid.GetScenePosition(BottomLeft),
                _grid.GetScenePosition(BottomLeft + Size),
                transform.position.z,
                Color.cyan);
        }
#endif
    }
}