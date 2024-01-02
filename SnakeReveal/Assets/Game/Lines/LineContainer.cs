using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Extensions;
using Game.Grid;
using JetBrains.Annotations;
using UnityEngine;
using Utility;

namespace Game.Lines
{
    public abstract partial class LineContainer : MonoBehaviour, IEnumerable<Line>
    {
        [SerializeField] private SimulationGrid _grid;

        [SerializeField] private LineCache _lineCache;

        [SerializeField] [HideInInspector] protected Line _start;

        [SerializeField] [HideInInspector] private bool _displayLinesInHierarchy = true;

        /// <summary>
        ///     Whether the implementation is a loop or a chain.
        ///     The custom editor uses this flag to map the lines
        ///     to and from a modifiable list of corner positions.
        /// </summary>
        protected abstract bool Loop { get; }

        protected abstract Color GizmosColor { get; }

        protected void Reset()
        {
            _grid = SimulationGrid.EditModeFind();
            _lineCache = FindObjectsByType<LineCache>(FindObjectsInactive.Include, FindObjectsSortMode.None).FirstOrDefault();
        }

        protected void OnDrawGizmos()
        {
            if (_start == null)
            {
                return;
            }

            Color originalGizmoColor = Gizmos.color;
            Gizmos.color = GizmosColor;

            Vector3 startArrowStart = _start.StartWorldPosition;
            float cellDiameter = _grid.SceneCellSize.magnitude;
            Gizmos.DrawSphere(startArrowStart, cellDiameter * 0.2f);
            if (_start.Start != _start.End)
            {
                GizmosUtility.DrawArrow(startArrowStart, _start.EndWorldPosition, cellDiameter * 0.25f, 25f);
            }

            Gizmos.color = originalGizmoColor;
        }

        IEnumerator<Line> IEnumerable<Line>.GetEnumerator()
        {
            return GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public Vector3 GetWorldPosition(Vector2Int position)
        {
            return _grid.GetScenePosition(position).ToVector3(transform.position.z);
        }

        [PublicAPI("Allocation-free enumerator")]
        public Line.Enumerator GetEnumerator()
        {
            return new Line.Enumerator(_start);
        }

        [PublicAPI("Allocation-free enumerator with options")]
        public LineEnumerable AsEnumerable(LineEnumerator.Options options)
        {
            return _start.AsEnumerable(options);
        }
    }
}