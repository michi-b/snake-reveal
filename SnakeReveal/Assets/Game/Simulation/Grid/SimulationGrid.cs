using System;
using Extensions;
using Game.Enums;
using Game.Enums.Extensions;
using Game.Lines;
using Game.Simulation.Grid.Bounds;
using JetBrains.Annotations;
using Unity.Mathematics;
using UnityEngine;

namespace Game.Simulation.Grid
{
    public class SimulationGrid : MonoBehaviour
    {
        [SerializeField] private Vector2Int _size = new(1024, 1024);
        [SerializeField] private Vector2 _sceneCellSize;
        [SerializeField] private Vector2 _lowerLeftCornerScenePosition;
        [SerializeField] private int _gizmoCellSizeMultiplier = 32;
        [SerializeField] private Color _gizmoColor = new(0.7f, 0.7f, 0.7f, 0.5f);
        [SerializeField] private GridBounds _bounds;
        [SerializeField, HideInInspector] private Vector2 _sceneSize = new(10.8f, 10.8f);
        [SerializeField, HideInInspector] private float _paddingThickness;
        [SerializeField, HideInInspector] private float _collidersThickness;

        [SerializeField, HideInInspector] private Camera _camera;
        [SerializeField, HideInInspector] private float _orthographicHeight;
        [SerializeField, HideInInspector] private float _sceneAspect;

        public const string SceneSizePropertyName = nameof(_sceneSize);
        public const string PaddingThicknessPropertyName = nameof(_paddingThickness);
        public const string CollidersThicknessPropertyName = nameof(_collidersThickness);

        public const string CameraPropertyName = nameof(_camera);
        public const string OrthographicHeightPropertyName = nameof(_orthographicHeight);
        public const string SceneAspectPropertyName = nameof(_sceneAspect);

        public Camera Camera => _camera;
        public Vector2Int Size => _size;

        public Vector2 SceneCellSize => _sceneCellSize;
        public Vector2Int CenterPosition => _size / 2;

        public int GetCellCount() => _size.x * _size.y;

        protected void Update()
        {
            AdjustCamera();
        }

        public void AdjustCamera()
        {
            float orthographicHeight = _sceneAspect > _camera.aspect
                ? _orthographicHeight * _sceneAspect / _camera.aspect
                : _orthographicHeight;

            // ReSharper disable once CompareOfFloatsByEqualityOperator
            if (orthographicHeight != _camera.orthographicSize)
            {
                _camera.orthographicSize = orthographicHeight;
            }
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

        public Vector2 GetScenePosition(Vector2Int gridPosition) => _lowerLeftCornerScenePosition + SceneCellSize * gridPosition;

        private Vector3 GetWorldPosition(Vector2Int gridPosition) => GetWorldPosition(GetScenePosition(gridPosition));

        private Vector3 GetWorldPosition(Vector2 scenePosition) => scenePosition.ToVector3(transform.position.z);

        public Vector2Int Clamp(Vector2Int gridPosition) =>
            new(
                math.clamp(gridPosition.x, 0, _size.x),
                math.clamp(gridPosition.y, 0, _size.y)
            );

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

        public Vector2 ToSceneVector(Vector2Int gridVector) => _sceneCellSize * gridVector;

        public bool GetIsInBounds(Vector2Int position) => position.x >= 0 && position.x <= _size.x && position.y >= 0 && position.y <= _size.y;

        public bool GetIsOnBounds(Vector2Int position) => position.x == 0 || position.x == _size.x || position.y == 0 || position.y == _size.y;

        public GridSide GetBoundsSide(Line line)
        {
            return line.Direction.GetAxis() switch
            {
                GridAxis.Horizontal => line.Start.y == 0
                    ? GridSide.Bottom
                    : line.Start.y == _size.y
                        ? GridSide.Top
                        : GridSide.None,
                GridAxis.Vertical => line.Start.x == 0
                    ? GridSide.Left
                    : line.Start.x == _size.x
                        ? GridSide.Right
                        : GridSide.None,
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        /// <returns>the bounds side of the position, which might be either adjacent side on corners (undefined)</returns>
        public GridSide GetBoundsSide(Vector2Int actorPosition) =>
            actorPosition.x == 0 ? GridSide.Left :
            actorPosition.x == _size.x ? GridSide.Right :
            actorPosition.y == 0 ? GridSide.Bottom :
            actorPosition.y == _size.y ? GridSide.Top : GridSide.None;

        public GridCorner GetBoundsCorner(Vector2Int position)
        {
            if (position.x == 0)
            {
                if (position.y == 0)
                {
                    return GridCorner.BottomLeft;
                }
                else if (position.y == _size.y)
                {
                    return GridCorner.TopLeft;
                }
            }
            else if (position.x == _size.x)
            {
                if (position.y == 0)
                {
                    return GridCorner.BottomRight;
                }
                else if (position.y == _size.y)
                {
                    return GridCorner.TopRight;
                }
            }

            return GridCorner.None;
        }

        public bool GetCanMoveInDirectionInsideBounds(Vector2Int position, GridDirection requestedDirection)
        {
            GridSide boundsSide = GetBoundsSide(position);
            if (boundsSide == GridSide.None)
            {
                // not on bounds => can choose Direction
                return true;
            }

            return GetBoundsCorner(position) == GridCorner.None && requestedDirection != boundsSide.GetOutsideDirection();
        }

        public void ApplyColliderThickness()
        {
            if (_bounds == null || _bounds.Colliders == null)
            {
                Debug.LogWarning("No colliders to apply thickness to");
                return;
            }

            float paddingSize = _paddingThickness * 2f;
            _bounds.Colliders.SetSize(_sceneSize + new Vector2(paddingSize, paddingSize), _collidersThickness);
        }

        public void ApplyPaddingThickness()
        {
            if (_bounds == null || _bounds.Padding == null)
            {
                Debug.LogWarning("No bounds to apply padding to");
                return;
            }

            _bounds.Padding.SetSize(_sceneSize, _paddingThickness);
        }
    }
}