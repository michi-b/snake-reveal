using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace Game.Lines
{
    public struct LineEnumerator : IEnumerator<Line>
    {
        private Line _begin;
        private Line _exclusiveEnd;
        private readonly bool _loop;

        /// <summary>
        ///     Enumerates the lines from start to end, EXCLUDING <see cref="exclusiveEnd" />.
        ///     It will NOT yield anything if <see cref="begin" /> is NULL.
        /// </summary>
        /// <param name="begin">
        ///     The first Line this enumerator should yield
        /// </param>
        /// <param name="exclusiveEnd">
        ///     The line after the last line the enumerator should yield.
        ///     NULL is a valid choice for an open-ended line chain.
        /// </param>
        /// <param name="loop">
        ///     Just in case <see cref="begin" /> is the same as <see cref="exclusiveEnd" />,
        ///     should the line list be assumed to be loop and yield all lines (otherwise none)
        /// </param>
        /// <remarks>Bypasses Unity lifecycle checks for performance reasons.</remarks>
        public LineEnumerator(Line begin, [CanBeNull] Line exclusiveEnd, bool loop)
        {
            _begin = begin;
            _exclusiveEnd = exclusiveEnd;
            _loop = loop;
            Current = null;
        }

        public bool MoveNext()
        {
            // first iteration case
            if (ReferenceEquals(Current, null))
            {
                Current = _begin;
                return !ReferenceEquals(Current, null)
                       && (_loop || !ReferenceEquals(Current, _exclusiveEnd));
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
            _begin = null;
            _exclusiveEnd = null;
        }
    }
}