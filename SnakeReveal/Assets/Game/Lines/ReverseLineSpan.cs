using System.Collections;
using System.Collections.Generic;
using Game.Enums;
using JetBrains.Annotations;

namespace Game.Lines
{
    public readonly struct ReverseLineSpan : IEnumerable<Line>
    {
        private readonly Line _start;
        private readonly Line _end;
        
        /// <inheritdoc cref="ReverseLineEnumerator(Line, Line)" />
        public ReverseLineSpan(Line start, [CanBeNull] Line end)
        {
            _start = start;
            _end = end;
        }

        public ReverseLineEnumerator GetEnumerator()
        {
            // ReSharper disable once Unity.NoNullPropagation
            return new ReverseLineEnumerator(_start, _end);
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