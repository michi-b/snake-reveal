﻿using System.Collections.Generic;
using Game.Enums;
using UnityEditor;

namespace Game.Lines
{
    public partial class LineContainer
    {
        public static class EditModeUtility
        {
            public const string LinesPropertyName = nameof(_lines);
            public const string LoopPropertyName = nameof(_loop);
            public const string ClockwiseTurnWeightPropertyName = nameof(_clockwiseTurnWeight);

            public static void EditModeReevaluateClockwiseTurnWeight(LineContainer target)
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

            public static void RebuildLineRenderers(LineContainer target)
            {
                foreach (LineChainRenderer lineChainRenderer in target._lineRenderers)
                {
                    lineChainRenderer.EditModeRebuild(target._lines);
                }
            }

            public static void FixLines(LineContainer target)
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

            public static void Invert(LineContainer container)
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

            public static void RebuildLineColliders(LineContainer container)
            {
                while (container._colliderContainer)
                {
                    DestroyImmediate(container._colliderContainer.gameObject);
                }
            }
        }
    }
}