using System.Collections;
using System.Collections.Generic;

namespace Game.Lines
{
    public readonly struct SkipFirstLineEnumerable
    {
        private readonly Line _line;

        public SkipFirstLineEnumerable(Line line)
        {
            _line = line;
        }

        public SkipFirstEnumerator GetEnumerator()
        {
            return new SkipFirstEnumerator(_line);
        }

        public struct SkipFirstEnumerator : IEnumerator<Line>
        {
            private readonly Line _first;
            public Line Current { get; private set; }

            public SkipFirstEnumerator(Line first)
            {
                _first = first;
                Current = _first;
            }

            public bool MoveNext()
            {
                // empty case
                if (Current == null)
                {
                    return false;
                }

                Current = Current.Next;
                return Current != null && Current != _first;
            }

            public void Reset()
            {
                Current = _first;
            }

            object IEnumerator.Current => Current;

            public void Dispose()
            {
            }
        }
    }
}