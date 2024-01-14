using System;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace Game.Lines
{
    /// <summary>
    ///     Enumerates the lines from <see cref="_start" /> to <see cref="_end" /> in reverse order, yielding both and all
    ///     lines
    ///     in between.
    ///     It is imperative that the <see cref="_start" /> can be reached from the <see cref="_end" /> by following the
    ///     <see cref="Line.Next" /> property.
    /// </summary>
    /// <remarks>Skips Unity lifecycle checks and also other general null checks for performance reasons.</remarks>
    public struct ReverseLineEnumerator : IEnumerator<Line>
    {
        private Line _start;
        private Line _end;

        /// <summary>
        ///     Enumerates the lines from <see cref="start" /> to <see cref="end" /> in reverse order, yielding both and all lines
        ///     in between.
        ///     It is imperative that the <see cref="start" /> can be reached from the <see cref="end" /> by following the
        ///     <see cref="Line.Next" /> property.
        /// </summary>
        /// <param name="start">
        ///     The last Line this enumerator should yield
        /// </param>
        /// <param name="end">
        ///     The first Line this enumerator should yield
        /// </param>
        /// <remarks>Skips Unity lifecycle checks and also other general null checks for performance reasons.</remarks>
        public ReverseLineEnumerator(Line start, [CanBeNull] Line end)
        {
            if (start == null)
            {
                throw new ArgumentNullException(nameof(start));
            }

            if (end == null)
            {
                throw new ArgumentNullException(nameof(end));
            }

            _start = start;
            _end = end;
            Current = null;
        }

        public bool MoveNext()
        {
            // first iteration case
            if (ReferenceEquals(Current, null))
            {
                Current = _end;
                return true;
            }

            if (ReferenceEquals(Current, _start))
            {
                return false;
            }

            Current = Current.Previous;
            return true;
        }

        public void Reset()
        {
            Current = null;
        }

        public Line Current { get; private set; }

        object IEnumerator.Current => Current;

        public void Dispose()
        {
            _start = null;
            _end = null;
        }
    }
}