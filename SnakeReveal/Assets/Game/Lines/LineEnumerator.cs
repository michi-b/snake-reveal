using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace Game.Lines
{
    /// <summary>
    ///     Enumerates the lines from <see cref="_start" /> to <see cref="_end" />, yielding both and all lines in between.
    ///     It is imperative that the <see cref="_end" /> line can be reached from the <see cref="_start" /> line by following
    ///     the <see cref="Line.Next" />
    ///     property.
    /// </summary>
    public struct LineEnumerator : IEnumerator<Line>
    {
        private Line _start;
        private Line _end;

        /// <summary>
        ///     Enumerates the lines from <see cref="start" /> to <see cref="end" />, yielding both and all lines in between.
        ///     It is imperative that the <see cref="end" /> line can be reached from the <see cref="start" /> line by following
        ///     the <see cref="Line.Next" />
        ///     property.
        /// </summary>
        /// <param name="start">
        ///     The first Line this enumerator should yield
        /// </param>
        /// <param name="end">
        ///     The last Line this enumerator should yield
        /// </param>
        /// <remarks>Skips Unity lifecycle checks and also other general null checks for performance reasons.</remarks>
        public LineEnumerator(Line start, [CanBeNull] Line end)
        {
            _start = start;
            _end = end;
            Current = null;
        }

        public bool MoveNext()
        {
            // first iteration case
            if (ReferenceEquals(Current, null))
            {
                if (_start == null || _end == null)
                {
                    return false;
                }

                Current = _start;
                return true;
            }

            if (ReferenceEquals(Current, _end))
            {
                return false;
            }

            Current = Current.Next;
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