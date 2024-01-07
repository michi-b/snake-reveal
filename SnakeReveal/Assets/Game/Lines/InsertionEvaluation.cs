using System;
using System.Collections.Generic;
using Game.Enums;
using UnityEngine;

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
            ReconnectsInTurn = default;
        }

        public Line ReInsertionLine { get; private set; } // break-in line on loop
        public Line BreakoutLine { get; private set; } // breakout line on loop
        public bool ReconnectsInTurn { get; private set; }

        public void Evaluate(Turn loopTurn, LineChain chain, Line loopBreakoutLine, Line loopReinsertionLine)
        {
#if DEBUG
            Debug.Assert(loopBreakoutLine.Contains(chain.Start.Start));
            Debug.Assert(loopReinsertionLine.Contains(chain.End.End));
#endif
            ReInsertionLine = loopReinsertionLine;
            BreakoutLine = loopBreakoutLine;

            int loopTurnClockwiseWeight = new LineSpan(loopBreakoutLine, loopReinsertionLine).SumClockwiseTurnWeight();
            int chainTurnClockwiseWeight = chain.AsSpan().SumClockwiseTurnWeight();
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