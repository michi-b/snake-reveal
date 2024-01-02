using System.Collections;
using System.Collections.Generic;
using Extensions;
using Game.Grid;
using JetBrains.Annotations;
using UnityEngine;

namespace Game.Lines
{
    public partial class DoubleLinkedLineList : MonoBehaviour, IEnumerable<Line>
    {
        private const int InitialLinesCapacity = 1000;

        [SerializeField] private SimulationGrid _grid;

        [SerializeField] private LineCache _lineCache;

        [SerializeField] private bool _drawGizmos = true;

        [SerializeField] private Color _gizmosColor = Color.blue;

        [SerializeField] [HideInInspector] [CanBeNull]
        protected Line _start;

        protected void Reset()
        {
            _grid = SimulationGrid.EditModeFind();
        }

        protected void OnDrawGizmos()
        {
            if (_start == null || !_drawGizmos)
            {
                return;
            }

            Color originalGizmoColor = Gizmos.color;
            Gizmos.color = _gizmosColor;

            Gizmos.DrawWireSphere(_start.StartWorldPosition, _grid.SceneCellSize.magnitude * 0.5f);

            foreach (Line line in _start.SkipFirst())
            {
                Gizmos.DrawLine(_start.StartWorldPosition, _start.EndWorldPosition);
            }

            Gizmos.color = originalGizmoColor;
        }

        public Vector3 GetWorldPosition(Vector2Int position)
        {
            return _grid.GetScenePosition(position).ToVector3(transform.position.z);
        }

        public Line.Enumerator GetEnumerator()
        {
            return new Line.Enumerator(_start);
        }

        public SkipFirstLineEnumerable SkipFirst()
        {
            return new SkipFirstLineEnumerable(_start);
        }

        IEnumerator<Line> IEnumerable<Line>.GetEnumerator()
        {
            return GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}