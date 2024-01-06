using System.Collections.Generic;
using System.Diagnostics;
using Extensions;
using Game.Enums;
using Game.Grid;
using JetBrains.Annotations;
using UnityEditor;
using UnityEngine;
using Utility;

namespace Game.Lines
{
    /// <summary>
    ///     minimal immutable struct to cache information of line container lines
    /// </summary>
    [RequireComponent(typeof(LineRenderer)), RequireComponent(typeof(EdgeCollider2D)), DebuggerDisplay("{DebuggerDisplay,nq}")]
    public class Line : MonoBehaviour
    {
        private static readonly List<Vector2> ColliderPointsUpdateBuffer = new() { Vector2.zero, Vector2.right };

        [SerializeField] private SimulationGrid _grid;
        [SerializeField] private LineRenderer _renderer;
        [SerializeField] private EdgeCollider2D _collider;

        [SerializeField] private LineData _line;
        [SerializeField, CanBeNull] private Line _previous;
        [SerializeField, CanBeNull] private Line _next;

        public Line(Vector2Int start, Vector2Int end)
        {
            _line = new LineData(start, end);
        }

        public string DebuggerDisplay => $"{_line.DebuggerDisplay})";

        public SimulationGrid Grid
        {
            set => _grid = value;
        }

        public GridDirection Direction => _line.Direction;

        public Vector2Int Start
        {
            get => _line.Start;
            set
            {
                _line.Start = value;
                if (_previous != null)
                {
                    _previous._line.End = value;
                    _previous.ApplyPositions();
                }

                ApplyPositions();
            }
        }

        public Vector2Int End
        {
            get => _line.End;
            set
            {
                _line.End = value;
                if (_next != null)
                {
                    _next._line.Start = value;
                    _next.ApplyPositions();
                }

                ApplyPositions();
            }
        }

        public AxisOrientation Orientation => _line.Direction.GetOrientation();

        public Vector3 StartWorldPosition => transform.position;
        public Vector3 EndWorldPosition => transform.position + _renderer.GetPosition(1);

        [CanBeNull]
        public Line Previous
        {
            get => _previous;
            set => _previous = value;
        }

        [CanBeNull]
        public Line Next
        {
            get => _next;
            set => _next = value;
        }

        public LineData DataStruct => _line;

        protected void Reset()
        {
            _grid = SimulationGrid.EditModeFind();
        }

        protected void OnDrawGizmosSelected()
        {
            if (Selection.Contains(gameObject))
            {
                Color originalGizmoColor = Gizmos.color;
                Gizmos.color = Color.black;

                float diameter = _grid.SceneCellSize.magnitude * 0.25f;
                Vector3 startWorldPosition = StartWorldPosition;
                Vector3 endWorldPosition = EndWorldPosition;
                Gizmos.DrawWireSphere(startWorldPosition, diameter);
                Vector3 direction = endWorldPosition - startWorldPosition;
                GizmosUtility.DrawArrowHead(endWorldPosition, direction, diameter);

                Gizmos.color = originalGizmoColor;
            }
        }

        private void ApplyPositions()
        {
            _line.ReevaluateDirection();

            transform.SetLocalPositionXY(_grid.GetScenePosition(Start));

            Vector2Int delta = End - Start;

            Vector2 sceneDelta = delta * _grid.SceneCellSize;

            // first point of renderer must always be Vector2.zero
            _renderer.SetPosition(1, sceneDelta);

            // first element in collider points update buffer must always be Vector2.zero
            ColliderPointsUpdateBuffer[1] = sceneDelta;
            _collider.SetPoints(ColliderPointsUpdateBuffer);
        }

        public void RegisterUndoWithNeighbors(string operationName)
        {
            if (_previous != null)
            {
                _previous.RegisterUndo(operationName);
            }

            RegisterUndo(operationName);
            if (_next != null)
            {
                _next.RegisterUndo(operationName);
            }
        }

        public void RegisterUndo(string operationName)
        {
            Undo.RegisterFullObjectHierarchyUndo(this, operationName);
        }

        public void Stitch(Line previous, Line next)
        {
            StitchToPrevious(previous);
            StitchToNext(next);
        }

        public void StitchToPrevious(Line previous)
        {
            Start = previous.End;
            _previous = previous;
            previous._next = this;
        }

        public void StitchToNext(Line next)
        {
            End = next.Start;
            _next = next;
            next._previous = this;
        }
    }
}