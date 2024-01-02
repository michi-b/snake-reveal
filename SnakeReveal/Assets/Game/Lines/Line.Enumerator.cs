using System.Collections;
using System.Collections.Generic;

namespace Game.Lines
{
    public partial class Line
    {
        public struct Enumerator : IEnumerator<Line>
        {
            public Enumerator(Line start)
            {
                _start = start;
                Current = null;
            }

            private readonly Line _start;

            public bool MoveNext()
            {
                if (Current == null)
                {
                    Current = _start;
                    return Current != null;
                }

                if (Current.Next == null)
                {
                    return false;
                }

                Current = Current.Next;

                return Current != _start;
            }

            public void Reset()
            {
                Current = null;
            }

            public Line Current { get; private set; }

            object IEnumerator.Current => Current;

            public void Dispose()
            {
            }
        }
    }
}