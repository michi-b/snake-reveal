using Game.Lines;
using Game.Simulation;
using Game.Simulation.Grid;
using JetBrains.Annotations;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;
using Utility;
using Debug = System.Diagnostics.Debug;

namespace Game.PlayerActor
{
    public class PlayerActor : SimulationDrivenBehaviour
    {
        [SerializeField] private PlayerActorRenderer _renderer;

        [SerializeField] private SimulationGrid _grid;

        [SerializeField] private LineCache _lineCache;

        [SerializeField] private LineContainer _lineContainer;

        [SerializeField] private int2 _gridPosition;

        [FormerlySerializedAs("_gridDirection")]
        [SerializeField]
        private GridDirection _direction = GridDirection.None;

        [SerializeField] private int _speed = 1;
        [SerializeField] private int2 _velocity;

        [SerializeField] private Color _gizmoColor = Color.yellow;
        [SerializeField] [Range(0f, 1f)] private float _gizmoWireAlphaMultiplier = 0.5f;

        private PlayerActorControls _controls;

        [CanBeNull] private Line _currentLine;

        public int Speed
        {
            get => _speed;
            set
            {
                _speed = value;
                UpdateVelocity();
            }
        }

        public GridDirection Direction
        {
            get => _direction;
            set
            {
                _direction = value;
                UpdateRendererDirection();
                UpdateVelocity();
            }
        }

        protected virtual void Awake()
        {
            _controls = new PlayerActorControls();
            _controls.PlayerActor.Right.performed += _ => TurnRight();
            _controls.PlayerActor.Up.performed += _ => TurnUp();
            _controls.PlayerActor.Left.performed += _ => TurnLeft();
            _controls.PlayerActor.Down.performed += _ => TurnDown();
        }

        protected override void OnEnable()
        {
            _controls.Enable();
            ApplyGridPosition();
            base.OnEnable();
        }

        protected override void OnDisable()
        {
            _controls.Disable();
            base.OnDisable();
        }

        protected void OnDrawGizmos()
        {
            Color oldColor = Gizmos.color;

            Gizmos.color = _gizmoColor;
            float singleCellRadius = _grid.SceneCellSize.magnitude * MathfUtility.UnitDiagonal;
            Vector3 position = transform.position;
            Gizmos.DrawSphere(position, singleCellRadius);

            Color wireColor = _gizmoColor;
            wireColor.a *= _gizmoWireAlphaMultiplier;
            Gizmos.color = wireColor;
            Gizmos.DrawWireSphere(position, singleCellRadius * _grid.GizmoCellSizeMultiplier);

            Gizmos.color = oldColor;
        }

        protected void OnValidate()
        {
            UpdateVelocity();
            UpdateRendererDirection();
        }

        private void UpdateRendererDirection()
        {
            _renderer.ApplyDirection(Direction);
        }

        private void UpdateVelocity()
        {
            _velocity = _direction.ToInt2() * _speed;
        }

        private void TurnDown()
        {
            Direction = GridDirection.Down;
        }

        private void TurnLeft()
        {
            Direction = GridDirection.Left;
        }

        private void TurnUp()
        {
            Direction = GridDirection.Up;
        }

        private void TurnRight()
        {
            Direction = GridDirection.Right;
        }

        private void ApplyGridPosition()
        {
            _grid.Place(transform, _gridPosition);
        }

        public override void SimulationStepUpdate()
        {
            if (_direction != GridDirection.None)
            {
                Move(_direction.ToInt2() * _speed);
            }
        }

        private void Move(int2 move)
        {
            EnsureHasCurrentLine();

            Debug.Assert(_currentLine != null, nameof(_currentLine) + " != null");

            int2 newGridPosition = _grid.Clamp(_gridPosition + move);

            if (newGridPosition.x != _currentLine.Start.x && newGridPosition.y != _currentLine.Start.y)
            {
                StartNewLine();
            }

            _lineContainer.Place(_currentLine, _currentLine.Start, newGridPosition);

            _gridPosition = newGridPosition;

            ApplyGridPosition();
        }

        private void EnsureHasCurrentLine()
        {
            if (_currentLine == null)
            {
                StartNewLine();
            }
        }

        private void StartNewLine()
        {
            _currentLine = _lineCache.Get();
            _lineContainer.Place(_currentLine, _gridPosition, _gridPosition);
        }
    }
}