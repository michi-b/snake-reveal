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

        public LineSpan AdvanceEnd(int count = 1)
        {
            Line newEnd = _exclusiveEnd;
            for (int i = 0; i < count; i++)
            {
                newEnd = newEnd!.Next;
            }

            return new LineSpan(_forcedIncludedBegin, newEnd, _loop);
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

        public int SumClockwiseTurnWeight()
        {
            int result = 0;

            // ReSharper disable once ForeachCanBeConvertedToQueryUsingAnotherGetEnumerator
            // avoid LINQ to avoid boxing allocation
            foreach (Line line in this)
            {
                Line next = line.Next;
                if (ReferenceEquals(next, null))
                {
                    return result;
                }
                
                result += line.Direction.GetTurn(next.Direction).GetClockwiseWeight();
            }

            return result;
        }
    }
}