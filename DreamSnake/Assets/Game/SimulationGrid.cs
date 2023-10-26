using Game.Extensions;
using Unity.Mathematics;
using UnityEngine;

namespace Game
{
    public class SimulationGrid : MonoBehaviour
    {
        [SerializeField] private Vector2 _sceneSize = new Vector2(10.8f, 10.8f);
        [SerializeField] private int2 _size = new int2(1024, 1024);
        [SerializeField] private int _gizmoCellSize = 32;
        [SerializeField] private Color _gizmoColor = new Color(0.7f, 0.7f, 0.7f, 0.5f);

        protected void OnDrawGizmos()
        {
            int2 gizmoCellCount = _size / _gizmoCellSize;

            Color oldColor = Gizmos.color;
            Gizmos.color = _gizmoColor;

            for (int gizmoLineIndex = 0; gizmoLineIndex <= gizmoCellCount.x; gizmoLineIndex++)
            {
                int x = gizmoLineIndex * _gizmoCellSize;
                Vector3 start = GetWorldPosition(new int2(x, 0));
                Vector3 end = GetWorldPosition(new int2(x, _size.y));
                Gizmos.DrawLine(start, end);
            }

            for (int gizmoLineIndex = 0; gizmoLineIndex <= gizmoCellCount.y; gizmoLineIndex++)
            {
                int y = gizmoLineIndex * _gizmoCellSize;
                Vector3 start = GetWorldPosition(new int2(0, y));
                Vector3 end = GetWorldPosition(new int2(_size.x, y));
                Gizmos.DrawLine(start, end);
            }

            Gizmos.color = oldColor;
        }

        public Vector2 GetScenePosition(int2 gridPosition)
        {
            Vector2 lowerLeftCornerPosition = -_sceneSize * 0.5f;
            Vector2 cellSize = _sceneSize / _size.ToVector2();
            return lowerLeftCornerPosition + cellSize * gridPosition.ToVector2();
        }

        public Vector3 GetWorldPosition(int2 gridPosition) => GetScenePosition(GetScenePosition(gridPosition));

        private Vector3 GetScenePosition(Vector2 scenePosition) => scenePosition.ToVector3(transform.position.z);
    }
}