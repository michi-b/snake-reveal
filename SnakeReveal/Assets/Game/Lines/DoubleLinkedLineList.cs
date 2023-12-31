using Extensions;
using Game.Grid;
using JetBrains.Annotations;
using UnityEngine;

namespace Game.Lines
{
    public partial class DoubleLinkedLineList : MonoBehaviour
    {
        private const int InitialLinesCapacity = 1000;

        [SerializeField] private SimulationGrid _grid;

        [SerializeField] private bool _drawGizmos = true;

        [SerializeField] private Color _gizmosColor = Color.blue;

        [SerializeField] [HideInInspector] [CanBeNull]
        protected Line _start;

        public SimulationGrid Grid => _grid;

        protected void Reset()
        {
            _grid = SimulationGrid.EditModeFind();
        }

        protected void OnDrawGizmos()
        {
            if (_start == null)
            {
                return;
            }

            Color originalGizmoColor = Gizmos.color;
            Gizmos.color = _gizmosColor;

            Gizmos.DrawWireSphere(_start.StartWorldPosition, Grid.SceneCellSize.magnitude * 0.5f);

            foreach (Line line in _start.SkipFirst())
            {
                Gizmos.DrawLine(_start.StartWorldPosition, _start.EndWorldPosition);
            }

            Gizmos.color = originalGizmoColor;
        }

        public Vector3 GetWorldPosition(Vector2Int position)
        {
            return Grid.GetScenePosition(position).ToVector3(transform.position.z);
        }

        private struct Skip
        {
        }
    }
}