using System;
using System.Collections.Generic;
using Game.Enums;
using UnityEngine;

namespace Game.Lines
{
    public struct ChainInsertionEvaluation
    {
        // sorted lines to insert into loop between breakout line and break-in line
        // be wary of that the breakout line and break-in line might be the same
        public readonly List<LineData> LinesToInsert;
        public Line BreakInLine { get; private set; } // break-in line on loop
        public Line BreakoutLine { get; private set; } // breakout line on loop
        public bool ReconnectsInTurn { get; private set; }

        public ChainInsertionEvaluation(int chainInsertionInitialCapacity)
        {
            LinesToInsert = new List<LineData>(chainInsertionInitialCapacity);
            BreakoutLine = default;
            BreakInLine = default;
            ReconnectsInTurn = default;
        }

        public void Evaluate(Turn loopTurn, LineChain insertTarget, Line loopBreakoutLine, Line loopBreakInLine)
        {
#if DEBUG
            Debug.Assert(loopBreakoutLine.Contains(insertTarget.Start.Start));
            Debug.Assert(loopBreakInLine.Contains(insertTarget.End.End));
#endif
            BreakInLine = loopBreakInLine;
            BreakoutLine = loopBreakoutLine;

            int loopTurnClockwiseWeight = new LineSpan(loopBreakoutLine, loopBreakInLine).SumClockwiseTurnWeight();
            int chainTurnClockwiseWeight = insertTarget.AsSpan(true).SumClockwiseTurnWeight();
            int deltaClockwiseWeight = chainTurnClockwiseWeight - loopTurnClockwiseWeight;

            ReconnectsInTurn = loopTurn switch
            {
                Turn.None => throw new ArgumentOutOfRangeException(),
                Turn.Right => deltaClockwiseWeight > 0,
                Turn.Left => deltaClockwiseWeight < 0,
                _ => throw new ArgumentOutOfRangeException()
            };

            LinesToInsert.Clear();

            if (ReconnectsInTurn)
            {
                foreach (Line line in insertTarget)
                {
                    LinesToInsert.Add(line.Data);
                }
            }
            else
            {
                (BreakoutLine, BreakInLine) = (BreakInLine, BreakoutLine);
                // ReSharper disable once ForeachCanBeConvertedToQueryUsingAnotherGetEnumerator
                // no LINQ to avoid allocations
                foreach (Line line in insertTarget.AsReverseSpan())
                {
                    LinesToInsert.Add(line.Data.Reverse());
                }
            }
        }
    }
}