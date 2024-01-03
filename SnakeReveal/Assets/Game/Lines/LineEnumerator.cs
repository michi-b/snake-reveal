using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace Game.Lines
{
    public struct LineEnumerator : IEnumerator<Line>
    {
        private Line _forcedIncludedBegin;
        private Line _exclusiveEnd;

        /// <summary>
        ///     Enumerates the lines from start to end, EXCLUDING end.
        ///     It will NOT yield anything if <see cref="forcedIncludedBegin" /> is NULL.
        ///     Otherwise, if <see cref="forcedIncludedBegin" /> and <see cref="exclusiveEnd" /> are the same,
        ///     the beginning line is still yielded.
        /// </summary>
        /// <param name="forcedIncludedBegin">
        ///     The first Line this enumerator will yield.
        ///     It is also yielded if it is the same as <see cref="exclusiveEnd" />.
        /// </param>
        /// <param name="exclusiveEnd">
        ///     The line after the last line the enumerator should yield.
        ///     NULL is a valid choice for an open-ended line chain.
        ///     If it is not NULL but the same as, it is yielded.
        /// </param>
        public LineEnumerator(Line forcedIncludedBegin, [CanBeNull] Line exclusiveEnd)
        {
            _forcedIncludedBegin = forcedIncludedBegin;
            _exclusiveEnd = exclusiveEnd;
            Current = null;
        }

        public bool MoveNext()
        {
            // first iteration case
            if (ReferenceEquals(Current, null))
            {
                Current = _forcedIncludedBegin;
                return !ReferenceEquals(Current, null);
            }

            Current = Current.Next;
            return !ReferenceEquals(Current, _exclusiveEnd);
        }

        public void Reset()
        {
            Current = null;
        }

        public Line Current { get; private set; }

        object IEnumerator.Current => Current;

        public void Dispose()
        {
            _forcedIncludedBegin = null;
            _exclusiveEnd = null;
        }
    }
}