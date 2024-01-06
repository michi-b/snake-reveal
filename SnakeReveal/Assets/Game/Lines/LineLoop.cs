using System;
using System.Collections.Generic;
using System.Linq;
using Extensions;
using Game.Enums;
using JetBrains.Annotations;
using UnityEditor;
using UnityEngine;

namespace Game.Lines
{
    public class LineLoop : LineContainer
    {
        private const int ChainInsertionLineCapacity = 1000;

        public const string TurnFieldName = nameof(_turn);

        [SerializeField, HideInInspector] private Turn _turn;

        private readonly ChainInsertionData _chainInsertionData = new(ChainInsertionLineCapacity);

        private readonly Collider2D[] _findLinesBuffer = new Collider2D[2];

        protected override Color GizmosColor => new(0f, 0.5f, 1f);
        public override bool Loop => true;
        public override Line End => Start.Previous;
        public Turn Turn => _turn;

        protected override void PostProcessEditModeLineChanges()
        {
            base.PostProcessEditModeLineChanges();

            _turn = ClockwiseTurnWeight switch
            {
                4 => Turn.Right,
                -4 => Turn.Left,
                _ => Turn.None
            };
        }

        protected override int EvaluateClockwiseTurnWeight()
        {
            return base.EvaluateClockwiseTurnWeight() + End.Direction.GetTurn(Start.Direction).GetClockwiseWeight();
        }

        public void Insert(LineChain target)
        {
            EvaluateInsertion(target, _chainInsertionData);

            Line breakoutLine = _chainInsertionData.BreakoutLine;
            Line breakInLine = _chainInsertionData.BreakInLine;
            List<LineData> linesToInsert = _chainInsertionData.LinesToInsert;

            if (breakoutLine == breakInLine)
            {
                // split breakout line into separate breakout and break-in line
                breakInLine = GetNewLine(new LineData(linesToInsert[^1].End, breakoutLine.End, breakoutLine.Direction));

                breakInLine.Next = breakoutLine.Next;
                breakoutLine.Next!.Previous = breakInLine;

                breakoutLine.End = linesToInsert[0].Start;
                breakoutLine.Next = null;
            }
            else
            {
                if (breakoutLine.Next != breakInLine)
                {
                    //remove in-between lines
                    foreach (Line inBetweenLine in new LineSpan(breakoutLine.Next, breakInLine.Previous))
                    {
                        LineCache.Return(inBetweenLine);
                    }
                }

                breakoutLine.Next = null;
                breakInLine.Previous = null;
            }

            // last inserted line to connect next inserted line to
            Line lastLine = breakoutLine;
            // create in-between lines and stitch it all together
            // ReSharper disable once ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator
            // no LINQ to avoid allocations
            foreach (LineData lineData in linesToInsert)
            {
                Line line = GetNewLine(lineData);
                lastLine.Next = line;
                line.Previous = lastLine;
            }

            lastLine.Next = breakoutLine;
            breakoutLine.Previous = lastLine;

            // remove superfluous connection lines that can happen when connecting exactly to loop corners
            if (breakoutLine.Start == breakoutLine.End)
            {
                DissolveZeroLengthLine(breakoutLine);
            }

            if (breakInLine.Start == breakInLine.End)
            {
                DissolveZeroLengthLine(breakInLine);
                breakInLine = breakInLine.Previous!;
            }

            _start = breakInLine;

            // assumes the given line has zero lenght, and that the line before and after have the same direction
            // -> merge the three lines into one
            // by removing the given line and the line after, and connecting the line before to the line after the (formerly) three lines
            void DissolveZeroLengthLine(Line line)
            {
                Line next = line.Next;
                Line previous = line.Previous;
#if DEBUG
                Debug.Assert(Start == End);
                Debug.Assert(previous != null && line.Next != null && line.Next.Next != null);
                Debug.Assert(line.Next!.Direction == previous.Direction);
#endif
                Destroy(gameObject);
                Destroy(line.Next.gameObject);
                previous.End = line.Next.Start;
                Line newNext = next.Next;
#if DEBUG
                Debug.Assert(newNext != null);
#endif
                previous.Next = newNext;
            }

            Start = breakInLine;
        }

        private Line GetNewLine(LineData lineData)
        {
            Line line = LineCache.Get();
            line.Set(lineData);
            return line;
        }

        private void EvaluateInsertion(LineChain insertTarget, ChainInsertionData result)
        {
            Line chainStart = insertTarget.Start;
            GridDirection breakoutDirection = chainStart.Direction.Turn(_turn);
            result.BreakoutPosition = chainStart.Start;
            result.BreakoutLine = FindLine(result.BreakoutPosition, breakoutDirection);
            Debug.Assert(result.BreakoutLine != null, "No suitable breakout line on loop.");

            Line chainEnd = insertTarget.Last();
            GridDirection breakInDirection = chainEnd.Direction.Turn(_turn.Reverse());
            result.BreakInPosition = chainEnd.End;
            result.BreakInLine = FindLine(result.BreakInPosition, breakInDirection);
            Debug.Assert(result.BreakInLine != null, "No suitable break-in on loop.");

            int loopTurnClockwiseWeight = new LineSpan(result.BreakoutLine, result.BreakInLine).SumClockwiseTurnWeight();
            int chainTurnClockwiseWeight = insertTarget.AsSpan(true).SumClockwiseTurnWeight();
            int deltaClockwiseWeight = chainTurnClockwiseWeight - loopTurnClockwiseWeight;

            bool reconnectsInTurn = Turn switch
            {
                Turn.None => throw new ArgumentOutOfRangeException(),
                Turn.Right => deltaClockwiseWeight > 0,
                Turn.Left => deltaClockwiseWeight < 0,
                _ => throw new ArgumentOutOfRangeException()
            };

#if DEBUG
            Debug.Log(reconnectsInTurn ? "Chain reconnects IN turn" : "Chain reconnects COUNTER turn");
#endif

            result.LinesToInsert.Clear();

            if (reconnectsInTurn)
            {
                foreach (Line line in insertTarget)
                {
                    result.LinesToInsert.Add(line.DataStruct);
                }
            }
            else
            {
                (result.BreakoutLine, result.BreakInLine) = (result.BreakInLine, result.BreakoutLine);
                foreach (Line line in insertTarget.AsReverseSpan())
                {
                    result.LinesToInsert.Add(line.DataStruct.Reverse());
                }
            }
        }

