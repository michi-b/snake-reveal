using System;
using System.Collections.Generic;
using System.Diagnostics;
using Game.Enums;
using JetBrains.Annotations;
using UnityEngine;

namespace Game.Lines
{
    public class LineLoop : LineContainer
    {
        public const string TurnFieldName = nameof(_turn);

        [SerializeField, HideInInspector] private Turn _turn;

        private readonly InsertionEvaluation _insertionEvaluation = new();

        protected override Color GizmosColor => new(0f, 0.5f, 1f);
        public override bool Loop => true;

        // ReSharper disable once Unity.NoNullPropagation
        public override Line End => Start?.Previous;
        public Turn Turn => _turn;

        public Turn GetTurn(bool startToEnd = true)
        {
            return startToEnd ? Turn : Turn.Reverse();
        }

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

        /// <summary>
        ///     Inserts the target line chain into this loop, without altering the target line chain.
        ///     Also shifts the <see cref="LineLoop.Start" /> to contain the break-in position.
        /// </summary>
        /// <param name="lineChain">The line chain to insert</param>
        /// <param name="breakoutLine">The line on this loop that the line chain originates at</param>
        /// <param name="reinsertionLine">The lin on this loop that the lin chain ends at</param>
        /// <returns>Whether the chain reconnects to this loop in the turn of this loop</returns>
        public InsertionResult Insert(LineChain lineChain, [NotNull] Line breakoutLine, [NotNull] Line reinsertionLine)
        {
            _insertionEvaluation.Evaluate(_turn, lineChain, breakoutLine, reinsertionLine);
            breakoutLine = _insertionEvaluation.BreakoutLine;
            reinsertionLine = _insertionEvaluation.ReInsertionLine;
            List<LineData> linesToInsert = _insertionEvaluation.LinesToInsert;

            if (breakoutLine == reinsertionLine)
            {
                // crate new line after chain
                reinsertionLine = GetNewLine(reinsertionLine.Data);
                reinsertionLine.Next = breakoutLine.Next;
                breakoutLine.Next!.Previous = reinsertionLine;
            }
            else
            {
                if (breakoutLine.Next != reinsertionLine)
                {
                    //remove in-between lines
                    foreach (Line inBetweenLine in new LineSpan(breakoutLine.Next, reinsertionLine.Previous))
                    {
                        LineCache.Return(inBetweenLine);
                    }
                }
            }

            breakoutLine.Next = null;
            breakoutLine.End = linesToInsert[0].Start;
            reinsertionLine.Previous = null;
            reinsertionLine.Start = linesToInsert[^1].End;

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

                lastLine = line;
            }

            lastLine.Next = reinsertionLine;
            reinsertionLine.Previous = lastLine;

            // remove superfluous connection lines that can happen when connecting exactly to loop corners
            bool brokeOutOnCorner = breakoutLine.Direction == GridDirection.None;
            if (brokeOutOnCorner)
            {
                breakoutLine = DissolveZeroLengthLine(breakoutLine);
            }

            bool reconnectedOnCorner = reinsertionLine.Direction == GridDirection.None;
            if (reconnectedOnCorner)
            {
                reinsertionLine = DissolveZeroLengthLine(reinsertionLine);
            }

            // start line may have been eliminated -> just set it to the line the chain "flows" into
            Line nextLineAfterInsertion = _insertionEvaluation.IsStartToEnd
                ? reinsertionLine
                : breakoutLine;

            Start = nextLineAfterInsertion;

            Line continuation = _insertionEvaluation.IsStartToEnd
                ? reconnectedOnCorner
                    ? nextLineAfterInsertion
                    : nextLineAfterInsertion.Previous
                : brokeOutOnCorner
                    ? nextLineAfterInsertion
                    : nextLineAfterInsertion.Next;

            return new InsertionResult(continuation, _insertionEvaluation.IsStartToEnd);

            // assumes the given line has zero lenght, and that the line before and after have the same direction
            // -> merge the three lines into one and return the merged line
            // by removing the given line and the line after, and connecting the line before to the line after the (formerly) three lines
            Line DissolveZeroLengthLine(Line line)
            {
                Line previous = line.Previous;
                Line next = line.Next!;
                Line newNext = next.Next!;
#if DEBUG
                if (line.Direction != GridDirection.None
                    || previous == null
                    || next == null
                    || line.Next!.Direction != previous.Direction)
                {
                    throw new ArgumentException(nameof(line), nameof(line));
                }
#endif
                Destroy(line.gameObject);
                Destroy(next.gameObject);

                previous.End = newNext.Start;
                previous.Next = newNext;
                newNext.Previous = previous;

                return previous;
            }
        }
    }
}