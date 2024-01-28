using System;
using Extensions;
using Game.Enums;
using JetBrains.Annotations;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem.iOS;

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
        public Vector2Int BottomLeft => Vector2Int.zero;
        public Vector2Int TopLeft => new(0, _size.y);
        public Vector2Int TopRight => _size;
        public Vector2Int BottomRight => new(_size.x, 0);

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

        public bool IsInBounds(Vector2Int position)
        {
            return position.x >= 0 && position.x <= _size.x && position.y >= 0 && position.y <= _size.y;
        }

        public bool TryGetCornerTurn(Vector2Int position, GridDirection direction, out Turn turn)
        {
            switch (direction)
            {
                case GridDirection.None:
                    throw new ArgumentOutOfRangeException(nameof(direction), direction, null);
                case GridDirection.Right:
                    if (GetIsTopRightCorner())
                    {
                        turn = Turn.Right;
                        return true;
                    }

                    if (GetIsBottomRightCorner())
                    {
                        turn = Turn.Left;
                        return true;
                    }

                    break;
                case GridDirection.Up:
                    if (GetIsTopLeftCorner())
                    {
                        turn = Turn.Right;
                        return true;
                    }

                    if (GetIsTopRightCorner())
                    {
                        turn = Turn.Left;
                        return true;
                    }

                    break;
                case GridDirection.Left:
                    if (GetIsBottomLeftCorner())
                    {
                        turn = Turn.Right;
                        return true;
                    }

                    if (GetIsTopLeftCorner())
                    {
                        turn = Turn.Left;
                        return true;
                    }

                    break;
                case GridDirection.Down:
                    if (GetIsBottomRightCorner())
                    {
                        turn = Turn.Right;
                        return true;
                    }

                    if (GetIsBottomLeftCorner())
                    {
                        turn = Turn.Left;
                        return true;
                    }

                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(direction), direction, null);
            }

            turn = default;
            return false;

            bool GetIsTopRightCorner()
            {
                return position == TopRight;
            }

            bool GetIsTopLeftCorner()
            {
                return position == TopLeft;
            }

            bool GetIsBottomLeftCorner()
            {
                return position == BottomLeft;
            }

            bool GetIsBottomRightCorner()
            {
                return position == BottomRight;
            }
        }
    }
}