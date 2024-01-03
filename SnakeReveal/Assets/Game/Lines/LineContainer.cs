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
    public abstract partial class LineContainer : MonoBehaviour, IEnumerable<Line>
    {
        [SerializeField] private SimulationGrid _grid;

        [SerializeField] private LineCache _lineCache;

        [SerializeField, HideInInspector] private Line _start;

        [SerializeField, HideInInspector] private bool _displayLinesInHierarchy;

        protected abstract Color GizmosColor { get; }

        public abstract bool Loop { get; }

        protected Line Start => _start;

        protected SimulationGrid Grid => _grid;

        private Line ExclusiveEnd => Loop ? _start : null;

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

        protected abstract void PostProcessLineChanges();

        public Vector3 GetWorldPosition(Vector2Int position)
        {
            return _grid.GetScenePosition(position).ToVector3(transform.position.z);
        }

        public LineEnumerator GetEnumerator()
        {
            return new LineEnumerator(_start, ExclusiveEnd, Loop);
        }

        public static class EditModeUtility
        {
            public const string StartPropertyName = nameof(_start);
            public const string DisplayLinesInHierarchyPropertyName = nameof(_displayLinesInHierarchy);

            public static Line GetStart(LineContainer container)
            {
                return container._start;
            }

            public static void Rebuild(LineContainer container, List<Vector2Int> positions)
            {
                Debug.Assert(positions.Count >= 2, "positions.Count >= 2");

                Undo.RegisterFullObjectHierarchyUndo(container.gameObject, nameof(Rebuild) + " - clear Lines");
                ClearLines(container);

                Undo.RegisterFullObjectHierarchyUndo(container.gameObject, nameof(Rebuild));
                Line start = InstantiateLine(container, positions[0], positions[1]);
                Line last = start;
                for (int i = 2; i < positions.Count; i++)
                {
                    // note: the order of assignments and registering UNDO operations is extremely sensible here! 
                    Line line = InstantiateLine(container, last.End, positions[i]);
                    Undo.RecordObject(line, nameof(Rebuild) + " - patch created line");
                    line.Previous = last;

                    Undo.RecordObject(last, nameof(Rebuild) + " - patch line before created line");
                    last.Next = line;
                    last = line;
                }

                if (container.Loop)
                {
                    // note: the order of assignments and registering UNDO operations is extremely sensible here! 
                    Line loopConnection = InstantiateLine(container, last.End, start.Start);
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

            public static Line InstantiateLine(LineContainer container, Vector2Int startPosition, Vector2Int endPosition)
            {
                Line result = Instantiate(container._lineCache.Prefab, container.transform);
                Undo.RecordObject(result, nameof(InstantiateLine) + " - Initialization");
                result.Grid = container._grid;
                result.Start = startPosition;
                result.End = endPosition;

                GameObject resultGameObject = result.gameObject;
                Undo.RecordObject(resultGameObject, nameof(InstantiateLine) + " - Set Layer");
                resultGameObject.layer = container.gameObject.layer;
                Undo.RegisterCreatedObjectUndo(resultGameObject, nameof(InstantiateLine));

                ApplyHideLineInSceneView(container, result);

                return result;
            }

            private static void ClearLines(LineContainer container)
            {
                if (container._start != null)
                {
                    foreach (Line line in container.ToArray())
                    {
                        Undo.DestroyObjectImmediate(line.gameObject);
                    }
                }

                container._start = null;
            }

            public static bool GetHasGridAndLineCache(LineContainer container)
            {
                return container._grid != null && container._lineCache != null;
            }

            public static SimulationGrid GetGrid(LineContainer container)
            {
                return container._grid;
            }

            public static void ApplyHideLinesInSceneView(LineContainer container)
            {
                foreach (Line line in container)
                {
                    ApplyHideLineInSceneView(container, line);
                }
            }

            private static void ApplyHideLineInSceneView(LineContainer container, Line line)
            {
                Undo.RegisterFullObjectHierarchyUndo(line.gameObject, nameof(ApplyHideLineInSceneView));
                line.gameObject.SetVisibleInSceneView(container._displayLinesInHierarchy);
            }


            public static void PostProcessLineChanges(LineContainer container)
            {
                Undo.RecordObject(container, nameof(PostProcessLineChanges));
                container.PostProcessLineChanges();
            }

            public static Line GetExclusiveEnd(LineContainer container)
            {
                return container.Loop ? container._start : null;
            }
        }
    }
}