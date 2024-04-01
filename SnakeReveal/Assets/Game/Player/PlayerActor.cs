using System;
using Extensions;
using Game.Enums;
using Game.Enums.Extensions;
using Game.Lines;
using Game.Player.Controls;
using Game.Simulation.Grid;
using JetBrains.Annotations;
using UnityEngine;
using Utility;

namespace Game.Player
{
    [RequireComponent(typeof(GridPlacement))]
    public class PlayerActor : MonoBehaviour
    {
        [SerializeField] private SimulationGrid _grid;
        [SerializeField] private GridPlacement _placement;
        [SerializeField] private PlayerActorRenderer _renderer;

        [SerializeField, Header("Enemies")] private LayerMask _enemiesLayerMask;
        [SerializeField] private float _enemyCollisionRadius = 0.005f;

        [SerializeField, Header("Runtime")] private GridDirection _direction = GridDirection.None;
        [SerializeField] private int _speed = 1;
        [SerializeField] private GridDirection _latestHorizontalDirection = GridDirection.None;
        [SerializeField] private GridDirection _latestVerticalDirection = GridDirection.None;

        private PlayerActorControls _controls;

        [CanBeNull] private Line _currentLine;

        public void Initialize(GridDirection direction, GridDirection lastDirection)
        {
            GridAxis currentAxis = direction.GetAxis();

            Debug.Assert(currentAxis != lastDirection.GetAxis());

            Debug.Assert(direction.GetAxis() != lastDirection.GetAxis());

            SetLatestDirection(lastDirection);

            Direction = direction;
        }

        private void SetLatestDirection(GridDirection direction)
        {
            switch (direction.GetAxis())
            {
                case GridAxis.Horizontal:
                    _latestHorizontalDirection = direction;
                    break;
                case GridAxis.Vertical:
                    _latestVerticalDirection = direction;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(direction), direction, null);
            }
        }

        public GridDirection Direction
        {
            get => _direction;
            set
            {
                if (value != _direction)
                {
#if DEBUG && false
                    Debug.Log($"Player actor direction change: {_direction} -> {value}");
#endif
                    
                    _direction = value;
                    if (value != GridDirection.None)
                    {
                        SetLatestDirection(_direction);
                    }

                    ApplyRendererDirection();
                }
            }
        }

        public GridDirection GetLatestDirection(GridAxis axis)
        {
            return axis switch
            {
                GridAxis.Horizontal => _latestHorizontalDirection,
                GridAxis.Vertical => _latestVerticalDirection,
                _ => throw new ArgumentOutOfRangeException(nameof(axis), axis, null)
            };
        }

        public int Speed => _speed;

        public Vector2Int Position
        {
            get => _placement.Position;
            set => _placement.Position = value;
        }

        public SimulationGrid Grid => _grid;

        protected void Reset()
        {
            _placement = GetComponent<GridPlacement>();
        }

        protected void OnDrawGizmos()
        {
            GizmosUtility.DrawCircle(transform.position, _enemyCollisionRadius, Color.red);
        }

        protected void OnValidate()
        {
            ApplyRendererDirection();
        }

        public void ApplyPosition()
        {
            transform.SetLocalPositionXY(Grid.GetScenePosition(Position));
        }

        private void ApplyRendererDirection()
        {
            _renderer.ApplyDirection(Direction);
        }

        public void Move()
        {
            Position += Direction.AsVector();

#if DEBUG
            Debug.Assert(Grid.GetIsInBounds(Position));
#endif

            // todo: extrapolate grid position in Update() instead (this just applies the grid position to scene position for rendering
            ApplyPosition();
        }

        public bool TryMoveCheckingEnemies()
        {
            Vector2 sceneVector = Grid.ToSceneVector(Direction.AsVector());

            RaycastHit2D hit = Physics2D.CircleCast(transform.position,
                _enemyCollisionRadius,
                sceneVector,
                sceneVector.magnitude,
                _enemiesLayerMask);

            if (hit.collider != null)
            {
                return false;
            }

            Move();

            return true;
        }

        public bool GetCanMoveInGridBounds(GridDirection requestedDirection) => _grid.GetCanMoveInDirectionInsideBounds(Position, requestedDirection);

        public GridDirections RestrictDirectionsInBounds(GridDirections directions) => directions.RestrictInBounds(_grid, Position);
    }
}