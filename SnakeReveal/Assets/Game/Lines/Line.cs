using System;
using System.Collections.Generic;
using System.Diagnostics;
using Extensions;
using Game.Enums;
using Game.Grid;
using JetBrains.Annotations;
using UnityEngine;
using Utility;
using Debug = UnityEngine.Debug;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Game.Lines
{
    /// <summary>
    ///     minimal immutable struct to cache information of line container lines
    /// </summary>
    [RequireComponent(typeof(LineRenderer)), RequireComponent(typeof(EdgeCollider2D))]
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

        public SimulationGrid Grid
        {
            get => _grid;
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
                    _previous.Apply();
                }

                Apply();
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
                    _next.Apply();
                }

                Apply();
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

        public LineData Data => _line;

        protected void Reset()
        {
            _grid = SimulationGrid.EditModeFind();
            _collider = GetComponent<EdgeCollider2D>();
            _renderer = GetComponent<LineRenderer>();
            Apply();
        }

        public override string ToString()
        {
            return _line.ToString();
        }

        public void Place(LineData lineData)
        {
            _line = lineData;
            Apply();
        }

        public void Initialize(SimulationGrid grid)
        {
            _previous = null;
            _next = null;
            _line = new LineData(Vector2Int.zero, Vector2Int.right);
            _grid = grid;
        }

        private void Apply()
        {
            _line.Direction = _line.Start.GetDirection(_line.End);

            transform.SetLocalPositionXY(_grid.GetScenePosition(Start));

            Vector2Int delta = End - Start;

            Vector2 sceneDelta = delta * _grid.SceneCellSize;

            // first point of renderer must always be Vector2.zero
            _renderer.SetPosition(1, sceneDelta);

            // first element in collider points update buffer must always be Vector2.zero
            ColliderPointsUpdateBuffer[1] = sceneDelta;
            _collider.SetPoints(ColliderPointsUpdateBuffer);
        }

        public bool Contains(Vector2Int position)
        {
            return Direction switch
            {
                GridDirection.None => false,
                GridDirection.Right => IsSameY() && position.x >= Start.x && position.x <= End.x,
                GridDirection.Up => IsSameX() && position.y >= Start.y && position.y <= End.y,
                GridDirection.Left => IsSameY() && position.x <= Start.x && position.x >= End.x,
                GridDirection.Down => IsSameX() && position.y <= Start.y && position.y >= End.y,
                _ => throw new ArgumentOutOfRangeException()
            };

            bool IsSameY()
            {
                return position.y == Start.y;
            }

            bool IsSameX()
            {
                return position.x == Start.x;
            }
        }

        public GridDirection GetDirection(bool startToEnd = true)
        {
            return startToEnd ? _line.Direction : _line.Direction.Reverse();
        }

        public Vector2Int GetEnd(bool startToEnd = true)
        {
            return startToEnd ? _line.End : _line.Start;
        }

        public Line GetNext(bool startToEnd = true)
        {
            return startToEnd ? Next : Previous;
        }

        public Line GetPrevious(bool startToEnd = true)
        {
            return startToEnd ? Previous : Next;
        }

        public bool TryExtend(Vector2Int targetPosition)
        {
            if (Direction switch
                {
                    GridDirection.None => Start == End && (targetPosition.x == Start.x || targetPosition.y == Start.y),
                    GridDirection.Right => targetPosition.y == Start.y && targetPosition.x >= End.x,
                    GridDirection.Up => targetPosition.x == Start.x && targetPosition.y >= End.y,
                    GridDirection.Left => targetPosition.y == Start.y && targetPosition.x <= End.x,
                    GridDirection.Down => targetPosition.x == Start.x && targetPosition.y <= End.y,
                    _ => throw new ArgumentOutOfRangeException()
                })
            {
                End = targetPosition;
                return true;
            }

            return false;
        }

        [Conditional("DEBUG")]
        public void Validate()
        {
            Debug.Assert(Start != End
                         && Direction != GridDirection.None
                         && Direction == Start.GetDirection(End),
                $"Invalid line: {this}", this);
        }

#if UNITY_EDITOR
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

        public void EditModeStitchToNext(Line next)
        {
            RegisterUndo(nameof(EditModeStitchToNext) + " - current");
            End = next.Start;
            _next = next;

            _next!.RegisterUndo(nameof(EditModeStitchToNext) + " - next");
            next._previous = this;
        }
#endif

// Selection and HandleUtility class ar in editor assembly, therefore this preprocessor switch is required
#if UNITY_EDITOR
        protected void OnDrawGizmosSelected()
        {
            if (!Selection.Contains(gameObject) || _grid == null)
            {
                return;
            }

            Color originalGizmoColor = Gizmos.color;
            Gizmos.color = Color.black;

            DrawArrowGizmo();

            Gizmos.color = originalGizmoColor;
        }

        public void DrawArrowGizmo()
        {
            Vector3 startWorldPosition = StartWorldPosition;
            Gizmos.DrawWireSphere(startWorldPosition, HandleUtility.GetHandleSize(startWorldPosition) * 0.1f);
            GizmosUtility.DrawArrow(startWorldPosition, EndWorldPosition);
        }

#endif
    }
}