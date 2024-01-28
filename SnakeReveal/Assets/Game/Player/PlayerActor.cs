using System;
using Extensions;
using Game.Enums;
using Game.Grid;
using Game.Lines;
using Game.Player.Controls;
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
        [SerializeField] private GridDirection _lastHorizontalDirection = GridDirection.None;
        [SerializeField] private GridDirection _lastVerticalDirection = GridDirection.None;
        [SerializeField] private Turn _lastTurn = Turn.None;

        private PlayerActorControls _controls;

        [CanBeNull] private Line _currentLine;

        public void Initialize(GridDirection direction, GridDirection lastDirection)
        {
            AxisOrientation currentOrientation = direction.GetOrientation();

            Debug.Assert(currentOrientation != lastDirection.GetOrientation());

            if (lastDirection.GetOrientation() == AxisOrientation.Horizontal)
            {
                _lastHorizontalDirection = lastDirection;
            }
            else
            {
                _lastVerticalDirection = lastDirection;
            }

            Direction = direction;
        }

        public GridDirection Direction
        {
            get => _direction;
            set
            {
                _direction = value;
                if (_direction.GetOrientation() == AxisOrientation.Horizontal)
                {
                    _lastHorizontalDirection = _direction;
                    _lastTurn = _lastVerticalDirection.GetTurn(_lastHorizontalDirection);
                }
                else
                {
                    _lastVerticalDirection = _direction;
                    _lastTurn = _lastHorizontalDirection.GetTurn(_lastVerticalDirection);
                }

                ApplyRendererDirection();
            }
        }

        public int Speed => _speed;

        public Vector2Int Position
        {
            get => _placement.Position;
            set => _placement.Position = value;
        }

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
            transform.SetLocalPositionXY(_grid.GetScenePosition(Position));
        }

        private void ApplyRendererDirection()
        {
            _renderer.ApplyDirection(Direction);
        }

        public void Move()
        {
            Position += Direction.ToVector2Int();

#if DEBUG
            Debug.Assert(_grid.IsInBounds(Position));
#endif

            // todo: extrapolate grid position in Update() instead (this just applies the grid position to scene position for rendering
            ApplyPosition();
        }

        public bool TryMoveCheckingEnemies()
        {
            Vector2 sceneVector = _grid.ToSceneVector(Direction.ToVector2Int());
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

        public bool GetCanMoveInGridBounds(GridDirection requestedDirection)
        {
            return _placement.GetCanMoveInGridBounds(requestedDirection);
        }

        public void TurnOnGridBounds()
        {
            Direction = Direction.Turn(_grid.TryGetCornerTurn(Position, Direction, out Turn cornerTurn)
                ? cornerTurn // literal "corner" case
                : _lastTurn.Reverse()); // default case - invert last turn?
        }
    }
}