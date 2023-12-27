using Extensions;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Serialization;

namespace Game
{
    public class SimulationGrid : MonoBehaviour
    {
        [SerializeField] private Vector2 _sceneSize = new(10.8f, 10.8f);
        [SerializeField] private int2 _size = new(1024, 1024);
        [SerializeField] private Vector2 _sceneCellSize;

        [FormerlySerializedAs("_lowerLeftScenePosition")] [SerializeField]
        private Vector2 _lowerLeftCornerScenePosition;

        [SerializeField, Range(1, 10)] private int _gizmoCellSizeMultiplier = 1;

        [SerializeField] private Color _gizmoColor = new(0.7f, 0.7f, 0.7f, 0.5f);

        [SerializeField] private bool _drawGizmo = true;

        public int2 Size => _size;

        protected void OnDrawGizmos()
        {
            if (!_drawGizmo)
            {
                return;
            }

            int2 gizmoCellCount = _size / _gizmoCellSizeMultiplier;

            Color oldColor = Gizmos.color;
            Gizmos.color = _gizmoColor;

            for (int x = 0; x <= gizmoCellCount.x; x++)
            {
                Vector3 bottom = GetWorldPosition(new int2(x, 0) * _gizmoCellSizeMultiplier);
                Vector3 top = GetWorldPosition(new int2(x, gizmoCellCount.y) * _gizmoCellSizeMultiplier);
                Gizmos.DrawLine(bottom, top);
            }
            
            for (int y = 0; y <= gizmoCellCount.y; y++)
            {
                Vector3 left = GetWorldPosition(new int2(0, y) * _gizmoCellSizeMultiplier);
                Vector3 right = GetWorldPosition(new int2(gizmoCellCount.x, y) * _gizmoCellSizeMultiplier);
                Gizmos.DrawLine(left, right);
            }

            Gizmos.color = oldColor;
        }

        protected void OnValidate()
        {
            _sceneCellSize = _sceneSize / _size.ToVector2();
            _lowerLeftCornerScenePosition = -_sceneSize * 0.5f;
        }

        public Vector2 GetScenePosition(int2 gridPosition)
        {
            return _lowerLeftCornerScenePosition + _sceneCellSize * gridPosition.ToVector2();
        }

        private Vector3 GetWorldPosition(int2 gridPosition)
        {
            return GetWorldPosition(GetScenePosition(gridPosition));
        }

        private Vector3 GetWorldPosition(Vector2 scenePosition)
        {
            return scenePosition.ToVector3(transform.position.z);
        }

        public int2 Clamp(int2 gridPosition)
        {
            return new int2(
                math.clamp(gridPosition.x, 0, _size.x),
                math.clamp(gridPosition.y, 0, _size.y)
            );
        }
    }
}