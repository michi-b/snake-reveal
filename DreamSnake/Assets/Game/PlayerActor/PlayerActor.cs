using Game.Simulation;
using Game.Simulation.Grid;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Serialization;
using Utility;

namespace Game.PlayerActor
{
    public class PlayerActor : SimulationDrivenBehaviour
    {
        [FormerlySerializedAs("_simulationGrid")]
        [SerializeField]
        private SimulationGrid _grid;

        [SerializeField] private int2 _gridPosition;
        [FormerlySerializedAs("_gridDirection")] [SerializeField] private GridDirection _direction = GridDirection.None;
        [SerializeField] private int _speed = 1;
        [SerializeField] private int2 _velocity;

        [SerializeField] private Color _gizmoColor = Color.yellow;
        [SerializeField] [Range(0f, 1f)] private float _gizmoWireAlphaMultiplier = 0.5f;

        private PlayerActorControls _controls;

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
            _gridPosition = _grid.Clamp(_gridPosition);
            _gridPosition += _direction.ToInt2() * _speed;
            ApplyGridPosition();
        }
    }
}