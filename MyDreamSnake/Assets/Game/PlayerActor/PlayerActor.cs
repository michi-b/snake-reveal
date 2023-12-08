using Game.Lines;
using Game.Simulation.Grid;
using JetBrains.Annotations;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Serialization;
using Utility;
using Debug = System.Diagnostics.Debug;

namespace Game.PlayerActor
{
    public class PlayerActor : MonoBehaviour
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