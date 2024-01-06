using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Extensions;
using Game.Enums;
using Game.Grid;
using JetBrains.Annotations;
using UnityEditor;
using UnityEngine;
using Utility;

namespace Game.Lines
{
    public abstract class LineContainer : MonoBehaviour, IEnumerable<Line>
    {
        [SerializeField] private SimulationGrid _grid;
        [SerializeField] private LineCache _lineCache;
        [SerializeField, HideInInspector] protected Line _start;
        [SerializeField, HideInInspector] private bool _displayLinesInHierarchy = true;
        [SerializeField, HideInInspector] private int _clockwiseTurnWeight;
        private readonly Collider2D[] _findLinesBuffer = new Collider2D[2];

        protected abstract Color GizmosColor { get; }
        public abstract bool Loop { get; }

        public Line Start
        {
            get => _start;
            protected set => _start = value;
        }

        public SimulationGrid Grid => _grid;
        public LineCache LineCache => _lineCache;
        public abstract Line End { get; }
        protected int ClockwiseTurnWeight => _clockwiseTurnWeight;

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
            Gizmos.DrawSphere(startArrowStart, cellDiameter);
            if (_start.Start != _start.End)
            {
                GizmosUtility.DrawArrow(startArrowStart, _start.EndWorldPosition, cellDiameter * 2, 25f);
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

        protected virtual void PostProcessEditModeLineChanges()
        {
            _clockwiseTurnWeight = EvaluateClockwiseTurnWeight();
        }

        protected virtual int EvaluateClockwiseTurnWeight()
        {
            return AsSpan().SumClockwiseTurnWeight();
        }

        public Vector3 GetWorldPosition(Vector2Int position)
        {
            return _grid.GetScenePosition(position).ToVector3(transform.position.z);
        }

        public LineEnumerator GetEnumerator()
        {
            return new LineEnumerator(_start, End);
        }

        public LineSpan AsSpan()
        {
            return new LineSpan(_start, End);
        }

        public ReverseLineSpan AsReverseSpan()
        {
            return new ReverseLineSpan(_start, End);
        }

        //todo: move to edit mode utility class
        public Line EditModeInstantiateLine(Vector2Int startPosition, Vector2Int endPosition, bool registerUndo)
        {
            Line result = Instantiate(_lineCache.Prefab, transform);
            if (registerUndo)
            {
                Undo.RegisterCreatedObjectUndo(result.gameObject, nameof(EditModeInstantiateLine));
            }

            result.Grid = _grid;
            result.Start = startPosition;
            result.End = endPosition;

            GameObject resultGameObject = result.gameObject;
            if (registerUndo)
            {
                Undo.RecordObject(resultGameObject, nameof(EditModeInstantiateLine) + " - Set Layer");
            }

            resultGameObject.layer = gameObject.layer;

            EditModeUtility.ApplyHideLineInSceneView(this, result);

            return result;
        }

        public bool ContainsLineAt(Vector2Int position)
        {
            return GetSingleColliderAt(position) != null;
        }

        public bool TryGetFirstLineAt(Vector2Int position, out Line line)
        {
            line = GetFirstLineAt(position);
            return line != null;
        }

        [CanBeNull]
        public Line GetFirstLineAt(Vector2Int position)
        {
            Collider2D lineCollider = GetSingleColliderAt(position);
            if (lineCollider == null)
            {
                return null;
            }

            return lineCollider.TryGetComponent(out Line line) ? line : null;
        }

        public bool ContainsLineAt(Vector2Int position, GridDirection direction)
        {
            return GetLineAt(position, direction) != null;
        }

        public bool TryGetLineAt(Vector2Int position, GridDirection loopLineDirection, out Line line)
        {
            line = GetLineAt(position, loopLineDirection);
            return line != null;
        }

        [CanBeNull]
        public Line GetLineAt(Vector2Int position, GridDirection direction)
        {
            foreach (Collider2D lineCollider in GetUpToTwoCollidersAt(position))
            {
                if (lineCollider.TryGetComponent(out Line line))
                {
                    if (line.Direction == direction)
                    {
                        return line;
                    }
                }
            }

            return null;
        }

        private Collider2D GetSingleColliderAt(Vector2Int position)
        {
            return Physics2D.OverlapPoint(position.GetScenePosition(Grid), 1 << gameObject.layer);
        }

        private Span<Collider2D> GetUpToTwoCollidersAt(Vector2Int position)
        {
            int count = Physics2D.OverlapPoint(position.GetScenePosition(Grid), GetContactFilter(), _findLinesBuffer);
#if DEBUG
            Debug.Assert(count <= _findLinesBuffer.Length, $"Overlap point count {count} is larger than collider buffer size {_findLinesBuffer.Length}, " +
                                                           " which should not be possible by design (usage of layers and avoiding overlapping lines).");
#endif
            return new Span<Collider2D>(_findLinesBuffer, 0, count);
        }

        private ContactFilter2D GetContactFilter()
        {
            return new ContactFilter2D
            {
                useLayerMask = true,
                layerMask = 1 << gameObject.layer
            };
        }

        protected Line GetNewLine(Vector2Int start, Vector2Int end)
        {
            return GetNewLine(new LineData(start, end));
        }

        protected Line GetNewLine(LineData lineData)
        {
            Line line = LineCache.Get();
            line.transform.parent = transform;
            line.Initialize(Grid, lineData);
            return line;
        }

        public static class EditModeUtility
        {
            public const string ClockwiseTurnWeightPropertyName = nameof(_clockwiseTurnWeight);
            public const string StartPropertyName = nameof(_start);
            public const string DisplayLinesInHierarchyPropertyName = nameof(_displayLinesInHierarchy);

            public static void Rebuild(LineContainer container, List<Vector2Int> positions)
            {
                Debug.Assert(positions.Count >= 2, "positions.Count >= 2");

                Undo.RegisterFullObjectHierarchyUndo(container.gameObject, nameof(Rebuild) + " - clear Lines");
                ClearLines(container);

                Undo.RegisterFullObjectHierarchyUndo(container.gameObject, nameof(Rebuild));
                Line start = container.EditModeInstantiateLine(positions[0], positions[1], true);
                Line last = start;
                for (int i = 2; i < positions.Count; i++)
                {
                    // note: the order of assignments and registering UNDO operations is extremely sensible here! 
                    Line line = container.EditModeInstantiateLine(last.End, positions[i], true);
                    Undo.RecordObject(line, nameof(Rebuild) + " - patch created line");
                    line.Previous = last;

                    Undo.RecordObject(last, nameof(Rebuild) + " - patch line before created line");
                    last.Next = line;
                    last = line;
                }

                if (container.Loop)
                {
                    // note: the order of assignments and registering UNDO operations is extremely sensible here! 
                    Line loopConnection = container.EditModeInstantiateLine(last.End, start.Start, true);
                    Undo.RecordObject(loopConnection, nameof(Rebuild) + " - patch loop connection");
                    loopConnection.Previous = last;
                    loopConnection.Next = start;

                    Undo.RecordObject(start, nameof(Rebuild) + " - patch start to loop connection");
                    Undo.RecordObject(last, nameof(Rebuild) + " - patch last to loop connection");
                    last.Next = loopConnection;
                    start.Previous = loopConnection;
                }

                Undo.RecordObject(container, nameof(Rebuild) + " Assign Start");
                container._start = start;
            }

            private static void ClearLines(LineContainer container)
            {
                Line current = container._start;
                while (current != null)
                {
                    DestroyImmediate(current.gameObject);
                    current = current.Next;
                }

                container._start = null;
            }

            public static void ApplyHideLinesInSceneView(LineContainer container)
            {
                foreach (Line line in container)
                {
                    ApplyHideLineInSceneView(container, line);
                }
            }

            public static void ApplyHideLineInSceneView(LineContainer container, Line line)
            {
                if (!Application.isPlaying)
                {
                    Undo.RegisterFullObjectHierarchyUndo(line.gameObject, nameof(ApplyHideLineInSceneView));
                }

                line.gameObject.SetVisibleInSceneView(container._displayLinesInHierarchy);
            }

            public static void PostProcessLineChanges(LineContainer container)
            {
                if (!Application.isPlaying)
                {
                    Undo.RecordObject(container, nameof(PostProcessLineChanges));
                }

                container.PostProcessEditModeLineChanges();
            }
        }
    }
}