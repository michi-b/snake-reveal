using System.Collections.Generic;
using Game.Enums;
using JetBrains.Annotations;
using UnityEngine;

namespace Game.Lines
{
    public class LineLoop : LineContainer
    {
        public const string TurnFieldName = nameof(_turn);

        [SerializeField, HideInInspector] private Turn _turn;

        private readonly ChainInsertionEvaluation _chainInsertionEvaluation = new ChainInsertionEvaluation();

        protected override Color GizmosColor => new(0f, 0.5f, 1f);
        public override bool Loop => true;

        // ReSharper disable once Unity.NoNullPropagation
        public override Line End => Start?.Previous;
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

        /// <summary>
        ///     Inserts the target line chain into this loop, without altering the target line chain.
        ///     Also shifts the <see cref="LineLoop.Start" /> to contain the break-in position.
        /// </summary>
        /// <param name="lineChain">The line chain to insert</param>
        /// <param name="beforeChain">The line on this loop that the line chain originates at</param>
        /// <param name="afterChain">The lin on this loop that the lin chain ends at</param>
        /// <returns>Whether the chain reconnects to this loop in the turn of this loop</returns>
        public bool Insert(LineChain lineChain, [NotNull] Line beforeChain, [NotNull] Line afterChain)
        {
            _chainInsertionEvaluation.Evaluate(_turn, lineChain, beforeChain, afterChain);
            beforeChain = _chainInsertionEvaluation.BreakoutLine;
            afterChain = _chainInsertionEvaluation.BreakInLine;
            List<LineData> linesToInsert = _chainInsertionEvaluation.LinesToInsert;

            if (beforeChain == afterChain)
            {
                // crate new line after chain
                afterChain = GetNewLine(afterChain.Data);
                afterChain.Next = beforeChain.Next;
                beforeChain.Next!.Previous = afterChain;
            }
            else
            {
                if (beforeChain.Next != afterChain)
                {
                    //remove in-between lines
                    foreach (Line inBetweenLine in new LineSpan(beforeChain.Next, afterChain.Previous))
                    {
                        LineCache.Return(inBetweenLine);
                    }
                }
            }

            beforeChain.Next = null;
            beforeChain.End = linesToInsert[0].Start;
            afterChain.Previous = null;
            afterChain.Start = linesToInsert[^1].End;

            // last inserted line to connect next inserted line to
            Line lastLine = beforeChain;
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

            lastLine.Next = afterChain;
            afterChain.Previous = lastLine;

            // remove superfluous connection lines that can happen when connecting exactly to loop corners
            if (beforeChain.Start == beforeChain.End)
            {
                DissolveZeroLengthLine(beforeChain);
                beforeChain = beforeChain.Previous!;
            }

            if (afterChain.Start == afterChain.End)
            {
                DissolveZeroLengthLine(afterChain);
                afterChain = afterChain.Previous!;
            }

            Start = _chainInsertionEvaluation.ReconnectsInTurn ? afterChain : beforeChain;
            return _chainInsertionEvaluation.ReconnectsInTurn;

            // assumes the given line has zero lenght, and that the line before and after have the same direction
            // -> merge the three lines into one
            // by removing the given line and the line after, and connecting the line before to the line after the (formerly) three lines
            void DissolveZeroLengthLine(Line line)
            {
                Line next = line.Next;
                Line previous = line.Previous;
#if DEBUG
                Debug.Assert(Start == End);
                Debug.Assert(previous != null && next != null);
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
        }
    }
}