        [CanBeNull]
        private Line FindLine(Vector2Int position, GridDirection direction)
        {
            foreach (Collider2D lineCollider in FindColliders(position))
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

        private Span<Collider2D> FindColliders(Vector2Int position)
        {
            var contactFilter = new ContactFilter2D
            {
                useLayerMask = true,
                layerMask = LayerMask
            };

            int count = Physics2D.OverlapPoint(position.GetScenePosition(Grid), contactFilter, _findLinesBuffer);

#if DEBUG
            Debug.Assert(count <= _findLinesBuffer.Length, $"Overlap point count {count} is larger than collider buffer size {_findLinesBuffer.Length}, " +
                                                           " which should not be possible by design (usage of layers and avoiding overlapping lines).");
#endif

            return new Span<Collider2D>(_findLinesBuffer, 0, count);
        }

        private class ChainInsertionData
        {
            // sorted lines to insert into loop between breakout line and break-in line
            // be wary of that the breakout line and break-in line might be the same
            public readonly List<LineData> LinesToInsert;
            public Line BreakInLine; // break-in line on loop
            public Vector2Int BreakInPosition;

            public Line BreakoutLine; // breakout line on loop
            public Vector2Int BreakoutPosition;

            public ChainInsertionData(int chainInsertionInitialCapacity)
            {
                LinesToInsert = new List<LineData>(chainInsertionInitialCapacity);
            }
        }

        #region Edit Mode Utility

        public void EditModeInsert(LineChain insertTarget)
        {
            Undo.IncrementCurrentGroup();
            int insertionUndoGroup = Undo.GetCurrentGroup();

            Undo.RegisterFullObjectHierarchyUndo(gameObject, nameof(EditModeInsert));

            EvaluateInsertion(insertTarget, _chainInsertionData);

            Line breakoutLine = _chainInsertionData.BreakoutLine;
            Line breakInLine = _chainInsertionData.BreakInLine;


            if (breakoutLine == breakInLine)
            {
                // split breakout line in two to get a separate break in line, which is necessary to insert the chain in between
                breakInLine = EditModeInstantiateLine(breakoutLine.Start, breakoutLine.End, true);
                Undo.IncrementCurrentGroup();
                breakInLine.EditModeStitchToNext(breakoutLine.Next);
            }
            else if (breakoutLine.Next != breakInLine)
            {
                foreach (Line inBetweenLine in new LineSpan(breakoutLine.Next, breakInLine.Previous))
                {
                    Undo.DestroyObjectImmediate(inBetweenLine.gameObject);
                }
            }

            breakoutLine.RegisterUndo(nameof(EditModeInsert) + " - disconnect breakout line next");
            breakoutLine.Next = null;

            breakInLine.RegisterUndo(nameof(EditModeInsert) + " - disconnect break-in line previous");
            breakInLine.Previous = null;

            List<LineData> linesToInsert = _chainInsertionData.LinesToInsert;

            // adjust breakout and break-in line positions
            breakInLine.Start = linesToInsert[^1].End;

            // insert lines
            Line lastLine = breakoutLine;
            foreach (LineData line in linesToInsert)
            {
                Line newLine = EditModeInstantiateLine(line.Start, line.End, true);
                Undo.IncrementCurrentGroup();
                lastLine.EditModeStitchToNext(newLine);
                lastLine = lastLine.Next!;
            }

            lastLine.EditModeStitchToNext(breakInLine);

            // remove superfluous connection lines that can happen when connecting exactly to loop corners
            if (breakoutLine.Start == breakoutLine.End)
            {
                EditModeDissolve(breakoutLine);
            }

            if (breakInLine.Start == breakInLine.End)
            {
                Line dissolvingBreakInLine = breakInLine;
                breakInLine = dissolvingBreakInLine!.Previous;
                EditModeDissolve(dissolvingBreakInLine);
            }

            // start may have been deleted, so it is re-assigned
            Undo.RecordObject(this, nameof(EditModeInsert) + " - assign new start");
            _start = breakInLine;

            Undo.CollapseUndoOperations(insertionUndoGroup);
        }

        private static void EditModeDissolve(Line line)
        {
            Line previous = line.Previous;
            Line next = line.Next!;
            Line nextNext = next.Next;
            Undo.DestroyObjectImmediate(line.gameObject);
            Undo.DestroyObjectImmediate(next.gameObject);
            previous!.EditModeStitchToNext(nextNext);
        }

        #endregion
    }
}