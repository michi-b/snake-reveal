using System;
using System.Collections.Generic;
using Game.Enums;

namespace Game.Lines
{
    public class InsertionEvaluation
    {
        private const int DefaultInsertionLineCapacity = 1000;

        // sorted lines to insert into loop between breakout line and break-in line
        // be wary of that the breakout line and break-in line might be the same
        public readonly List<LineData> LinesToInsert;

        public InsertionEvaluation(int chainInsertionInitialCapacity = DefaultInsertionLineCapacity)
        {
            LinesToInsert = new List<LineData>(chainInsertionInitialCapacity);
            BreakoutLine = default;
            ReInsertionLine = default;
            IsStartToEnd = default;
        }

        public Line ReInsertionLine { get; private set; } // break-in line on loop
        public Line BreakoutLine { get; private set; } // breakout line on loop
        public bool IsStartToEnd { get; private set; }

        public void Evaluate(Turn loopTurn, LineChain chain, Line loopBreakoutLine, Line loopReinsertionLine)
        {
#if DEBUG
            if (!loopBreakoutLine.Contains(chain.Start.Start))
            {
                throw new ArgumentException("Breakout line does not contain chain start");
            }

            if (!loopReinsertionLine.Contains(chain.End.End))
            {
                throw new ArgumentException("Reinsertion line does not contain chain end");
            }
#endif
            ReInsertionLine = loopReinsertionLine;
            BreakoutLine = loopBreakoutLine;

            int loopTurnClockwiseWeight = new LineSpan(loopBreakoutLine, loopReinsertionLine).SumClockwiseTurnWeight();
            int chainTurnClockwiseWeight = chain.AsSpan().SumClockwiseTurnWeight();
            int deltaClockwiseWeight = chainTurnClockwiseWeight - loopTurnClockwiseWeight;

            IsStartToEnd = loopTurn switch
            {
                Turn.None => throw new ArgumentOutOfRangeException(),
                Turn.Right => deltaClockwiseWeight > 0,
                Turn.Left => deltaClockwiseWeight < 0,
                _ => throw new ArgumentOutOfRangeException()
            };

            LinesToInsert.Clear();

            if (IsStartToEnd)
            {
                foreach (Line line in chain)
                {
                    LinesToInsert.Add(line.Data);
                }
            }
            else
            {
                (BreakoutLine, ReInsertionLine) = (ReInsertionLine, BreakoutLine);
                // ReSharper disable once ForeachCanBeConvertedToQueryUsingAnotherGetEnumerator
                // no LINQ to avoid allocations
                foreach (Line line in chain.AsReverseSpan())
                {
                    LinesToInsert.Add(line.Data.Reverse());
                }
            }
        }
    }
}