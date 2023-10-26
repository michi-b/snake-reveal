using Extensions;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Serialization;

namespace Game.Simulation.Grid
{
    public class SimulationGrid : MonoBehaviour
    {
        [SerializeField] private Vector2 _sceneSize = new(10.8f, 10.8f);
        [SerializeField] private int2 _size = new(1024, 1024);
        [SerializeField] private Vector2 _sceneCellSize;
        [FormerlySerializedAs("_lowerLeftScenePosition")] [SerializeField] private Vector2 _lowerLeftCornerScenePosition;

        [SerializeField] private int _gizmoCellSizeMultiplier = 32;
        [SerializeField] private Color _gizmoColor = new(0.7f, 0.7f, 0.7f, 0.5f);

        [SerializeField] private PlayerActor.PlayerActor _playerActor;

        public Vector2 SceneCellSize => _sceneCellSize;
        
        public int GizmoCellSizeMultiplier => _gizmoCellSizeMultiplier;

        protected void OnDrawGizmos()
        {
            int2 gizmoCellCount = _size / _gizmoCellSizeMultiplier;

            Color oldColor = Gizmos.color;
            Gizmos.color = _gizmoColor;

            for (int gizmoLineIndex = 0; gizmoLineIndex <= gizmoCellCount.x; gizmoLineIndex++)
            {
                int x = gizmoLineIndex * _gizmoCellSizeMultiplier;
                Vector3 start = GetWorldPosition(new int2(x, 0));
                Vector3 end = GetWorldPosition(new int2(x, _size.y));
                Gizmos.DrawLine(start, end);
            }

            for (int gizmoLineIndex = 0; gizmoLineIndex <= gizmoCellCount.y; gizmoLineIndex++)
            {
                int y = gizmoLineIndex * _gizmoCellSizeMultiplier;
                Vector3 start = GetWorldPosition(new int2(0, y));
                Vector3 end = GetWorldPosition(new int2(_size.x, y));
                Gizmos.DrawLine(start, end);
            }

            Gizmos.color = oldColor;
        }

        protected void OnValidate()
        {
            _lowerLeftCornerScenePosition = -_sceneSize * 0.5f;
            _sceneCellSize = _sceneSize / _size.ToVector2();
        }

        public Vector2 GetScenePosition(int2 gridPosition) => _lowerLeftCornerScenePosition + SceneCellSize * gridPosition.ToVector2();

        public Vector3 GetWorldPosition(int2 gridPosition) => GetScenePosition(GetScenePosition(gridPosition));

        private Vector3 GetScenePosition(Vector2 scenePosition) => scenePosition.ToVector3(transform.position.z);

        public void Place(Transform targetTransform, int2 gridPosition)
        {
            targetTransform.position = GetScenePosition(gridPosition).ToVector3(targetTransform.position.z);
        }

        public int2 Clamp(int2 gridPosition)
            => new
            (
                math.clamp(gridPosition.x, 0, _size.x),
                math.clamp(gridPosition.y, 0, _size.y)
            );
    }
}