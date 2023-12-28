using Extensions;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Serialization;

namespace Game
{
    public class SimulationGrid : MonoBehaviour
    {
        [SerializeField] private Vector2 _sceneSize = new(10.8f, 10.8f);
        [SerializeField] private Vector2Int _size = new(1024, 1024);
        [SerializeField] private Vector2 _sceneCellSize;

        [FormerlySerializedAs("_lowerLeftScenePosition")] [SerializeField]
        private Vector2 _lowerLeftCornerScenePosition;

        [SerializeField] private int _gizmoCellSizeMultiplier = 32;

        [SerializeField] private Color _gizmoColor = new(0.7f, 0.7f, 0.7f, 0.5f);

        [SerializeField] private bool _drawGizmo = true;

        public Vector2Int Size => _size;

        protected void OnDrawGizmos()
        {
            if (!_drawGizmo)
            {
                return;
            }

            Vector2Int gizmoCellCount = _size / _gizmoCellSizeMultiplier;

            Color oldColor = Gizmos.color;
            Gizmos.color = _gizmoColor;

            for (int x = 0; x <= gizmoCellCount.x; x++)
            {
                Vector3 bottom = GetWorldPosition(new Vector2Int(x, 0));
                Vector3 top = GetWorldPosition(new Vector2Int(x, gizmoCellCount.y));
                Gizmos.DrawLine(bottom, top);
            }
            
            for (int y = 0; y <= gizmoCellCount.y; y++)
            {
                Vector3 left = GetWorldPosition(new Vector2Int(0, y));
                Vector3 right = GetWorldPosition(new Vector2Int(gizmoCellCount.x, y));
                Gizmos.DrawLine(left, right);
            }

            Gizmos.color = oldColor;
        }

        protected void OnValidate()
        {
            _sceneCellSize = _sceneSize / _size;
            _lowerLeftCornerScenePosition = -_sceneSize * 0.5f;
        }

        public Vector2 GetScenePosition(Vector2Int gridPosition)
        {
            return _lowerLeftCornerScenePosition + _sceneCellSize * gridPosition;
        }

        private Vector3 GetWorldPosition(Vector2Int gridPosition)
        {
            return GetWorldPosition(GetScenePosition(gridPosition));
        }

        private Vector3 GetWorldPosition(Vector2 scenePosition)
        {
            return scenePosition.ToVector3(transform.position.z);
        }

        public Vector2Int Clamp(Vector2Int gridPosition)
        {
            return new Vector2Int(
                math.clamp(gridPosition.x, 0, _size.x),
                math.clamp(gridPosition.y, 0, _size.y)
            );
        }

        public Vector2Int RoundToGrid(Vector2 scenePosition)
        {
            Vector2 gridPosition = scenePosition - _lowerLeftCornerScenePosition;
            gridPosition /= _sceneCellSize;
            return Vector2Int.RoundToInt(gridPosition);
        }
    }
}