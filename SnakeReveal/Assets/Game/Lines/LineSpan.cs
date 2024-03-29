﻿using System.Collections;
using System.Collections.Generic;
using Game.Enums.Extensions;
using JetBrains.Annotations;

namespace Game.Lines
{
    /// <inheritdoc cref="LineEnumerator" />
    public readonly struct LineSpan : IEnumerable<Line>
    {
        private readonly Line _start;
        private readonly Line _end;

        /// <inheritdoc cref="LineEnumerator(Line, Line)" />
        public LineSpan(Line start, [CanBeNull] Line end)
        {
            _start = start;
            _end = end;
        }

        /// <inheritdoc cref="LineEnumerator(Line, Line)" />
        public LineEnumerator GetEnumerator() =>
            // ReSharper disable once Unity.NoNullPropagation
            new(_start, _end);

        IEnumerator<Line> IEnumerable<Line>.GetEnumerator() => GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public int SumClockwiseTurnWeight()
        {
            if (_start == _end)
            {
                return 0;
            }

            int result = 0;

            // ReSharper disable once ForeachCanBeConvertedToQueryUsingAnotherGetEnumerator
            // avoid LINQ to avoid boxing allocation
            foreach (Line line in new LineSpan(_start, _end.Previous))
            {
                Line next = line.Next;
                result += line.Direction.GetTurn(next!.Direction).GetClockwiseWeight();
            }

            return result;
        }
    }
}