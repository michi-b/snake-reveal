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
        [SerializeField] private GridDirection _gridDirection;
        [SerializeField] private int _speed;
        [SerializeField] private Color _gizmoColor = Color.yellow;
        [SerializeField] [Range(0f, 1f)] private float _gizmoWireAlphaMultiplier = 0.5f;

        protected override void OnEnable()
        {
            ApplyGridPosition();
            base.OnEnable();
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

        private void ApplyGridPosition()
        {
            _grid.Place(transform, _gridPosition);
        }

        public override void SimulationStepUpdate()
        {
            _gridPosition = _grid.Clamp(_gridPosition);
            _gridPosition += _gridDirection.ToInt2() * _speed;
            ApplyGridPosition();
        }
    }
}