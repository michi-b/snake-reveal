using System.Collections;
using System.Collections.Generic;
using Game.Enums;
using JetBrains.Annotations;

namespace Game.Lines
{
    public readonly struct LineSpan : IEnumerable<Line>
    {
        private readonly Line _forcedIncludedBegin;
        private readonly Line _exclusiveEnd;
        private readonly bool _loop;

        /// <inheritdoc cref="LineEnumerator(Line, Line, bool)" />
        public LineSpan(Line forcedIncludedBegin, [CanBeNull] Line exclusiveEnd, bool loop = false)
        {
            _forcedIncludedBegin = forcedIncludedBegin;
            _exclusiveEnd = exclusiveEnd;
            _loop = loop;
        }

        public LineEnumerator GetEnumerator()
        {
            // ReSharper disable once Unity.NoNullPropagation
            return new LineEnumerator(_forcedIncludedBegin, _exclusiveEnd, _loop);
        }

        IEnumerator<Line> IEnumerable<Line>.GetEnumerator()
        {
            return GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public int SumClockwiseTurnWeight(Turn turn)
        {
            int result = 0;
            // ReSharper disable once ForeachCanBeConvertedToQueryUsingAnotherGetEnumerator
            // performance critical code -> avoid LINQ boxing allocation
            foreach (Line line in this)
            {
                result += line.Direction.GetTurn(line.Next!.Direction).GetClockwiseWeight();
            }

            return result;
        }
    }
}