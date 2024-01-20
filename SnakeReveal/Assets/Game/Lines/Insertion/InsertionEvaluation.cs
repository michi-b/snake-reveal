using System;
using System.Collections.Generic;
using Game.Enums;
using UnityEngine;

namespace Game.Lines.Insertion
{
    public class InsertionEvaluation
    {
        private const int DefaultCapacity = 1000;

        // lines on the original loop in reverse direction that complete the lines to insert into a new loop (for quads/surface fill generation)
        private readonly List<LineData> _insertionConnection;

        // sorted lines to insert into loop between breakout line and break-in line
        // be wary of that the breakout line and break-in line might be the same
        private readonly List<LineData> _linesToInsert;

        public InsertionEvaluation(int capacity = DefaultCapacity)
        {
            _linesToInsert = new List<LineData>(capacity);
            _insertionConnection = new List<LineData>(capacity);
            BreakoutLine = default;
            ReInsertionLine = default;
            IsStartToEnd = default;
        }

        public IReadOnlyList<LineData> LinesToInsert => _linesToInsert;
        public Line ReInsertionLine { get; private set; } // break-in line on loop
        public Line BreakoutLine { get; private set; } // breakout line on loop
        public bool IsStartToEnd { get; private set; }

        public Turn Turn { get; private set; }

        /// <summary>
        ///     Get an enumerable loops view that represents the insertion, reconnecting the inserted chain along the loop in
        ///     reverse direction.
        ///     Note that this is only a view of the insertion evaluation, and caches no additional data for performance reason.
        ///     Therefore updating the insertion evaluation will also update this loop view.
        /// </summary>
        /// <value>A struct that can be enumerated to get the sorted line data of the loop</value>
        public InsertionLoopView Loop => new(this);

        /// <summary>
        ///     Clear any previous results and regenerate all results from the input
        /// </summary>
        /// <param name="loopTurn">The turn of the loop</param>
        /// <param name="chain">The target chain, whose line data shall be inserted into the loop</param>
        /// <param name="loopBreakoutLine">The line on the loop that the target chain originates at</param>
        /// <param name="loopReinsertionLine">The line on the loop that the target chain reinserts into</param>
        /// <exception cref="ArgumentException">
        ///     When the chain start position is non on the breakout line or
        ///     the chain end position is non on the reinsertion line
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">When the loopTurn is NONE</exception>
        public void Evaluate(Turn loopTurn, LineChain chain, Line loopBreakoutLine, Line loopReinsertionLine)
        {
            Turn = loopTurn;
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

            IsStartToEnd = Turn switch
            {
                Turn.None => throw new ArgumentOutOfRangeException(),
                Turn.Right => deltaClockwiseWeight > 0,
                Turn.Left => deltaClockwiseWeight < 0,
                _ => throw new ArgumentOutOfRangeException()
            };

            // fill lines to insert, reversing the chain and swapping breakout/reconnect if the reconnection is not in loop turn            

            _linesToInsert.Clear();

            if (IsStartToEnd)
            {
                foreach (Line line in chain)
                {
                    _linesToInsert.Add(line.Data);
                }
            }
            else
            {
                (BreakoutLine, ReInsertionLine) = (ReInsertionLine, BreakoutLine);
                // ReSharper disable once ForeachCanBeConvertedToQueryUsingAnotherGetEnumerator
                // no LINQ to avoid allocations
                foreach (Line line in chain.AsReverseSpan())
                {
                    _linesToInsert.Add(line.Data.Reverse());
                }
            }

            // create lines backwards along the original loop that complete the lines to insert into a new loop (for quads/surface fill generation)
            // note that at this point, lines to insert as well as breakout line and reinsertion line are corrected to be guaranteed in loop turn

            _insertionConnection.Clear();

            Vector2Int chainStartPosition = _linesToInsert[0].Start;
            Vector2Int chainEndPosition = _linesToInsert[^1].End;
            if (BreakoutLine == ReInsertionLine)
            {
                _insertionConnection.Add(new LineData(chainEndPosition, chainStartPosition));
            }
            else
            {
                _insertionConnection.Add(new LineData(chainEndPosition, ReInsertionLine.Start));
                if (BreakoutLine != ReInsertionLine.Previous)
                {
                    foreach (Line line in new ReverseLineSpan(BreakoutLine.Next, ReInsertionLine.Previous))
                    {
                        _insertionConnection.Add(line.Data.Reverse());
                    }
                }

                _insertionConnection.Add(new LineData(BreakoutLine.End, chainStartPosition));
            }
        }

        public readonly struct InsertionLoopView
        {
            private readonly InsertionEvaluation _evaluation;

            public Turn Turn => _evaluation.Turn;

            public InsertionLoopView(InsertionEvaluation evaluation)
            {
                _evaluation = evaluation;
            }
            
            public InsertionLoopEnumerator GetEnumerator()
            {
                return new InsertionLoopEnumerator(_evaluation._linesToInsert, _evaluation._insertionConnection);
            }
        }
    }
}