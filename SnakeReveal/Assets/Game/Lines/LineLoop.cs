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

        public void EditModeInsert(LineChain insertTarget)
        {
            Undo.IncrementCurrentGroup();
            int insertionUndoGroup = Undo.GetCurrentGroup();

            Undo.RegisterFullObjectHierarchyUndo(gameObject, nameof(EditModeInsert));

            EvaluateChainInsertion(insertTarget, _chainInsertionData);

            Line breakoutLine = _chainInsertionData.BreakoutLine;
            Line breakInLine = _chainInsertionData.BreakInLine;

            
            if (breakoutLine == breakInLine)
            {
                // split breakout line in two to get a separate break in line, which is necessary to insert the chain in between
                breakInLine = EditModeInstantiateLine(breakoutLine.Start, breakoutLine.End, true);
                Undo.IncrementCurrentGroup();
                
                breakInLine.RegisterUndo(nameof(EditModeInsert) + " - connect break-in line to next");
                breakoutLine.Next!.RegisterUndo(nameof(EditModeInsert) + " - connect line after break-in to previous");
                breakInLine.StitchToNext(breakoutLine.Next);
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
            for (int i = 0; i < linesToInsert.Count; i++)
            {
                Line newLine = EditModeInstantiateLine(linesToInsert[i].Start, linesToInsert[i].End, true);
                Undo.IncrementCurrentGroup();
                lastLine!.RegisterUndo(nameof(EditModeInsert) + " - connect previous line to inserted");
                newLine!.RegisterUndo(nameof(EditModeInsert) + " - connect inserted line to previous");
                lastLine!.StitchToNext(newLine);
                lastLine = lastLine.Next;
            }

            lastLine!.RegisterUndo(nameof(EditModeInsert) + " - connect last inserted to break-in line");
            breakInLine!.RegisterUndo(nameof(EditModeInsert) + " - connect break-in line to last inserted");
            lastLine!.StitchToNext(breakInLine);
            // start may have been deleted, so it is re-assigned
            Undo.RecordObject(this, nameof(EditModeInsert) + " - assign new start");
            _start = breakInLine;

            Undo.CollapseUndoOperations(insertionUndoGroup);
        }

        private void EvaluateChainInsertion(LineChain insertTarget, ChainInsertionData result)
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

            Debug.Log(reconnectsInTurn ? "Chain reconnects IN turn" : "Chain reconnects COUNTER turn");

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
    }
}