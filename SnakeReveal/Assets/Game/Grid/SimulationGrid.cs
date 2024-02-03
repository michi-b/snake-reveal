using Extensions;
using Game.Enums;
using Game.Enums.Extensions;
using JetBrains.Annotations;
using Unity.Mathematics;
using UnityEngine;

namespace Game.Grid
{
    public class SimulationGrid : MonoBehaviour
    {
        [SerializeField] private Vector2 _sceneSize = new(10.8f, 10.8f);
        [SerializeField] private Vector2Int _size = new(1024, 1024);
        [SerializeField] private Vector2 _sceneCellSize;
        [SerializeField] private Vector2 _lowerLeftCornerScenePosition;
        [SerializeField] private int _gizmoCellSizeMultiplier = 32;
        [SerializeField] private Color _gizmoColor = new(0.7f, 0.7f, 0.7f, 0.5f);

        public Vector2Int Size => _size;

        public Vector2 SceneCellSize => _sceneCellSize;
        public Vector2Int CenterPosition => _size / 2;
        private static Vector2Int BottomLeft => Vector2Int.zero;
        private Vector2Int TopLeft => new(0, _size.y);
        private Vector2Int TopRight => _size;
        private Vector2Int BottomRight => new(_size.x, 0);

        public int GetCellCount()
        {
            return _size.x * _size.y;
        }

        protected void OnDrawGizmos()
        {
            Vector2Int gizmoCellCount = _size / _gizmoCellSizeMultiplier;

            Color oldColor = Gizmos.color;
            Gizmos.color = _gizmoColor;

            int maxX = gizmoCellCount.x * _gizmoCellSizeMultiplier;
            int maxYIndex = gizmoCellCount.y * _gizmoCellSizeMultiplier;

            for (int x = 0; x <= gizmoCellCount.x; x++)
            {
                int xIndex = x * _gizmoCellSizeMultiplier;
                Vector3 bottom = GetWorldPosition(new Vector2Int(xIndex, 0));
                Vector3 top = GetWorldPosition(new Vector2Int(xIndex, maxYIndex));
                Gizmos.DrawLine(bottom, top);
            }

            for (int y = 0; y <= gizmoCellCount.y; y++)
            {
                int yIndex = y * _gizmoCellSizeMultiplier;
                Vector3 left = GetWorldPosition(new Vector2Int(0, yIndex));
                Vector3 right = GetWorldPosition(new Vector2Int(maxX, yIndex));
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
            return _lowerLeftCornerScenePosition + SceneCellSize * gridPosition;
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

        public Vector2Int Round(Vector2 scenePosition)
        {
            Vector2 gridPosition = scenePosition - _lowerLeftCornerScenePosition;
            gridPosition /= SceneCellSize;
            return Vector2Int.RoundToInt(gridPosition);
        }

        [CanBeNull]
        public static SimulationGrid EditModeFind()
        {
            SimulationGrid[] grids = FindObjectsByType<SimulationGrid>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            return grids is { Length: > 0 } ? grids[0] : null;
        }

        public Vector2 ToSceneVector(Vector2Int gridVector)
        {
            return _sceneCellSize * gridVector;
        }

        public bool GetIsInBounds(Vector2Int position)
        {
            return position.x >= 0 && position.x <= _size.x && position.y >= 0 && position.y <= _size.y;
        }

        public bool GetIsOnBounds(Vector2Int position)
        {
            return position.x == 0 || position.x == _size.x || position.y == 0 || position.y == _size.y;
        }

        public GridSide GetBoundsSide(Vector2Int position)
        {
            return position.x == 0
                ? GridSide.Left
                : position.x == _size.x
                    ? GridSide.Right
                    : position.y == 0
                        ? GridSide.Bottom
                        : position.y == _size.y
                            ? GridSide.Top
                            : GridSide.None;
        }

        public GridCorner GetBoundsCorner(Vector2Int position)
        {
            return position == BottomLeft
                ? GridCorner.BottomLeft
                : position == TopLeft
                    ? GridCorner.TopLeft
                    : position == TopRight
                        ? GridCorner.TopRight
                        : position == BottomRight
                            ? GridCorner.BottomRight
                            : GridCorner.None;
        }

        public bool GetCanMoveInDirectionInsideBounds(Vector2Int position, GridDirection requestedDirection)
        {
            GridSide boundsSide = GetBoundsSide(position);
            if (boundsSide == GridSide.None)
            {
                // not on bounds => can choose Direction
                return true;
            }

            if (GetBoundsCorner(position) != GridCorner.None)
            {
                // on corner => cannot choose Direction
                return false;
            }

            // on bounds but not on corner => can choose direction that does not leave bounds
            return requestedDirection != boundsSide.GetOutsideDirection();
        }
    }
}