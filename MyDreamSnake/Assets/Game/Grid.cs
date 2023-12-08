using Extensions;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Serialization;

namespace Game
{
    public class Grid : MonoBehaviour
    {
        [SerializeField] private Vector2 _sceneSize = new(10.8f, 10.8f);
        [SerializeField] private int2 _size = new(1024, 1024);
        [SerializeField] private Vector2 _sceneCellSize;

        [FormerlySerializedAs("_lowerLeftScenePosition")] [SerializeField]
        private Vector2 _lowerLeftCornerScenePosition;

        [SerializeField] private int _gizmoCellSizeMultiplier = 32;
        [SerializeField] private Color _gizmoColor = new(0.7f, 0.7f, 0.7f, 0.5f);

        [SerializeField] private bool _drawGizmo = true;


        public Vector2 SceneCellSize => _sceneCellSize;

        public int GizmoCellSizeMultiplier => _gizmoCellSizeMultiplier;

        protected void OnDrawGizmos()
        {
            if (!_drawGizmo)
            {
                return;
            }

            var gizmoCellCount = _size / _gizmoCellSizeMultiplier;

            var oldColor = Gizmos.color;
            Gizmos.color = _gizmoColor;

            for (var gizmoLineIndex = 0; gizmoLineIndex <= gizmoCellCount.x; gizmoLineIndex++)
            {
                var x = gizmoLineIndex * _gizmoCellSizeMultiplier;
                var start = GetWorldPosition(new int2(x, 0));
                var end = GetWorldPosition(new int2(x, _size.y));
                Gizmos.DrawLine(start, end);
            }

            for (var gizmoLineIndex = 0; gizmoLineIndex <= gizmoCellCount.y; gizmoLineIndex++)
            {
                var y = gizmoLineIndex * _gizmoCellSizeMultiplier;
                var start = GetWorldPosition(new int2(0, y));
                var end = GetWorldPosition(new int2(_size.x, y));
                Gizmos.DrawLine(start, end);
            }

            Gizmos.color = oldColor;
        }

        protected void OnValidate()
        {
            _lowerLeftCornerScenePosition = -_sceneSize * 0.5f;
            _sceneCellSize = _sceneSize / _size.ToVector2();
        }

        public Vector2 GetScenePosition(int2 gridPosition)
        {
            return _lowerLeftCornerScenePosition + SceneCellSize * gridPosition.ToVector2();
        }

        public Vector3 GetWorldPosition(int2 gridPosition)
        {
            return GetWorldPosition(GetScenePosition(gridPosition));
        }

        public Vector3 GetWorldPosition(int2 gridPosition, float z)
        {
            return GetScenePosition(gridPosition).ToVector3(z);
        }

        private Vector3 GetWorldPosition(Vector2 scenePosition)
        {
            return scenePosition.ToVector3(transform.position.z);
        }

        public void Place(Transform targetTransform, int2 gridPosition)
        {
            targetTransform.position = GetWorldPosition(gridPosition, targetTransform.position.z);
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