using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace Game.Lines
{
    public readonly struct LineSpan : IEnumerable<Line>
    {
        private readonly Line _forcedIncludedBegin;
        private readonly Line _exclusiveEnd;

        /// <summary>
        ///     Represents a span of lines from start to end,
        ///     excluding <see cref="exclusiveEnd" />,
        ///     except it is the same as <see cref="forcedIncludedBegin" />.
        /// </summary>
        public LineSpan(Line forcedIncludedBegin, [CanBeNull] Line exclusiveEnd)
        {
            _forcedIncludedBegin = forcedIncludedBegin;
            _exclusiveEnd = exclusiveEnd;
        }

        public LineEnumerator GetEnumerator()
        {
            // ReSharper disable once Unity.NoNullPropagation
            return new LineEnumerator(_forcedIncludedBegin, _exclusiveEnd);
        }

        IEnumerator<Line> IEnumerable<Line>.GetEnumerator()
        {
            return GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}