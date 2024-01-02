using System.Collections.Generic;
using System.Linq;
using Game.Grid;
using UnityEditor;
using UnityEngine;

namespace Game.Lines
{
    public partial class DoubleLinkedLineList
    {
        public static class EditModeUtility
        {
            public const string StartPropertyName = nameof(_start);

            public static Line GetStart(DoubleLinkedLineList container)
            {
                return container._start;
            }

            public static void Rebuild(DoubleLinkedLineList container, List<Vector2Int> positions)
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
            }

            private static Line InstantiateLine(DoubleLinkedLineList container, Vector2Int startPosition, Vector2Int endPosition)
            {
                Line result = Instantiate(container._lineCache.Prefab, container.transform);
                Undo.RegisterCreatedObjectUndo(result.gameObject, "Instantiate Line");
                result.Grid = container._grid;
                result.Start = startPosition;
                result.End = endPosition;
                return result;
            }

            private static void ClearLines(DoubleLinkedLineList container)
            {
                foreach (Line line in container.ToArray())
                {
                    Undo.DestroyObjectImmediate(line.gameObject);
                }

                container._start = null;
            }

            public static bool GetIsFullyAssigned(DoubleLinkedLineList container)
            {
                return container._grid != null && container._lineCache != null;
            }

            public static SimulationGrid GetGrid(DoubleLinkedLineList container)
            {
                return container._grid;
            }
        }

#if false
            public const string CollidersPropertyName = nameof(_colliders);
            public const string LoopPropertyName = nameof(_loop);

            public static void EditModeReevaluateClockwiseTurnWeight(DoubleLinkedLineList target)
            {
                var lastDirection = GridDirection.None;

                if (target._loop)
                {
                    for (int i = target.Count - 1; i > 0; i--)
                    {
                        lastDirection = target._lines[i].Direction;
                        if (lastDirection != GridDirection.None)
                        {
                            break;
                        }
                    }
                }

                // sum of clockwise turns - sum of counter clockwise turns
                int clockwiseWeight = 0;

                // ReSharper disable once ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator
                // not using linq to avoid allocation
                foreach (Line line in target._lines)
                {
                    if (line.Direction == GridDirection.None)
                    {
                        continue;
                    }

                    if (lastDirection != GridDirection.None)
                    {
                        Turn turn = lastDirection.GetTurn(line.Direction);
                        clockwiseWeight += turn.GetClockwiseWeight();
                    }

                    lastDirection = line.Direction;
                }

                target._clockwiseTurnWeight = clockwiseWeight;
            }

            public static void RebuildLineRenderers(DoubleLinkedLineList target)
            {
                foreach (LineChainRenderer lineChainRenderer in target._lineRenderers)
                {
                    lineChainRenderer.EditModeRebuild(target.Grid, target._lines);
                }
            }

            public static void FixLines(DoubleLinkedLineList target)
            {
                if (target.Count == 0)
                {
                    return;
                }

                for (int i = 0; i < target._lines.Count; i++)
                {
                    target._lines[i] = target._lines[i].Clamp(target.Grid);
                }

                for (int i = 0; i < target.Count - 1; i++)
                {
                    target._lines[i] = target._lines[i].WithEnd(target._lines[i + 1].Start).AsOpenChainEnd(false);
                }

                // special handling of last line based on loop
                if (target.Loop)
                {
                    target._lines[^1] = target._lines[^1].WithEnd(target._lines[0].Start).AsOpenChainEnd(false);
                }
                else
                {
                    target._lines[^1] = target._lines[^1].AsOpenChainEnd(true);
                }
            }

            public static void Invert(DoubleLinkedLineList container)
            {
                Undo.RecordObject(container, nameof(Invert));

                var newLines = new List<Line>(InitialLinesCapacity);
                int count = container.Count;
                for (int i = 0; i < count; i++)
                {
                    int invertedIndex = count - i - 1;
                    newLines.Add(container[invertedIndex].Invert());
                }

                if (!container.Loop)
                {
                    newLines[0] = newLines[0].AsOpenChainEnd(false);
                    newLines[^1] = newLines[^1].AsOpenChainEnd(true);
                }

                container._lines = newLines;
            }

            public static void RebuildLineColliders(DoubleLinkedLineList target)
            {
                Debug.Assert(target._colliderContainer != null);
                Debug.Assert(target._colliderCache != null);
                Debug.Assert(target._colliderCache.Prefab != null);

                Transform container = target._colliderContainer;
                while (container.childCount > 0)
                {
                    DestroyImmediate(container.GetChild(0).gameObject);
                }

                target._colliders.Clear();

                for (int index = 0; index < target.Count; index++)
                {
                    if (target[index].Direction != GridDirection.None)
                    {
                        LineCollider lineCollider = Instantiate(target._colliderCache.Prefab, container);
                        lineCollider.Set(target, index);
                    }
                }
            }
        }
#endif
    }
}