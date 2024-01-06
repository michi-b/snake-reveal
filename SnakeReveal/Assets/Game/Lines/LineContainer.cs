using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Extensions;
using Game.Grid;
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
        [SerializeField, HideInInspector] private bool _displayLinesInHierarchy;
        [SerializeField, HideInInspector] private int _clockwiseTurnWeight;

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
        protected int LayerMask => 1 << gameObject.layer;
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