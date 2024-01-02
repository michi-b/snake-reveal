using System;
using System.Collections.Generic;
using System.Linq;
using Extensions;
using Game.Grid;
using UnityEditor;
using UnityEngine;

namespace Game.Lines
{
    public partial class LineContainer
    {
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
                Undo.RegisterFullObjectHierarchyUndo(container, "Rebuild Double Linked Line List");
                ClearLines(container);

                Line start = InstantiateLine(container, positions[0], positions[1]);
                container._start = start;
                Line last = start;

                for (int i = 2; i < positions.Count; i++)
                {
                    Line line = InstantiateLine(container, last.End, positions[i]);
                    last.Next = line;
                    line.Previous = last;
                    last = line;
                }

                if (container.Loop)
                {
                    Line loopConnection = InstantiateLine(container, last.End, start.Start);
                    last.Next = loopConnection;
                    loopConnection.Previous = last;
                    start.Previous = loopConnection;
                    loopConnection.Next = start;
                }
            }

            public static bool GetIsLoop(LineContainer container)
            {
                return container.Loop;
            }

            private static Line InstantiateLine(LineContainer container, Vector2Int startPosition, Vector2Int endPosition)
            {
                Line result = Instantiate(container._lineCache.Prefab, container.transform);
                Undo.RegisterCreatedObjectUndo(result.gameObject, "Instantiate Line");
                result.Grid = container._grid;
                result.Start = startPosition;
                result.End = endPosition;
                ApplyHideLineInSceneView(container, result);
                return result;
            }

            private static void ClearLines(LineContainer container)
            {
                foreach (Line line in container.ToArray())
                {
                    Undo.DestroyObjectImmediate(line.gameObject);
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

            public static void Insert(LineContainer container, LineContainer insertTarget)
            {
                throw new NotImplementedException();
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
        }
    }
}